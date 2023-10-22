using UnityEngine;

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Auto endpoint which registers GET, POST and PATCH at the given endpointPath. Calls allow to adjust
    /// position, rotation and scale of the given transform.
    /// </summary>
    [AddComponentMenu("Rest Server/Transform Auto Endpoint")]
    [HelpURL("https://markus-seidl.de/unity-restserver/doc/60_autoendpoints/#transform-auto-endpoint")]
    public class TransformAutoEndpoint : AbstractAutoEndpoint {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        public GameObject target;
        
        public override void Register() {
            _logger.logEnabled = restServer.DebugLog;
            if (target == null) {
                target = gameObject;
            }

            if (target == null) {
                return;
            }

            _logger.Log($"Registering GET/POST/PATCH at {endpointPath} to change transform of {target.name}");
            TransformAutoEndpointImpl.Register(restServer, target, endpointPath);
        }

        public override void Deregister() {
            TransformAutoEndpointImpl.Deregister(restServer, endpointPath);
        }
    }
}