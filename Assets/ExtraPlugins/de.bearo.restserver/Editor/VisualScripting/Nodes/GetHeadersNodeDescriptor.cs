#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(GetHeadersNode))]
    public class GetHeadersNodeDescriptor : UnitDescriptor<GetHeadersNode> {
        public GetHeadersNodeDescriptor(GetHeadersNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(GetHeadersNode.outputHeaders):
                    description.label = "Headers";
                    description.summary = "All headers from the incoming request, modifiable.";
                    break;
            }
        }
    }
}
#endif