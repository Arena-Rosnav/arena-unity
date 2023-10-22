#if RESTSERVER_VISUALSCRIPTING

namespace RestServer.VisualScripting {
    /// <summary>Internal data class to collect all information needed to register an endpoint.</summary>
    public class VisualEndpointDescription {
        public readonly RestServer RestServer;
        public readonly HttpMethod HttpMethod;
        public readonly VisualEndpointType EndpointType;
        public readonly string EndpointPath;

        public VisualEndpointDescription(RestServer restServer, HttpMethod httpMethod, VisualEndpointType endpointType, string endpointPath) {
            RestServer = restServer;
            HttpMethod = httpMethod;
            EndpointType = endpointType;
            EndpointPath = endpointPath;
        }
    }
}

#endif