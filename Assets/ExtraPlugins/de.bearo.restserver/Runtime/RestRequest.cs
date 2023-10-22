using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using RestServer.Helper;
using RestServer.NetCoreServer;
using UnityEngine;

namespace RestServer {
    /// <summary>
    /// Describes the current request.
    /// </summary>
    public struct RestRequest {
        #region Private Variables

        private readonly Logger _logger;

        #endregion

        #region Public Variables

        /// <summary>
        /// Reference to the endpoint definition that describes this request, can be null if no endpoint definition is found.
        /// </summary>
        public Endpoint? Endpoint;

        /// <summary>
        /// Internal reference to the low level http session
        /// </summary>
        public readonly LowLevelSession Session;

        /// <summary>
        /// Reference to the underlying http request object,
        /// </summary>
        public readonly HttpRequest HttpRequest;

        /// <summary>
        /// Reference to the underlying http response object to craft responses yourself. For maximum compatibility, try to use the send methods in the RestRequest struct.
        /// </summary>
        public readonly HttpResponse HttpResponse;

        /// <summary>
        /// Calling Query Parameters
        /// </summary>
        public readonly NameValueCollection QueryParameters;

        /// <summary>
        /// Calling Query Parameters as easy to handle, read-only dictionary
        /// </summary>
        public readonly IDictionary<string, IList<string>> QueryParametersDict;

        /// <summary>
        /// Dictionary of parsed path parameters. Only available if path parameters have been used in the endpoint.
        /// For example "/path/{param1}/path2/{param2}" will result in a dictionary with two entries.
        /// </summary>
        public readonly IDictionary<string, PathParamValue> PathParams;

        /// <summary>
        /// Request Url(i) that has been called. This is allows for easier url parsing.
        /// </summary>
        /// <remarks>Note that the host part is always 'localhost' regardless which endpoint the caller has used.</remarks>
        public readonly Uri RequestUri;

        /// <summary>
        /// Holds information about what response has been sent. Used for debug logging.
        /// </summary>
        internal RestRequestResponseLog ResponseLog { get; }

        /// <summary>String contents of the request body.</summary>
        public string Body => HttpRequest.Body;

        /// <summary>Byte content of the request body.</summary>
        public byte[] BodyBytes => HttpRequest.BodyBytes;

        /// <summary>
        /// Read only dictionary of the http request's headers. Copy to non-read-only dictionary with <see cref="HeaderBuilder.DeepClone"/> in class <see cref="HeaderBuilder"/>.
        /// </summary>
        public IDictionary<string, IList<string>> Headers => RequestHeaderHelper.ToReadOnlyHeaderDict(HttpRequest);

        /// <summary>
        /// Helper used to handle internal redirects.
        /// </summary>
        public readonly HttpRequestRedirectHelper RedirectHelper;

        #endregion

        public RestRequest(LowLevelSession session,
            HttpRequest httpRequest,
            HttpResponse httpResponse,
            Uri requestUri,
            bool debugLog,
            RestRequestResponseLog responseLog,
            HttpRequestRedirectHelper redirectHelper,
            Dictionary<string, PathParamValue> pathParams) {
            HttpRequest = httpRequest;
            Session = session;
            HttpResponse = httpResponse;
            RequestUri = requestUri;
            Endpoint = null;

            var queryParameters = HttpUtility.ParseQueryString(requestUri.Query);
            QueryParameters = queryParameters;

            _logger = new Logger(Debug.unityLogger.logHandler);
            _logger.logEnabled = debugLog;

            ResponseLog = responseLog;

            var tempQP = new Dictionary<string, IList<string>>();
            foreach (var key in queryParameters.AllKeys) {
                var values = queryParameters.GetValues(key);
                if (values != null)
                    tempQP.Add(key, new ReadOnlyCollection<string>(values));
            }

            QueryParametersDict = new ReadOnlyDictionary<string, IList<string>>(tempQP);
            RedirectHelper = redirectHelper;
            if (pathParams == null) {
                PathParams = new ReadOnlyDictionary<string, PathParamValue>(new Dictionary<string, PathParamValue>());
            } else {
                PathParams = new ReadOnlyDictionary<string, PathParamValue>(pathParams);                
            }
        }

        #region OLD Response Methods

        /// <summary>
        /// Send a response for a get request async. Helper function. It's called "get" because the underlying framework does the same, but it can be used for other responses as well.
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().Body(content, contentType).SendAsync()")]
#endif
        public bool SendAsyncGetResponse(string content = "", string contentType = MimeType.TEXT_PLAIN_UTF_8) {
            DebugLogSendAsync("SendAsyncGetResponse", 200);

            var ret = SendResponseAsync(HttpResponse.MakeGetResponse(content, contentType));
            return ret;
        }

