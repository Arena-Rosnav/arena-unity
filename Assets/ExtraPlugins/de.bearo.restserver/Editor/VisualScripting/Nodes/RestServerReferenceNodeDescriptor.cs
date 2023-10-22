#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(RestServerReferenceNode))]
    public class RestServerReferenceNodeDescriptor : UnitDescriptor<RestServerReferenceNode> {
        public RestServerReferenceNodeDescriptor(RestServerReferenceNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(RestServerReferenceNode.valueRestServer):
                    description.label = "Rest Server";
                    description.summary = "Reference to the existing Rest Server instance/game object to register the endpoints to.";
                    break;
            }
        }
    }
}

#endif