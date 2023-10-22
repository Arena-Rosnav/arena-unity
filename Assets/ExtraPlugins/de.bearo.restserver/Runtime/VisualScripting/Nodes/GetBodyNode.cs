#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;
using UnityEngine;

namespace RestServer.VisualScripting {
    [UnitTitle("Get Body")]
    [UnitSubtitle("Body of the incoming request.")]
    [UnitCategory("RestServer")]
    [TypeIcon(typeof(GameObject))]
    public class GetBodyNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueOutput outputBody;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            outputBody = ValueOutput<string>(nameof(outputBody));

            Succession(inputTrigger, outputTrigger);
            Assignment(inputTrigger, outputBody);
        }

        private ControlOutput Action(Flow flow) {
            var vreq = flow.GetRRRequestInfo();
            var req = vreq.RestRequest;

            flow.SetValue(outputBody, req.Body);

            return outputTrigger;
        }
    }
}
#endif