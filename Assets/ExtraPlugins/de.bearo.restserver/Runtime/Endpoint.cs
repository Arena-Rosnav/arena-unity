using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RestServer.WebSocket;

namespace RestServer {
    /// <summary>
    /// HTTP Endpoint description and it's handlers.
    /// </summary>
    public struct Endpoint {
        #region HTTP

        /// <summary>
        /// HTTP Method type to respond to (GET, POST, ...)
        /// </summary>
        public HttpMethod Method;

        /// <summary>
        /// Endpoint name this configuration should respond to. (e.g. /path).
        /// String matching is used to determine a match.
        ///
        /// This or <see cref="EndpointRegex"/> can be used.
        /// </summary>
        public string EndpointString;

        /// <summary>
        /// Regular expression used to determine if this endpoint matches. (e.g. /path.*)
        ///
        /// This or <see cref="EndpointString"/> can be used.
        /// </summary>
        public Regex EndpointRegex;

        /// <summary>
        /// Request handler, has always to send a response to the caller, otherwise the request times out.
        ///
        /// Please note to never call any Unity methods from this methods. It will result in very hard to debug
        /// threading problems. Most of the Unity methods are <b>not</b> thread-safe.
        /// </summary>
        public Action<RestRequest> RequestHandler;

        /// <summary>
        /// Dictionary of parsed path parameters. Only available if path parameters have been used in the endpoint.
        /// For example "/path/{param1}/path2/{param2}" will result in a dictionary with two entries.
        /// </summary>
        public Dictionary<string, PathParamDescription> PathParams;
        
        #endregion

        #region WebSocket

        /// <summary>
        /// The endpoint path allows the client to upgrade the connection to establish a WebSocket connection. The <see cref="RequestHandler"/> will not be
        /// called if the upgrade is requested and successful. If the upgrade is not requested the <see cref="RequestHandler"/> will be called.
        /// If the upgrade fails, an error is sent back to the client by default.
        /// </summary>
        public bool WebSocketUpgradeAllowed => WebSocketFrameHandler != null;

        /// <summary>
        /// Called if a new WebSocket frame has been received by any client
        /// </summary>
        public Action<WebsocketMessage> WebSocketFrameHandler;

        /// <summary>
        /// Internal websocket id that is used to distinguish websocket clients between different endpoints. 
        /// </summary>
        public WSEndpointId WsEndpointId;

        #endregion

        #region Observability

        /// <summary>
        /// Debug information: which method has been used in registering this endpoint. Only available in the editor.
        /// </summary>
        public string CodeLocation;

        /// <summary>
        /// Custom object that can be used to mark endpoints created by a specific algorithm. This is for example used to delete all endpoints created
        /// by an auto endpoint on disable.
        /// </summary>
        public object Tag;

        #endregion

        /// <summary>
        /// Check if the url matches this endpoint. Only checks the endpointUrl.
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <returns></returns>
        public bool CanHandle(string endpointUrl) {
            return EndpointRegex?.IsMatch(endpointUrl) == true
                   || endpointUrl.Equals(EndpointString, StringComparison.OrdinalIgnoreCase);
        }
    }

}