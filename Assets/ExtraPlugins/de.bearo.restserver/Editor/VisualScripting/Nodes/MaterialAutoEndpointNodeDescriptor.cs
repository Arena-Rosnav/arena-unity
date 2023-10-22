#if RESTSERVER_VISUALSCRIPTING && UNITY_2021
using Unity.VisualScripting;


namespace RestServer.VisualScripting {
    [Descriptor(typeof(MaterialAutoEndpointNode))]
    public class MaterialAutoEndpointNodeDescriptor : UnitDescriptor<MaterialAutoEndpointNode> {
        public MaterialAutoEndpointNodeDescriptor(MaterialAutoEndpointNode target) : base(target) { }
        
        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(MaterialAutoEndpointNode.inputMaterial):
                    description.label = "Material";
                    description.summary = "Material which is updated via the endpoints.";
                    break;
                case nameof(MaterialAutoEndpointNode.inputExposedPropertyNames):
                    description.label = "Exposed property names";
                    description.summary = "Names of the material properties, that will be exposed (prefix with '_').";
                    break;
                case nameof(MaterialAutoEndpointNode.inputEndpointPath):
                    description.label = "Endpoint Path";
                    description.summary =
                        "URL Path under which GET/POST/PATCH methods are registered to query and modify the position/rotation/scale of the game object.";
                    break;
            }
        }
    }
}

#endif