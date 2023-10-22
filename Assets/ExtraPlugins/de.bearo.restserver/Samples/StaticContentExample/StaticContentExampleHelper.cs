using System;
using RestServer;
using RestServer.Helper;
using UnityEngine;

namespace de.bearo.restserver.Samples.StaticContentExample {
    
    /// <summary>
    /// Backend implementation for the functionality exposed in the frontend.
    /// </summary>
    public class StaticContentExampleHelper : MonoBehaviour {
        public RestServer.RestServer server;
        public GameObject cube;

        public void Start() {
            // Create the cube when the request comes in
            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/api/spawn_cube", request => {
                ThreadingHelper.Instance.ExecuteAsync(() => cube.SetActive(true));
                request.CreateResponse()
                    .SendAsync();
            });

            // Destroy the cube when the request comes in
            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/api/destroy_cube", request => {
                ThreadingHelper.Instance.ExecuteAsync(() => cube.SetActive(false));
                request.CreateResponse()
                    .SendAsync();
            });

            server.SpecialHandlers.NoEndpointFoundHandler = request => {
                // Single page applications frameworks (like angular, react and others) like if the server redirects a not found route to the index.html.
                // This code removes everything after the last slash and internally redirects to this path in the hope that there is something to deliver to the 
                // client (hopefully an index.html)

                if (request.HttpRequest.Method != "GET") {
                    // No get request, delegate to the default 404 handler
                    request.CreateResponse()
                        .NotFound()
                        .SendAsync();
                    return;
                }

                var origPath = request.RequestUri.AbsolutePath;
                if (!string.IsNullOrEmpty(request.RedirectHelper.OriginalPath)) {
                    origPath = request.RedirectHelper.OriginalPath;
                }

                if (origPath == "/") {
                    request.CreateResponse()
                        .NotFound()
                        .SendAsync();
                    return;
                }

                var redirectPath = origPath;
                if (redirectPath.EndsWith("/")) {
                    // maybe we redirected already once, or it was requested like this. Move one directory up.
                    redirectPath = redirectPath.Remove(redirectPath.Length - 1);
                }

                var lastSlash = redirectPath.LastIndexOf("/", StringComparison.Ordinal);
                redirectPath = redirectPath.Remove(lastSlash);
                redirectPath = PathHelper.EnsureSlashPrefix(redirectPath);
                
                // Redirect internally to another endpoint implementation
                request.CreateResponse()
                    .ScheduleInternalRedirect(redirectPath);
                Debug.Log($"{origPath} couldn't be found, redirecting to {redirectPath}");
            };
        }
    }
}