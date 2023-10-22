#if RESTSERVER_VISUALSCRIPTING
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [UnitTitle("Get Query Parameters")]
    [UnitSubtitle("Query parameters of the incoming request.")]
    [UnitCategory("RestServer")]
    [TypeIcon(typeof(Enum))]
    public class GetQueryParametersNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueOutput outputParameters;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            outputParameters = ValueOutput<Dictionary<string, List<string>>>(nameof(outputParameters));

            Succession(inputTrigger, outputTrigger);
            Assignment(inputTrigger, outputParameters);
        }

        private ControlOutput Action(Flow flow) {
            var vreq = flow.GetRRRequestInfo();
            var req = vreq.RestRequest;

            var qp = req.QueryParameters;
            var tempQP = new Dictionary<string, List<string>>();
            foreach (var key in qp.AllKeys) {
                var values = qp.GetValues(key);
                if (values != null)
                    tempQP.Add(key, new List<string>(values));
            }

            flow.SetValue(outputParameters, tempQP);

            return outputTrigger;
        }
    }
}

#endif