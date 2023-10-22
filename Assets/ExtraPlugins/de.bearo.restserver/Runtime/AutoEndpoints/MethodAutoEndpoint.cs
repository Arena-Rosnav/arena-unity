using System;
using UnityEngine;
using UnityEngine.Events;

namespace RestServer.AutoEndpoints {
    
    /// <summary>
    /// Simple endpoint that calls a UnityEvent when a endpoint is called. 
    /// </summary>
    [AddComponentMenu("Rest Server/Method Auto Endpoint (Obsolete)")]
    [HelpURL("https://markus-seidl.de/unity-restserver/")]
    [Obsolete]
    public class MethodAutoEndpoint : AbstractAutoEndpoint {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        public UnityEvent Callee;

        public override void Register() {
            _logger.logEnabled = restServer.DebugLog;
            if (Callee == null) {
                return;
            }

            _logger.Log($"Registering POST at {endpointPath} to call registered callbacks.");
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST, endpointPath, RequestHandlerPost);
        }

        private void RequestHandlerPost(RestRequest request) {
            ThreadingHelper.Instance.ExecuteSync<object>(() => {
                                                             Callee.Invoke();
                                                             return null;
                                                         },
                                                         $"MethodAutoEndpoint-{endpointPath}"
                                                        );

            request.CreateResponse().SendAsync();
        }
        
        public override void Deregister() {
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.POST, endpointPath);
        }
    }
}