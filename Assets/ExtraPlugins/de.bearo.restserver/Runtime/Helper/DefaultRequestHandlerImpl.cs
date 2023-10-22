using System;
using System.Net.Sockets;
using RestServer.NetCoreServer;
using UnityEngine;

namespace RestServer.Helper {
    /// <summary>
    ///     Default handlers for easy usage. See descriptions on each handler for details.
    /// </summary>
    public static class DefaultRequestHandlerImpl {
        /// <summary>
        ///     Default exception handler. Logs the exception and sends 500 back to the caller.
        /// </summary>
        /// <param name="request">Reference to the current request</param>
        /// <param name="exception">Exception thrown by the endpoint implementation</param>
        public static void RequestEndpointExceptionHandler(RestRequest request, Exception exception) {
            if (exception != null) {
                Debug.LogError(exception);
            }

            request.CreateResponse()
                .InternalServerError()
                .SendAsync();
        }

        /// <summary>
        ///     Default no endpoint found handler. Sends 404 back to the caller.
        /// </summary>
        /// <param name="request"></param>
        public static void NoEndpointFoundHandler(RestRequest request) {
            request.CreateResponse()
                .NotFound()
                .SendAsync();
        }

        /// <summary>
        ///     Default authentication handler. Allows all endpoints to be called.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>True if authenticated (always); false if not (This implementation does never return false.)</returns>
        public static bool AllowAllAuthHandler(RestRequest request) {
            return true;
        }

        /// <summary>
        ///     Handle HTTP request error notification from the low level library.
        /// </summary>
        /// <remarks>Notification is called when HTTP request error was received from the client.</remarks>
        /// <param name="request">HTTP request</param>
        /// <param name="error">HTTP request error</param>
        public static void DefaultLowLevelOnOnReceivedRequestError(HttpRequest request, string error) {
            Debug.LogError($"Request error: {error}");
        }

        /// <summary>
        ///     Handle error notification from the low level library.
        /// </summary>
        /// <param name="error">Socket error code</param>
        public static void DefaultLowLevelOnError(SocketError error) {
            Debug.LogError($"HTTP session caught an error: {error}");
        }

        /// <summary>
        ///     Logs passed exceptions that occur in asynchronous execution of the ThreadingHelper.
        /// </summary>
        /// <param name="exception">Caught exception</param>
        public static void AsyncLogExceptionHandler(Exception exception) {
            Debug.LogError(exception);
        }

        /// <summary>
        ///     Do not log anything.
        /// </summary>
        /// <param name="accessLog"></param>
        public static void NoOpAccessLog(AccessLogDTO accessLog) {
            // NOP
        }

        /// <summary>
        ///     Default debug log functionality, outputs an apache inspired format.
        /// </summary>
        /// <param name="accessLog"></param>
        public static void DebugLogAccessLog(AccessLogDTO accessLog) {
            // 127.0.0.1 - frank [10/Oct/2000:13:55:36 -0700] "GET /apache_pb.gif HTTP/1.0" 200 2326
            var time = DateTime.Now.ToString("dd/MM/yyyy:HH:mm:ss zzz");
            Debug.Log($"{accessLog.IPAddress} - - [{time}] \"{accessLog.HttpMethod} " +
                      $"{accessLog.RequestUri} {accessLog.Protocol}\" {accessLog.ResponseStatus} {accessLog.ResponseTimeMS}ms");
        }

        /// <summary>
        /// Send a 400 response if a non-websocket request is received on a websocket endpoint.
        /// </summary>
        /// <param name="request"></param>
        public static void NoWsUpgradeRequest(RestRequest request) {
            request.CreateResponse().Status(400).Body("No websocket upgrade request").SendAsync();
        }

        /// <summary>
        /// Allow all websocket upgrade requests when they originate from localhost.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool AllowWsUpgradeRespectingOrigin(RestRequest request) {
            if (!request.Headers.TryGetValue(HttpHeader.ORIGIN, out var origins))
                return true;

            if (origins.Count == 0) {
                Debug.Log("No origin header found, denying websocket upgrade");
                return false; // no origin provided 
            }

            var origin = origins[0];
            if (!origin.StartsWith("http://localhost")) {
                Debug.Log($"Origin {origin} is not allowed, denying websocket upgrade. Update this code if other origins are needed");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Allow all websocket upgrade requests regardless of the origin.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool AllowWsUpgradeAlways(RestRequest request) {
            return true;
        }
    }
}