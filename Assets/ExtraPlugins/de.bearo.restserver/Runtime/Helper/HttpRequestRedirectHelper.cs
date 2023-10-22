namespace RestServer {
    /// <summary>Keep track of internal redirects</summary>
    public class HttpRequestRedirectHelper {
        /// <summary>
        /// Path to redirect to. Do not modify directly use RestRequest#ScheduleInternalRedirect
        /// </summary>
        public string InternalRedirectPath = null;

        /// <summary>
        /// Tag to ignore when finding the next endpoint. Do not modify directly use RestRequest#ScheduleInternalRedirect
        /// Only used when InternalRedirectPath is set.
        /// </summary>
        public object IgnoreTag = null;

        /// <summary>
        /// Original Path.
        /// </summary>
        public readonly string OriginalPath;

        /// <summary>
        /// Count of internal redirections; Do not modify directly.
        /// </summary>
        public int RedirectCount = 0;

        public HttpRequestRedirectHelper(string originalPath) {
            OriginalPath = originalPath;
        }
    }
}