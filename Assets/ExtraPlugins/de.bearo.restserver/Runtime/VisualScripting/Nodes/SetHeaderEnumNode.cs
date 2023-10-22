#if RESTSERVER_VISUALSCRIPTING
using System.Collections.Generic;
using RestServer.Helper;
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [UnitTitle("Set Header (Enum)")]
    [UnitCategory("RestServer")]
    [TypeIcon(typeof(Equal))]
    public class SetHeaderEnumNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput valueHeaderInput;

        [DoNotSerialize]
        public ValueInput valueHeaderName;

        [DoNotSerialize]
        public ValueInput valueHeaderValue;

        [DoNotSerialize]
        public ValueOutput valueHeaderOutput;


        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            valueHeaderInput = ValueInput<Dictionary<string, List<string>>>(nameof(valueHeaderInput));
            valueHeaderInput.AllowsNull();

            valueHeaderName = ValueInput<VisualHttpHeader>(nameof(valueHeaderName), VisualHttpHeader.ContentType);
            valueHeaderValue = ValueInput<string>(nameof(valueHeaderValue), "");

            valueHeaderOutput = ValueOutput<Dictionary<string, List<string>>>(nameof(valueHeaderOutput));

            Requirement(valueHeaderInput, inputTrigger);
            Succession(inputTrigger, outputTrigger);
            Assignment(inputTrigger, valueHeaderOutput);
        }

        private Dictionary<string, List<string>> GenerateOutput(Flow flow) {
            var defaultHeaders = new Dictionary<string, List<string>>();
            var prevHeaders = flow.TryGetValue<Dictionary<string, List<string>>>(valueHeaderInput, defaultHeaders);
            var hb = new HeaderBuilder(prevHeaders);

            var headerName = flow.GetValue<VisualHttpHeader>(valueHeaderName).ConvertToString();
            var headerValue = flow.GetValue<string>(valueHeaderValue);

            if (!string.IsNullOrWhiteSpace(headerValue)) {
                hb.withOverwriteHeader(headerName, headerValue);
            }

            return hb;
        }

        private ControlOutput Action(Flow flow) {
            var result = GenerateOutput(flow);
            flow.SetValue(valueHeaderOutput, result);

            return outputTrigger;
        }
    }
}
#endif