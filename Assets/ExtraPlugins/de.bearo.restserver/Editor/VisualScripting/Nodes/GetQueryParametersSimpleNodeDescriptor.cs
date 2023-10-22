#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(GetQueryParametersSimpleNode))]
    public class GetQueryParametersSimpleNodeDescriptor : UnitDescriptor<GetQueryParametersSimpleNode> {
        public GetQueryParametersSimpleNodeDescriptor(GetQueryParametersSimpleNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(GetQueryParametersSimpleNode.outputParameters):
                    description.label = "Parameters";
                    description.summary = "Query parameters (the ?a=b&c=d part of the url). One value for each key.";
                    break;
            }
        }
    }
}
#endif