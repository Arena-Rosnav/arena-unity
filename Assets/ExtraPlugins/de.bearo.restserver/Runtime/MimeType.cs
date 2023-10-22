namespace RestServer {
    /// <summary>
    /// List of mime types for easier use in code. For documentation see references.
    ///
    /// https://en.wikipedia.org/wiki/Media_type or https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
    /// </summary>
    public static class MimeType {
    
        // If changing something here, also update the collection in the StaticContentAEInspector.ALL_MIME_TYPES.
        
        public const string APPLICATION_JAVASCRIPT = "application/javascript";
        public const string APPLICATION_JSON = "application/json";
        public const string APPLICATION_JSON_UTF_8 = "application/json; charset=UTF-8";
        public const string APPLICATION_XML = "application/xml";
        public const string APPLICATION_OCTET_STREAM = "application/octet-stream";

        public const string APPLICATION_PDF = "application/pdf";
        public const string APPLICATION_RTF = "application/rtf";
        public const string APPLICATION_XHTML = "application/xhtml+xml";

        public const string TEXT_CSV = "text/csv";
        public const string TEXT_HTML = "text/html";
        public const string TEXT_XML = "text/xml";
        public const string TEXT_PLAIN_UTF_8 = "text/plain; charset=UTF-8";
        public const string TEXT_PLAIN = "text/plain";
        public const string TEXT_CSS = "text/css";

        public const string IMAGE_JPEG = "image/jpeg";
        public const string IMAGE_PNG = "image/png";
        public const string IMAGE_TIFF = "image/tiff";
        public const string IMAGE_GIF = "image/gif";

        public const string MESSAGE_HTTP = "message/http";
    }
}