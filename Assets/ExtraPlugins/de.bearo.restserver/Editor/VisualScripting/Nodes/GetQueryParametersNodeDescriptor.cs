#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(GetQueryParametersNode))]
    public class GetQueryParametersNodeDescriptor : UnitDescriptor<GetQueryParametersNode> {
        public GetQueryParametersNodeDescriptor(GetQueryParametersNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(GetQueryParametersNode.outputParameters):
                    description.label = "Parameters";
                    description.summary = "Query parameters (the ?a=b&c=d part of the url). Multiple values for each key are possible.";
                    break;
            }
        }
    }
}
#endif