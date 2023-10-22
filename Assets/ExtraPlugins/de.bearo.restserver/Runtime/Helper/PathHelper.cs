namespace RestServer.Helper {
    public static class PathHelper {
        
        /// <summary>
        /// Ensures that the path starts with a slash
        /// </summary>
        /// <param name="s">A valid url sub path without the protocol, host and port</param>
        public static string EnsureSlashPrefix(string s) {
            if (string.IsNullOrEmpty(s)) {
                return "/";
            }

            if (!s.StartsWith("/")) {
                return "/" + s;
            }

            return s;
        }

        /// <summary>
        /// Removes the trailing slash from a path
        /// </summary>
        /// <param name="s">A valid url sub path without the protocol, host and port</param>
        public static string RemoveEndingSlash(string s) {
            if (string.IsNullOrEmpty(s)) {
                return "";
            }

            return s.EndsWith("/") ? s.Remove(s.Length - 1) : s;
        }

        /// <summary>
        /// Concatenates two paths, ensuring that there is only one slash between them
        /// </summary>
        /// <param name="a">A valid url sub path without the protocol, host and port</param>
        /// <param name="b">A valid url sub path without the protocol, host and port</param>
        public static string ConcatPath(string a, string b) {
            if (string.IsNullOrEmpty(b)) {
                return a; // prevent trailing slash if b is empty
            }

            return RemoveEndingSlash(a) + EnsureSlashPrefix(b);
        }
    }
}