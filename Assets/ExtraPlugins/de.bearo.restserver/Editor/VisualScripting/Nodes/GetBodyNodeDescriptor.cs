#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(GetBodyNode))]
    public class GetBodyNodeDescriptor : UnitDescriptor<GetBodyNode> {
        public GetBodyNodeDescriptor(GetBodyNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(GetBodyNode.outputBody):
                    description.label = "Body";
                    description.summary = "Body from the request.";
                    break;
            }
        }
    }
}
#endif