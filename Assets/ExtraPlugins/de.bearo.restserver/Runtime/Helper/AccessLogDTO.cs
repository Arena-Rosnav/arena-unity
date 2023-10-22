using System.Net;

namespace RestServer.Helper {
    /// <summary>
    /// Data transfer object for incoming requests to generate an access log. WebSocket requests are not handled by this.
    /// </summary>
    public struct AccessLogDTO {
        /// <summary>
        /// IP address of the client
        /// </summary>
        public readonly IPAddress IPAddress;
        
        /// <summary>
        /// HTTP method used to make the request
        /// </summary>
        public readonly string HttpMethod;
        
        /// <summary>
        /// Request sub path of the request, doesn't include the base path of the server
        /// </summary>
        public readonly string RequestUri;
        
        /// <summary>
        /// Protocol used to call the server
        /// </summary>
        public readonly string Protocol;
        
        /// <summary>
        /// The http response status code the rest server responded with  
        /// </summary>
        public readonly int ResponseStatus;
        
        /// <summary>
        /// Milliseconds the server took to respond to the request
        /// </summary>
        public readonly long ResponseTimeMS;

        public AccessLogDTO(IPAddress ipAddress, string httpMethod, string requestUri, string protocol, int responseStatus, long responseTimeMS) {
            IPAddress = ipAddress;
            HttpMethod = httpMethod;
            RequestUri = requestUri;
            Protocol = protocol;
            ResponseStatus = responseStatus;
            ResponseTimeMS = responseTimeMS;
        }
    }
}