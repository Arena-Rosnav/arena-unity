#if UNITY_2021_1_OR_NEWER
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Exposes a unity material via rest and allows to query and modify its attributes.
    /// </summary>
    [AddComponentMenu("Rest Server/Material Auto Endpoint")]
    [HelpURL("https://markus-seidl.de/unity-restserver/doc/60_autoendpoints/#material-auto-endpoint")]
    public class MaterialAutoEndpoint : AbstractAutoEndpoint {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        [Tooltip("The material that can be changed via REST")]
        public Material material;
        
        [Tooltip("Properties of the material that should be exposed. Default unity names: _Color, _MainTex, _Glossiness, ...")]
        public string[] exposedPropertyNames;

        protected MAEPropertiesInfo _properties;

        public override void Register() {
            _logger.logEnabled = restServer.DebugLog;
            if (material == null) {
                _logger.LogError("No material referenced.", this);
                return;
            }

            if (exposedPropertyNames == null || exposedPropertyNames.Length == 0) {
                _logger.LogError("No exposed property names defined.", this);
                return;
            }

            _properties = MaterialAutoEndpointImpl.GeneratePropertiesInfo(material, exposedPropertyNames);

            MaterialAutoEndpointImpl.Register(restServer, material, _properties, endpointPath);
        }

        public override void Deregister() {
            MaterialAutoEndpointImpl.Deregister(restServer, endpointPath);
        }


    }

}
#endif
