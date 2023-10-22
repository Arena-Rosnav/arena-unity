using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using RestServer.Helper;
using RestServer.NetCoreServer;
using UnityEngine;

namespace RestServer {
    public class LowLevelSession : WsSession {
        #region Properties

        private readonly SpecialHandlers _specialHandlers;
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        private Endpoint? _endpoint;

        /// <summary>
        /// Enable debug logging on this session.
        ///
        /// Please set via RestServer.debugLog. Leaving this open for users that want to modify or have a separate RestServer implementation.
        /// </summary>
        public bool DebugLog {
            get => _logger.logEnabled;
            set => _logger.logEnabled = value;
        }

        #endregion

        #region Constructor

        public LowLevelSession(LowLevelServer server, SpecialHandlers specialHandlers, bool debugLog = false) : base(server) {
            if (!(Server is LowLevelServer)) {
                throw new ArgumentException("Server is not of type LowLevelHttpServer.");
            }

            _specialHandlers = specialHandlers;
            DebugLog = debugLog;
        }

        #endregion

        #region HTTP

        private Uri GetRequestUri(HttpRequest request) {
            return new Uri("http://localhost" + request.Url);
        }


        protected override void OnReceivedRequest(HttpRequest request) {
            if (WebSocket.WsHandshaked) {
                return; // Ignore HTTP requests after WebSocket handshake
            }
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var server = Server as LowLevelServer;
            if (_logger.logEnabled) {
                _logger.Log("Received request: " + request);
            }
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
            RestServerProfilerCounters.IncomingBytesCount.Value += request.BodyLength;
            RestServerProfilerCounters.IncomingRequestsCount.Value += 1;
#endif

            var requestUri = GetRequestUri(request);

            var responseLog = new RestRequestResponseLog();
            var redirectHelper = new HttpRequestRedirectHelper(requestUri.AbsolutePath);

            Dictionary<string, PathParamValue> pathParams = null;
            if (_endpoint.HasValue) {
                pathParams = PathParamHelper.ParseRequestUri(requestUri, _endpoint.Value.PathParams);
            }
            
            var requestAndResponse = new RestRequest(this, request, Response, requestUri, DebugLog, responseLog, redirectHelper, pathParams);

            if (_specialHandlers?.AuthHandler != null && !_specialHandlers.AuthHandler.Invoke(requestAndResponse)) {
                _logger.Log("Request denied by auth handler");
                return;
            }

            OnReceivedRequestExecuteHandler(request, server, requestUri, requestAndResponse);

            if (_logger.logEnabled && !responseLog.ResponseSent) {
                _logger.LogWarning(null, $"The request has not been terminated with a response to the client. The client will timeout. Request: {request}");
            }

            var accessLog = new AccessLogDTO((Socket.RemoteEndPoint as IPEndPoint)?.Address, request.Method, request.Url, request.Protocol,
                responseLog.ResponseStatus, sw.ElapsedMilliseconds);
            _specialHandlers?.AccessLog?.Invoke(accessLog);
        }

        protected virtual void OnReceivedRequestExecuteHandler(HttpRequest request, LowLevelServer server, Uri requestUri, RestRequest requestAndResponse) {
            var endpointPath = requestUri.AbsolutePath;
            object endpointPathIgnoreTag = null;
            var redirect = requestAndResponse.RedirectHelper;
            
            while (true) {
                // resolve endpoint again, because it might have changed due to internal redirect
                var endpoint = server.FindEndpoint(HttpMethodFrom(request.Method), endpointPath, endpointPathIgnoreTag);

                if (endpoint.HasValue) {
                    if (_logger.logEnabled) {
                        _logger.Log($"Endpoint found for request: {request}");
                    }

                    if (endpoint.Value.WebSocketUpgradeAllowed) { // websocket upgrade was allowed, but we are in the HTTP path --> no ws request or failed upgrade
                        _specialHandlers?.NoWsUpgradeRequest?.Invoke(requestAndResponse);
                        return;
                    }

                    try {
                        var ev = endpoint.Value;
                        requestAndResponse.Endpoint = ev;
                        ev.RequestHandler(requestAndResponse);
                    }
                    catch (Exception e) {
                        _logger.Log($"Error while handling request {request}: {e}");
                        _specialHandlers?.EndpointErrorHandler?.Invoke(requestAndResponse, e);
                    }
                }
                else {
                    if (_logger.logEnabled) {
                        _logger.Log($"No endpoint found for request: {request}");
                    }

                    _specialHandlers?.NoEndpointFoundHandler?.Invoke(requestAndResponse);
                }

                if (!string.IsNullOrEmpty(redirect.InternalRedirectPath)) {
                    endpointPath = redirect.InternalRedirectPath;
                    endpointPathIgnoreTag = redirect.IgnoreTag;
                    requestAndResponse.ClearInternalRedirect();
                    _logger.Log($"Internal redirect to {endpointPath}");

                    if (redirect.RedirectCount++ > 50) {
                        _logger.LogError(this.GetType().Name, $"Internal maximum redirection amount reached for request: {request}.");
                        _specialHandlers?.EndpointErrorHandler?.Invoke(requestAndResponse, new SystemException("Too many internal redirects."));
                        return;
                    }
                }
                else {
                    return;
                }
            }
        }


