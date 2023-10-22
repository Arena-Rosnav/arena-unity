namespace RestServer {
    
    /// <summary>
    /// List of http headers for easier use in code. For documentation see references.
    ///
    /// https://en.wikipedia.org/wiki/List_of_HTTP_header_fields or https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
    /// </summary>
    public static class HttpHeader {

        /// <summary>Valid methods for a specified resource. To be used for a 405 Method not allowed.</summary>
        public const string ALLOW = "allow";
        
        /// <summary>CORS relevant header</summary>
        public const string ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";

        /// <summary>CORS relevant header</summary>
        public const string ACCESS_CONTROL_ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";

        /// <summary>CORS relevant header</summary>
        public const string ACCESS_CONTROL_EXPOSE_HEADERS = "Access-Control-Expose-Headers";

        /// <summary>CORS relevant header</summary>
        public const string ACCESS_CONTROL_MAX_AGE = "Access-Control-Max-Age";

        /// <summary>CORS relevant header</summary>
        public const string ACCESS_CONTROL_ALLOW_METHODS = "Access-Control-Allow-Methods";

        /// <summary>CORS relevant header</summary>
        public const string ACCESS_CONTROL_ALLOW_HEADERS = "Access-Control-Allow-Headers";

        /// <summary>Tells all caching mechanisms from server to client whether they may cache this object. It is measured in seconds</summary>
        public const string CACHE_CONTROL = "Cache-Control";

        /// <summary>An opportunity to raise a "File Download" dialogue box for a known MIME type with binary format or suggest a filename for dynamic content. Quotes are necessary with special characters.</summary>
        public const string CONTENT_DISPOSITION = "Content-Disposition";

        /// <summary>The type of encoding used on the data.</summary>
        public const string CONTENT_ENCODING = "Content-Encoding";

        /// <summary>The length of the response body in octets (8-bit bytes)</summary>
        public const string CONTENT_LENGTH = "Content-Length";

        /// <summary>The MIME type of this content</summary>
        public const string CONTENT_TYPE = "Content-Type";

        /// <summary>An identifier for a specific version of a resource, often a message digest or a hash</summary>
        public const string E_TAG = "ETag";

        /// <summary>The last modified date for the requested object (in "HTTP-date" format as defined by RFC 7231)</summary>
        public const string LAST_MODIFIED = "Last-Modified";

        /// <summary>An HTTP cookie</summary>
        public const string SET_COOKIE = "Set-Cookie";

        /// <summary>Indicates the authentication scheme that should be used to access the requested entity. (For example "WWW-Authenticate: Basic")</summary>
        public const string WWW_AUTHENTICATE = "WWW-Authenticate";

        /// <summary>The Origin request header indicates the origin (scheme, hostname, and port) that caused the request.</summary>
        public const string ORIGIN = "Origin";
    }
}