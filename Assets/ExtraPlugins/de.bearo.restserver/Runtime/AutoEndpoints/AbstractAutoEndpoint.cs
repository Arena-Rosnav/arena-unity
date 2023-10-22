using UnityEngine;

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Simple base class for all auto endpoints, which tries to find the rest server if it's not supplied directly.
    /// </summary>
    public abstract class AbstractAutoEndpoint : MonoBehaviour {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        [Tooltip("Reference to the rest server to use. Tries to determine it if the rest server is on the same GameObject.")]
        public RestServer restServer;

        [Tooltip("Endpoint to register the functionality to. Must start with '/'.")]
        public string endpointPath = "/";

        /// <summary>
        /// Called on start if restServer and endpointPath is set. Register needed endpoints.
        /// </summary>
        public abstract void Register();

        /// <summary>
        /// Called OnDisable. Remove all registered endpoints. 
        /// </summary>
        public abstract void Deregister();

        /// <summary>True if the endpoint has been registered (= finished initialization)</summary>
        protected bool _registered;
        
        public void Start() {
            if (_registered) {
                return;
            }
            
            if (restServer == null) {
                // Try to find rest server if it's the same component
                restServer = GetComponent<RestServer>();
            }

            if (restServer == null) {
                _logger.LogError("No rest server instance could be found.", this);
                return;
            }

            if (string.IsNullOrEmpty(endpointPath)) {
                _logger.LogError("No endpoint path specified.", this);
                return;
            }

            Register();
            _registered = true;
        }
        
        public void OnDisable() {
            if (restServer == null) {
                return;
            }

            if (string.IsNullOrEmpty(endpointPath)) {
                return;
            }
            
            Deregister();
            _registered = false;
        }

        public void OnEnable() {
            Start();
        }
    }
}