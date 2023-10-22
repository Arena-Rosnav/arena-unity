#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(TransformAutoEndpointNode))]
    public class TransformAutoEndpointNodeDescriptor : UnitDescriptor<TransformAutoEndpointNode> {
        public TransformAutoEndpointNodeDescriptor(TransformAutoEndpointNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(TransformAutoEndpointNode.inputGameObject):
                    description.label = "Game Object";
                    description.summary = "Game object which position is updated via the endpoints.";
                    break;
                case nameof(TransformAutoEndpointNode.inputEndpointPath):
                    description.label = "Endpoint Path";
                    description.summary =
                        "URL Path under which GET/POST/PATCH methods are registered to query and modify the position/rotation/scale of the game object.";
                    break;
            }
        }
    }
}
#endif