        /// <summary>
        /// Send an error (by default 500) to the callee async. 
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().StatusError().Body(content, contentType).SendAsync()")]
#endif
        public bool SendAsyncErrorResponse(int status = 500, string content = "", string contentType = MimeType.TEXT_PLAIN_UTF_8) {
            DebugLogSendAsync("SendAsyncErrorResponse", status);

            var ret = SendResponseAsync(HttpResponse.MakeErrorResponse(status, content, contentType));
            return ret;
        }

        /// <summary>
        /// Send a get response for a get request async. Helper function. It's called "get" because the underlying framework does the same, but it can be used for other responses as well.
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().BodyJson(obj, contentType).SendAsync()")]
#endif
        public bool SendAsyncGetJsonResponse(object obj, string contentType = MimeType.APPLICATION_JSON_UTF_8) {
            DebugLogSendAsync("SendAsyncGetJsonResponse", 200);

            var ret = SendAsyncGetResponse(JsonUtility.ToJson(obj), contentType);
            return ret;
        }

        /// <summary>
        /// Send a get response for a get request async. Helper function. It's called "get" because the underlying framework does the same, but it can be used for other responses as well.
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().Status(status).Body(obj, contentType).SendAsync()")]
#endif
        public bool SendAsyncGetJsonResponse(int status, object obj, string contentType = MimeType.APPLICATION_JSON_UTF_8) {
            DebugLogSendAsync("SendAsyncGetJsonResponse", status);

            var ret = SendAsyncGetResponse(status, new HeaderBuilder(HttpHeader.CONTENT_TYPE, contentType), JsonUtility.ToJson(obj));
            return ret;
        }

        /// <summary>
        /// Send a response async with specified status code, headers and content. It's called "get" because the underlying framework does the same, but it can be used for other responses as well.
        /// </summary>
        /// <param name="status">Status code to send back (default 200)</param>
        /// <param name="headers">List of headers to send to the client</param>
        /// <param name="content">Content</param>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().Status(status).Body(content).Headers(headers).SendAsync()")]
#endif
        public bool SendAsyncGetResponse(int status, Dictionary<string, List<string>> headers, string content) {
            DebugLogSendAsync("SendAsyncGetResponse", status);
            var response = new HttpResponse().SetBegin(status);

            if (headers != null) {
                foreach (var header in headers) {
                    foreach (var value in header.Value) {
                        response.SetHeader(header.Key, value);
                    }
                }
            }

            response.SetBody(content);

            var ret = SendResponseAsync(response);
            return ret;
        }

        /// <summary>
        /// Send a response async with specified status code, headers and content. It's called "get" because the underlying framework does the same, but it can be used for other responses as well.
        /// </summary>
        /// <param name="status">Status code to send back (default 200)</param>
        /// <param name="headers">List of headers to send to the client</param>
        /// <param name="content">Content</param>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().Status(status).Body(content).Headers(headers).SendAsync()")]
#endif
        public bool SendAsyncGetResponse(int status, Dictionary<string, List<string>> headers, byte[] content) {
            DebugLogSendAsync("SendAsyncGetResponse", status);
            var response = new HttpResponse().SetBegin(status);

            if (headers != null) {
                foreach (var header in headers) {
                    foreach (var value in header.Value) {
                        response.SetHeader(header.Key, value);
                    }
                }
            }

            response.SetBody(content);

            var ret = SendResponseAsync(response);
            return ret;
        }

        /// <summary>
        /// Send a simple, empty response async which contains only the status code.
        /// </summary>
        /// <param name="status">OK status (default is 200 (OK))</param>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().SendAsync()")]
#endif
        public bool SendAsyncOkResponse(int status = 200) {
            DebugLogSendAsync("SendAsyncOkResponse", status);

            var ret = SendResponseAsync(HttpResponse.MakeOkResponse(status));
            return ret;
        }

        /// <summary>
        /// Send a response async for a HEAD request (status code 200)
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().SendAsync()")]
#endif
        public bool SendAsyncHeadResponse() {
            DebugLogSendAsync("SendAsyncHeadResponse", 200);

            var ret = SendAsyncOkResponse(200);
            return ret;
        }

        /// <summary>
        /// Send a response async for a OPTIONS request
        /// </summary>
        /// <param name="allow">Allow methods (default is "HEAD,GET,POST,PUT,DELETE,OPTIONS,TRACE")</param>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete(
            "Use the new CreateResponse() Method to send a response: request.CreateResponse().Header(HttpHeader.ALLOW, \"HEAD,GET,POST,PUT,DELETE,OPTIONS,TRACE\").SendAsync()")]
#endif
        public bool SendAsyncOptionsResponse(string allow = "HEAD,GET,POST,PUT,DELETE,OPTIONS,TRACE") {
            DebugLogSendAsync("SendAsyncOptionsResponse", 200);

            var ret = SendResponseAsync(HttpResponse.MakeOptionsResponse(allow));
            return ret;
        }

        /// <summary>
        /// Send a response async for a TRACE request
        /// </summary>
        /// <param name="request">Request string content</param>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().Body(content, MimeType.MESSAGE_HTTP).SendAsync()")]
