namespace RestServer.Helper {
    /// <summary>
    ///     Holds status information about the response sent from RestRequest. Useful for internal logging.
    /// </summary>
    public class RestRequestResponseLog {
        public RestRequestResponseLog() {
            ResponseSent = false;
            ResponseStatus = -1;
        }

        /// <summary>
        ///     Has the response already been sent?
        /// </summary>
        public bool ResponseSent { get; set; }

        /// <summary>
        ///     What response status code has already been sent back? -1 if no response has been sent yet.
        /// </summary>
        public int ResponseStatus { get; set; }
        
        public void MarkSent(int status) {
            ResponseSent = true;
            ResponseStatus = status;
        }
    }
}