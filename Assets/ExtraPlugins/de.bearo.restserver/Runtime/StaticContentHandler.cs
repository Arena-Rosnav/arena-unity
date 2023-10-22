using System;
using UnityEngine;

namespace RestServer {
    /// <summary>
    /// The content handler is called against end points that have been registered with StaticContentBuilder. It responds with the correct content for each
    /// request.
    /// </summary>
    public class StaticContentHandler {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        #region Content Management

        /// <summary>
        /// Register the content endpoint with the rest server.
        /// </summary>
        public virtual void RegisterContent(RestServer restServer, string path, byte[] byteContent, string contentType) {
            var entry = new StaticContentEntry(path, byteContent, null, contentType, this);

            restServer.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET,
                entry.Path, EndpointCallback, entry
            );
        }

        /// <summary>
        /// Register the content endpoint with the rest server.
        /// </summary>
        public virtual void RegisterContent(RestServer restServer, string path, string textContent, string contentType) {
            var entry = new StaticContentEntry(path, null, textContent, contentType, this);

            restServer.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET,
                entry.Path, EndpointCallback, entry
            );
        }

        /// <summary>
        /// Clear all content that was registered with this instance of the content handler.
        /// </summary>
        /// <param name="restServer"></param>
        public void ClearContent(RestServer restServer) {
            var endpoints = restServer.EndpointCollection.GetAllEndpoints(HttpMethod.GET);
            if (endpoints == null) {
                return; // no endpoints found
            }

            foreach (var endpoint in endpoints) {
                var tag = endpoint.Tag;
                if (tag == null) {
                    continue;
                }

                if (typeof(StaticContentEntry) != tag.GetType()) {
                    continue;
                }

                var content = (StaticContentEntry)tag;
                if (content.HandlerReference == this) {
                    restServer.EndpointCollection.RemoveEndpoint(HttpMethod.GET, endpoint.EndpointString);
                }
            }
        }

        #endregion

        /// <summary>
        /// Called from RestServer when a subpath matches an endpoint that has been created with this class.
        /// </summary>
        public void EndpointCallback(RestRequest request) {
            if (!request.Endpoint.HasValue) {
                // We have been called for a endpoint definition, but there is no endpoint for the request. Weird.
                throw new ArgumentNullException(nameof(request.Endpoint));
            }

            var endpoint = request.Endpoint.Value;
            var entry = endpoint.Tag as StaticContentEntry;
            if (entry == null) {
                throw new ArgumentNullException(nameof(request.Endpoint), "No static content found for request.");
            }

            SendContentResponse(request, entry);
        }

        protected virtual void SendContentResponse(RestRequest request, StaticContentEntry staticContent) {
            if (staticContent.IsBinary) {
                request.CreateResponse()
                    .Status(200)
                    .Body(staticContent.ByteContent, staticContent.ContentType)
                    .SendAsync();
            }
            else {
                request.CreateResponse()
                    .Body(staticContent.TextContent, staticContent.ContentType)
                    .SendAsync();
            }
        }
    }

    /// <summary>
    /// Used by the StaticContentHandler to set all necessary information on the endpoint to create a valid response.
    /// </summary>
    public class StaticContentEntry {
        /// <summary>
        /// Resolved path (rootPath + subPath) where this static content is located.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Byte array of the content to deliver to the caller. Can be null if the content is not binary (TextContent must then be not null)
        /// </summary>
        public readonly byte[] ByteContent;

        /// <summary>
        /// Text content to deliver to the caller. Can be null if the content is not text, but binary (ByteContent must then be not null)
        /// </summary>
        public readonly string TextContent;

        /// <summary>
        /// Content-Type Header to deliver to the caller.
        /// </summary>
        public readonly string ContentType;

        /// <summary>
        /// Reference to the creation class. Used when calling ClearContent on the Handler to determine if this Endpoint has been created by this handler
        /// instance.
        /// </summary>
        public readonly StaticContentHandler HandlerReference;


        /// <summary>
        /// True if the entry is binary and ByteContent shall be used.
        /// </summary>
        public bool IsBinary => ByteContent != null;

        public StaticContentEntry(string path, byte[] byteContent, string textContent, string contentType, StaticContentHandler handlerReference) {
            if ((byteContent != null && byteContent.Length > 0) && textContent != null) {
                throw new ArgumentException("Either byteContent or textContent can be provided, not both.");
            }

            if ((byteContent == null || byteContent.Length == 0) && textContent == null) { // empty text files are still valid
                throw new ArgumentException("Either byteContent or textContent must be provided.");
            }

            if (handlerReference == null) {
                throw new ArgumentNullException(nameof(handlerReference));
            }

            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(contentType)) {
                throw new ArgumentNullException(nameof(contentType));
            }

            Path = path;
            ByteContent = byteContent;
            TextContent = textContent;
            ContentType = contentType;
            HandlerReference = handlerReference;
        }
    }
}