#endif
        public bool SendAsyncTraceResponse(string request) {
            DebugLogSendAsync("SendAsyncTraceResponse", 200);

            var ret = SendResponseAsync(HttpResponse.MakeTraceResponse(request));
            return ret;
        }

        /// <summary>
        /// Send a response async for a TRACE request
        /// </summary>
        /// <param name="request">Request binary content</param>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().Body(content, MimeType.MESSAGE_HTTP).SendAsync()")]
#endif
        public bool SendAsyncTraceResponse(byte[] request) {
            DebugLogSendAsync("SendAsyncTraceResponse", 200);

            var ret = SendResponseAsync(HttpResponse.MakeTraceResponse(request));
            return ret;
        }

        /// <summary>
        /// Send 500 (Internal server error) to the caller.
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().InternalServerError().SendAsync()")]
#endif
        public bool SendAsyncDefaultInternalServerError() {
            DebugLogSendAsync("SendAsyncDefaultInternalServerError", 500);

            var ret = SendAsyncGetResponse(500, null, "Internal server error");
            return ret;
        }

        /// <summary>
        /// Send 404 (Not found) to the caller.
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().NotFound().SendAsync()")]
#endif
        public bool SendAsyncDefaultNotFound() {
            DebugLogSendAsync("SendAsyncDefaultNotFound", 404);

            var ret = SendAsyncGetResponse(404, null, $"No endpoint found for {HttpRequest.Url} and Method {HttpRequest.Method}.");
            return ret;
        }

        /// <summary>
        /// Send 401 (Authorization required) to the caller.
        /// </summary>
        /// <returns>'true' if the current HTTP response was successfully sent, 'false' if the session is not connected</returns>
#if !DISABLE_OBSOLETE_WARNING_OLD_RESPONSE_METHODS
        [Obsolete("Use the new CreateResponse() Method to send a response: request.CreateResponse().NotAuthenticated().SendAsync()")]
#endif
        public bool SendAsyncDefaultNotAuthenticated() {
            DebugLogSendAsync("SendAsyncDefaultNotAuthenticated", 401);

            var ret = SendAsyncGetResponse(401, new HeaderBuilder(HttpHeader.WWW_AUTHENTICATE, "Basic"), "");
            return ret;
        }

        #endregion

        #region Response Method

        /// <summary>
        /// Response builder (new). Use this method to create a response to the call.
        /// </summary>
        public ResponseBuilder CreateResponse() {
            return new ResponseBuilder(this, _logger);
        }

        #endregion

        #region Profiling

        internal bool SendResponseAsync(HttpResponse response) {
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
            RestServerProfilerCounters.OutgoingBytesCount.Value += response.BodyLength;
#endif

            var ret = Session.SendResponseAsync(response);
            ResponseLog.MarkSent(response.Status);
            return ret;
        }

        #endregion

        #region Internal Redirection

        /// <summary>
        /// After the this endpoint method has finished, redirect the endpoint to another endpoint. This is useful for 404 redirects, for example.
        /// The redirected endpoint will receive all information from the original request.
        /// </summary>
        /// <remarks>
        /// Make sure that there is only one response sent back to the caller.
        /// </remarks>
        /// <param name="redirectPath">Absolute path to redirect to</param>
        /// <param name="ignoreTag">Endpoint.Tag to ignore when trying to find the next endpoint to redirect to</param>
        public void ScheduleInternalRedirect(string redirectPath, object ignoreTag = null) {
            if (RedirectHelper == null) {
                throw new ArgumentException("Internal redirection is not supported on this request.");
            }

            RedirectHelper.InternalRedirectPath = redirectPath;
            RedirectHelper.IgnoreTag = ignoreTag;
        }

        /// <summary>
        /// Clears the information that this request should be redirected. Called automatically by the rest server library. 
        /// </summary>
        public void ClearInternalRedirect() {
            if (RedirectHelper == null) {
                throw new ArgumentException("Internal redirection is not supported on this request.");
            }

            RedirectHelper.InternalRedirectPath = null;
            RedirectHelper.IgnoreTag = null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper function to parse the request body to json.
        /// </summary>
        public T JsonBody<T>() {
            return JsonUtility.FromJson<T>(HttpRequest.Body);
        }

        #endregion

        #region Internal Debugging

        private void DebugLogSendAsync(string method, int status, string message = "") {
            if (!_logger.logEnabled) {
                return;
            }

            // var frame = (new System.Diagnostics.StackTrace()).GetFrame(3);
            // var type = frame.GetMethod().DeclaringType;
            // var caller = frame.GetMethod().Name;
            _logger.Log($"{method} with {status}.");
        }

        #endregion
    }
}