using System;
using System.Collections.Generic;
using RestServer.Helper;
using UnityEngine;
using UnityEngine.Events;

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Simple endpoint that calls a UnityEvent when a endpoint is called. 
    /// </summary>
    [AddComponentMenu("Rest Server/Method V2 Auto Endpoint")]
    [HelpURL("https://markus-seidl.de/unity-restserver/doc/60_autoendpoints/#method-v2-auto-endpoint")]
    public class MethodV2AutoEndpoint : AbstractAutoEndpoint {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        public List<MethodV2AutoEndpointDescription> endpoints = new List<MethodV2AutoEndpointDescription>();

        public override void Register() {
            _logger.logEnabled = restServer.DebugLog;
            if (endpoints == null || endpoints.Count == 0) {
                return;
            }

            foreach (var endpoint in endpoints) {
                var effectiveEndpointPath = PathHelper.ConcatPath(endpointPath, endpoint.subPath);
                _logger.Log($"Registering {endpoint.method} at {effectiveEndpointPath} to call registered callback.");
                endpoint.HandlerReference = this;
                restServer.EndpointCollection.RegisterEndpoint(endpoint.method, effectiveEndpointPath, RequestHandlerPost, endpoint);
            }
        }

        private void RequestHandlerPost(RestRequest request) {
            if (!request.Endpoint.HasValue) {
                return;
            }

            var endpointDescription = request.Endpoint.Value.Tag as MethodV2AutoEndpointDescription;
            if (endpointDescription == null) {
                return;
            }

            ThreadingHelper.Instance.ExecuteSync<object>(() => {
                    endpointDescription.callee.Invoke(request);
                    return null;
                },
                $"MethodAutoEndpoint-{endpointPath}"
            );

            if (!request.ResponseLog.ResponseSent) {
                // send a standard response back if the implementation didn't do this already - making it easier for implementers.
                request.CreateResponse().SendAsync();
            }
        }

        public override void Deregister() {
            // do not trust the endpoints collection, as it could have been modified
            foreach (var method in HttpMethodExtension.All()) {
                var methodEndpoints = restServer.EndpointCollection.GetAllEndpoints(method);

                if (methodEndpoints == null) {
                    continue; // no endpoints found
                }

                foreach (var endpoint in methodEndpoints) {
                    var tag = endpoint.Tag;
                    if (tag == null) {
                        continue;
                    }

                    if (typeof(MethodV2AutoEndpointDescription) != tag.GetType()) {
                        continue;
                    }

                    var content = (MethodV2AutoEndpointDescription)tag;
                    if (object.ReferenceEquals(content.HandlerReference, this)) {
                        restServer.EndpointCollection.RemoveEndpoint(method, endpoint.EndpointString);
                    }
                }
            }
        }
    }

    [Serializable]
    public class MethodV2AutoEndpointDescription {
        [Tooltip("Path under endpoint path of the component this resource can be found")]
        public string subPath = "/";

        public HttpMethod method = HttpMethod.GET;

        [Tooltip("Function to call, has to have the signature 'void method(RestRequest)'")]
        public UnityEvent<RestRequest> callee;

        /// <summary>
        /// Internal reference to easily clear only the endpoints registered by the registering component.
        /// </summary>
        internal MethodV2AutoEndpoint HandlerReference;
    }
}