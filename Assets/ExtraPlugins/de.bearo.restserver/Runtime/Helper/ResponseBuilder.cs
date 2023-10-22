using System;
using System.Collections.Generic;
using RestServer.NetCoreServer;
using UnityEngine;

namespace RestServer.Helper {
    /// <summary>
    /// Response builder that dynamically builds a response with the given options. Use SendAsync() to send the response to the caller. 
    /// </summary>
    public class ResponseBuilder {
        private readonly Logger _logger;
        
        private int _status;
        private readonly RestRequest _request;
        private readonly HeaderBuilder _header = new HeaderBuilder();
        private bool _internalRedirectScheduled;

        private ResponseBuilderBodyHelper _body;

        public ResponseBuilder(RestRequest request, Logger logger) {
            _status = 200;
            _request = request;
            _logger = logger;
        }

        #region Define Response Parameters

        /// <summary>
        /// Set status code to 500
        /// </summary>
        public ResponseBuilder StatusError() {
            return Status(500);
        }

        /// <summary>
        /// Convert object to json and send it with the given content type to the caller. This call overwrites the contentType header.
        /// </summary>
        public ResponseBuilder BodyJson(object obj, string contentType = MimeType.APPLICATION_JSON_UTF_8) {
            _header.withOverwriteHeader(HttpHeader.CONTENT_TYPE, contentType);
            _body = new ResponseBuilderBodyHelper(JsonUtility.ToJson(obj));
            return this;
        }

        /// <summary>
        /// Send the string body back to the caller with the given content type. This call overwrites the contentType header.
        /// </summary>
        public ResponseBuilder Body(string body, string contentType = MimeType.TEXT_PLAIN_UTF_8) {
            _header.withOverwriteHeader(HttpHeader.CONTENT_TYPE, contentType);
            _body = new ResponseBuilderBodyHelper(body);
            return this;
        }   

        /// <summary>
        /// Send the binary body to the caller with the given content type. This call overwrites the contentType header.
        /// </summary>
        public ResponseBuilder Body(byte[] body, string contentType = MimeType.APPLICATION_OCTET_STREAM) {
            _header.withOverwriteHeader(HttpHeader.CONTENT_TYPE, contentType);
            _body = new ResponseBuilderBodyHelper(body);
            return this;
        }

        /// <summary>
        /// Add the header with the given key and value and overwrite existing values. Use HttpHeader.XXX as key
        /// </summary>
        public ResponseBuilder Header(string name, string value) {
            _header.withOverwriteHeader(name, value);
            return this;
        }

        /// <summary>
        /// Add all given headers and overwrite existing values. Use HeaderBuilder class to construct the dictionary.
        /// </summary>
        public ResponseBuilder Headers(Dictionary<string, List<string>> headers) {
            foreach (var header in headers) {
                _header.withOverwriteHeader(header.Key, header.Value);
            }

            return this;
        }

        /// <summary>
        /// Redirect this call internally to another endpoint implementation. Does not return to the client. A response
        /// to the client must still be sent from the endpoint redirected to.
        /// </summary>
        public void ScheduleInternalRedirect(string endpointPath) {
            if (string.IsNullOrEmpty(endpointPath)) {
                throw new ArgumentNullException(nameof(endpointPath));
            }

            _request.RedirectHelper.InternalRedirectPath = endpointPath;
            _internalRedirectScheduled = true;
        }

        /// <summary>
        /// Set the status code of this response to the given number.
        /// </summary>
        public ResponseBuilder Status(int statusCode) {
            if (statusCode < 100 || statusCode > 900) {
                throw new ArgumentOutOfRangeException(nameof(statusCode));
            }

            _status = statusCode;

            return this;
        }

        #endregion

        #region Default Use Cases

        /// <summary>
        /// Sets status and body to a default internal server error message. (500, Internal Server Error).
        /// </summary>
        public ResponseBuilder InternalServerError(string body = "Internal server error") {
            return Status(500).Body(body);
        }

        /// <summary>
        /// Sets status and body to a default not found message. (404).
        /// </summary>
        public ResponseBuilder NotFound(string message = null) {
            if (string.IsNullOrEmpty(message)) {
                message = $"No endpoint found for {_request.HttpRequest.Url} and Method {_request.HttpRequest.Method}.";
            }

            return Status(404).Body(message);
        }

        /// <summary>
        /// Sets status and body to a default not authenticated message with the appropriate header. (401, WWW-Authenticate).
        /// </summary>
        public ResponseBuilder NotAuthenticated(string realm = "Basic") {
            return Status(401).Header(HttpHeader.WWW_AUTHENTICATE, realm);
        }

        #endregion

        #region Send Methods

        /// <summary>
        /// Send the response back to the caller asynchronously. Immediately returns without waiting for the response to be sent.
        /// </summary>
        /// <returns>True if the response was enqueued; false otherwise.</returns>
        public bool SendAsync() {
            if (_internalRedirectScheduled) {
                throw new SystemException("Internal redirect scheduled with this response builder, can't send response with this builder.");
            }

            var response = CreateResponse();
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
            RestServerProfilerCounters.OutgoingBytesCount.Value += response.BodyLength;
#endif

            var ret = _request.Session.SendResponseAsync(response);
            _request.ResponseLog.MarkSent(response.Status);
            
            if (_logger.logEnabled) {
                _logger.Log($"Send async response with {_status}.");
            }

            return ret;
        }

        private HttpResponse CreateResponse() {
            var r = new HttpResponse();
            r.SetBegin(_status);

            foreach (var header in (Dictionary<string, List<string>>)_header) {
                foreach (var value in header.Value) {
                    r.SetHeader(header.Key, value);
                }
            }

            if (_body != null) {
                _body.Apply(r);
            }
            else {
                r.SetBody();
            }

            return r;
        }

        #endregion
    }

    /// <summary>
    /// Internal class to easily switch between various body data types.
    /// </summary>
    internal class ResponseBuilderBodyHelper {
        private readonly string _bodyString;
        private readonly byte[] _bodyBytes;

        public ResponseBuilderBodyHelper(string bodyString) {
            _bodyString = bodyString;
            _bodyBytes = null;
        }

        public ResponseBuilderBodyHelper(byte[] bodyBytes) {
            _bodyString = null;
            _bodyBytes = bodyBytes;
        }

        public void Apply(HttpResponse response) {
            if (_bodyBytes != null) {
                response.SetBody(_bodyBytes);
            }
            else if (_bodyString != null) {
                response.SetBody(_bodyString);
            }
            else {
                response.SetBody();
            }
        }
    }
}