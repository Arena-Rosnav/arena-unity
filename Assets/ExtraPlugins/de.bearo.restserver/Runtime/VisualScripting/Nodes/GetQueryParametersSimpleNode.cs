#if RESTSERVER_VISUALSCRIPTING
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [UnitTitle("Get Query Parameters (simple)")]
    [UnitSubtitle("Query parameters of the incoming request.")]
    [UnitCategory("RestServer")]
    [TypeIcon(typeof(Enum))]
    public class GetQueryParametersSimpleNode : Unit {
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

            outputParameters = ValueOutput<Dictionary<string, string>>(nameof(outputParameters));

            Succession(inputTrigger, outputTrigger);
            Assignment(inputTrigger, outputParameters);
        }

        private ControlOutput Action(Flow flow) {
            var vreq = flow.GetRRRequestInfo();
            var req = vreq.RestRequest;

            var qp = req.QueryParameters;
            var tempQP = new Dictionary<string, string>();
            foreach (var key in qp.AllKeys) {
                var values = qp.GetValues(key);
                if (values != null && values.Length >= 1)
                    tempQP.Add(key, values.Last());
            }

            flow.SetValue(outputParameters, tempQP);

            return outputTrigger;
        }
    }
}

#endif