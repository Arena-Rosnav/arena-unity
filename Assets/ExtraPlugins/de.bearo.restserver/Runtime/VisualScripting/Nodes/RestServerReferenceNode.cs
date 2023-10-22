#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;
using UnityEngine;

namespace RestServer.VisualScripting {
    [UnitTitle("Rest Server Reference")]
    [UnitCategory("RestServer")]
    [TypeIcon(typeof(MonoBehaviour))]
    public class RestServerReferenceNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput valueRestServer;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            valueRestServer = ValueInput<RestServer>(nameof(valueRestServer));
            
            Requirement(valueRestServer, inputTrigger);
            Succession(inputTrigger, outputTrigger);
        }

        private ControlOutput Action(Flow flow) {
            var restServer = flow.GetValue<RestServer>(valueRestServer); 
            
            flow.SetRRRestServer(restServer);
            
            return outputTrigger;
        }
    }
}
#endif