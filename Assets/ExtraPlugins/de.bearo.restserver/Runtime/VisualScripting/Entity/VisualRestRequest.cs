#if RESTSERVER_VISUALSCRIPTING

using System.Threading;

namespace RestServer.VisualScripting {
    /// <summary>
    /// Represents all information needed for handling an incoming request in visual scripting.
    /// </summary>
    public class VisualRestRequest {
        public readonly RestRequest RestRequest;
        public readonly VisualEndpointDescription VisualEndpointDescription;
        /// <summary>
        /// Signal the rest server handler that the request has ended and return execution in the server.
        /// If there is no signal, then there will be a time out and a 500 response for the caller.
        /// </summary>
        public readonly AutoResetEvent WaitForEndRequest = new AutoResetEvent(false);
        
        /// <summary>
        /// Signal the rest server handler that the request has started processing inside the rendering loop.
        /// If there is no signal, then there will be a time out and a 500 response for the caller.
        /// </summary>
        public readonly AutoResetEvent WaitForRequestTriggered = new AutoResetEvent(false);

        public VisualRestRequest(RestRequest restRequest, VisualEndpointDescription visualEndpointDescription) {
            RestRequest = restRequest;
            VisualEndpointDescription = visualEndpointDescription;
        }
    }
}

#endif