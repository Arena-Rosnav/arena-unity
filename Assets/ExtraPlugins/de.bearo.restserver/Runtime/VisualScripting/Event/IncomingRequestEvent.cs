#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    
    [UnitTitle("Incoming Request Event")]
    [UnitCategory("RestServer")]
    [UnitSubtitle("Handle incoming request")]
    public class IncomingRequestEvent : EventUnit<VisualRestRequest> {
        
        [DoNotSerialize]
        public ValueInput valueEndpointReference;
        
        protected override bool register => true;
        
        public override EventHook GetHook(GraphReference reference) {
            return new EventHook(VisualScriptingConstants.IncomingRequestEvent);
        }

        protected override void Definition() {
            base.Definition();

            valueEndpointReference = ValueInput<VisualEndpointDescription>("Endpoint Reference");
        }

        protected override bool ShouldTrigger(Flow flow, VisualRestRequest args) {
            var def = flow.GetValue<VisualEndpointDescription>(valueEndpointReference);

            var ret = args.VisualEndpointDescription == def;

            return ret;
        }

        // Setting the value on our port.
        protected override void AssignArguments(Flow flow, VisualRestRequest data) {
            flow.SetRRRequestInfo(data);
        }
        
    }
}

#endif