        protected HttpMethod? HttpMethodFrom(string s) {
            if (Enum.TryParse(s, true, out HttpMethod result)) return result;

            return null;
        }

        #endregion

        #region WebSocket

        protected override void OnReceivedRequestHeader(HttpRequest request) {
            // try to find the correct endpoint and validate if we are allowed to upgrade to websocket or not
            var server = Server as LowLevelServer;
            var requestUri = GetRequestUri(request);
            var endpointPath = requestUri.AbsolutePath;
            _endpoint = server.FindEndpoint(HttpMethodFrom(request.Method), endpointPath);

            var allowUpgrade = _endpoint.HasValue && _endpoint.Value.WebSocketUpgradeAllowed;

            if (allowUpgrade && _specialHandlers != null && _specialHandlers.AllowWsUpgradeRequest != null) {
                allowUpgrade = _specialHandlers.AllowWsUpgradeRequest.Invoke(CreateWsTinyRequestAndResponse(request));
            }
            
            DoOnReceivedRequestHeaderWithUpgrade(request, allowUpgrade);
        }

        public override bool OnWsConnecting(HttpRequest request, HttpResponse response) {
            var requestAndResponse = CreateWsTinyRequestAndResponse(request);

            // The upgrade request would be successful and can be denied by sending a custom response and return false
            if (_specialHandlers != null && _specialHandlers.WsAuthHandler != null) {
                return _specialHandlers.WsAuthHandler.Invoke(requestAndResponse);
            }

            return base.OnWsConnecting(request, response);
        }

        private RestRequest CreateWsTinyRequestAndResponse(HttpRequest request) {
            var requestUri = GetRequestUri(request);

            var responseLog = new RestRequestResponseLog();
            var requestAndResponse = new RestRequest(this, request, Response, requestUri, DebugLog, responseLog, null, null);
            return requestAndResponse;
        }

        public override void OnWsConnected(HttpRequest request) {
            // response has been sent, client connected
            base.OnWsConnected(request);

            var server = Server as LowLevelServer;
            if (_endpoint.HasValue) {
                server.RegisterWsSession(_endpoint.Value.WsEndpointId, this);
            }
        }

        public override void OnWsDisconnected() {
            base.OnWsDisconnected();

            var server = Server as LowLevelServer;
            if (_endpoint.HasValue) {
                server.DeregisterWsSession(_endpoint.Value.WsEndpointId, this);
            }
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size) {
            base.OnWsReceived(buffer, offset, size);

            if (_endpoint.HasValue) {
                try {
                    _endpoint.Value.WebSocketFrameHandler(new WebsocketMessage(buffer, this));    
                }catch (Exception e) {
                    Debug.LogError($"Error while handling websocket frame <{e}> see next log message for stacktrace.");
                }
            }
        }

        public override void OnWsClose(byte[] buffer, long offset, long size) {
            base.OnWsClose(buffer, offset, size);

            var server = Server as LowLevelServer;
            if (_endpoint.HasValue) {
                server.DeregisterWsSession(_endpoint.Value.WsEndpointId, this);
            }
        }

        // public override void OnWsError(string error) {
        //     base.OnWsError(error);
        // }
        //
        // public override void OnWsError(SocketError error) {
        //     base.OnWsError(error);
        // }

        #endregion

        #region Error Handling

        protected override void OnReceivedRequestError(HttpRequest request, string error) {
            if (_logger.logEnabled) {
                _logger.Log($"OnReceivedRequestError. Error: {error} on request: {request}.");
            }

            _specialHandlers?.LowLevelOnReceivedRequestError?.Invoke(request, error);
        }

        protected override void OnError(SocketError error) {
            if (_logger.logEnabled) {
                _logger.Log($"OnError. Error: {error}.");
            }

            _specialHandlers?.LowLevelOnError?.Invoke(error);
        }

        #endregion
    }
}