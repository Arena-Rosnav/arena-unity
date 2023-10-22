namespace RestServer {
    /// <summary>
    /// Supported HTTP Methods
    /// </summary>
    public enum HttpMethod {
        HEAD,
        GET,
        POST,
        PUT,
        PATCH,
        DELETE,
        OPTIONS
    }

    public class HttpMethodExtension {
        public static HttpMethod[] All() {
            return new HttpMethod[] {
                HttpMethod.HEAD,
                HttpMethod.GET,
                HttpMethod.POST,
                HttpMethod.PUT,
                HttpMethod.PATCH,
                HttpMethod.DELETE,
                HttpMethod.OPTIONS
            };
        }
    }
}