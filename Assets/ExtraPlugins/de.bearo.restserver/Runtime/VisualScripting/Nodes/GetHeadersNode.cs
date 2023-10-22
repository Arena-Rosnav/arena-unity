#if RESTSERVER_VISUALSCRIPTING
using System.Collections.Generic;
using RestServer.Helper;
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [UnitTitle("Get Headers")]
    [UnitSubtitle("Headers of the incoming request.")]
    [UnitCategory("RestServer")]
    [TypeIcon(typeof(Dictionary<,>))]
    public class GetHeadersNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueOutput outputHeaders;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            outputHeaders = ValueOutput<Dictionary<string, List<string>>>(nameof(outputHeaders));

            Succession(inputTrigger, outputTrigger);
            Assignment(inputTrigger, outputHeaders);
        }

        private ControlOutput Action(Flow flow) {
            var vreq = flow.GetRRRequestInfo();
            var req = vreq.RestRequest;

            var headers = RequestHeaderHelper.ToHeaderDict(req.HttpRequest);
            flow.SetValue(outputHeaders, headers);

            return outputTrigger;
        }
    }
}
#endif