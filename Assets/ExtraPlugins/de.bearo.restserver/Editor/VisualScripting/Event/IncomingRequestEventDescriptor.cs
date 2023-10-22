#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    
    [Descriptor(typeof(IncomingRequestEvent))]
    public class IncomingRequestEventDescriptor : UnitDescriptor<IncomingRequestEvent> {

        public IncomingRequestEventDescriptor(IncomingRequestEvent target) : base(target) { }
        
        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(IncomingRequestEvent.valueEndpointReference):
                    description.label = "Endpoint Reference";
                    description.summary = "Connect this to the Endpoint Definition Endpoint Reference output.";
                    break;
            }
        }
    }
}
#endif
