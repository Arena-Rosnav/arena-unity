using System;
using System.Net.Sockets;
using RestServer.Helper;
using RestServer.NetCoreServer;

namespace RestServer {
    public class SpecialHandlers {
        /// <summary>
        /// Handler which is executed upon every request and which can decide if a request is handled or not. Useful to implement authentication handlers.
        /// This method should return true if the request is allowed to be processed further by any endpoint code.
        /// If false the endpoint will not be called. In that case the handler is tasked with sending a 401 authorized back to the caller.
        /// </summary>
        /// <returns>True if the request can be handed to the endpoint code, false if the request can't.</returns>
        public Func<RestRequest, bool> AuthHandler = DefaultRequestHandlerImpl.AllowAllAuthHandler;

        /// <summary>
        /// Handler which is called when the endpoint handler throws an exception. The default implementation returns an error with status 500 (Internal Server Error).
        /// The implementation is tasked to send this information back to the caller.
        /// </summary>
        public Action<RestRequest, Exception> EndpointErrorHandler = DefaultRequestHandlerImpl.RequestEndpointExceptionHandler;

        /// <summary>
        /// Handler which is called when no endpoint could be found. The default implementation returns 404 to the caller.
        /// The implementation is tasked to send this information back to the caller. 
        /// </summary>
        public Action<RestRequest> NoEndpointFoundHandler = DefaultRequestHandlerImpl.NoEndpointFoundHandler;

        /// <summary>
        /// Handle HTTP request error notification from the low level library.
        /// </summary>
        /// <remarks>Notification is called when HTTP request error was received from the client.</remarks>
        /// <param name="request">HTTP request</param>
        /// <param name="error">HTTP request error</param>
        public Action<HttpRequest, string> LowLevelOnReceivedRequestError = DefaultRequestHandlerImpl.DefaultLowLevelOnOnReceivedRequestError;

        /// <summary>
        /// Handle error notification from the low level library.
        /// </summary>
        /// <param name="error">Socket error code</param>
        public Action<SocketError> LowLevelOnError = DefaultRequestHandlerImpl.DefaultLowLevelOnError;

        /// <summary>
        /// Handles Exceptions are thrown by the Endpoint code, this caller is used to handle them. They can't be passed
        /// back to the calling code, as the request could already be finished. 
        /// </summary>
        public Action<Exception> AsynchronousExceptionHandler = DefaultRequestHandlerImpl.AsyncLogExceptionHandler;

        /// <summary>
        /// Handler which can be used to write an access log
        /// </summary>
        public Action<AccessLogDTO> AccessLog = DefaultRequestHandlerImpl.NoOpAccessLog;

        /// <summary>
        /// Handler executed when a non-websocket upgrade request is sent to a websocket endpoint. The default implementation returns 400 to the caller.
        /// </summary>
        public Action<RestRequest> NoWsUpgradeRequest = DefaultRequestHandlerImpl.NoWsUpgradeRequest;
        
        /// <summary>
        /// Handler executed to determine if a websocket upgrade request is allowed. The default implementation returns true.
        /// </summary>
        public Func<RestRequest, bool> AllowWsUpgradeRequest = DefaultRequestHandlerImpl.AllowWsUpgradeRespectingOrigin;

        /// <summary>
        /// Authenticate a websocket upgrade request. The default implementation returns true.
        /// </summary>
        public Func<RestRequest, bool> WsAuthHandler = DefaultRequestHandlerImpl.AllowAllAuthHandler;
    }
}