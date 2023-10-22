#if RESTSERVER_VISUALSCRIPTING
using System.Collections.Generic;
using RestServer.Helper;
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [UnitTitle("Send Response")]
    [UnitCategory("RestServer")]
    [UnitSubtitle("Return this information to the caller")]
    [TypeIcon(typeof(MoveTowards<>))]
    public class SendResponseNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput inputBody;

        [DoNotSerialize]
        public ValueInput inputHeaders;

        [DoNotSerialize]
        public ValueInput inputStatusCode;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            inputBody = ValueInput<string>(nameof(inputBody), "");

            inputHeaders = ValueInput<Dictionary<string, List<string>>>(nameof(inputHeaders), null);

            inputStatusCode = ValueInput<int>(nameof(inputStatusCode), 200);

            Succession(inputTrigger, outputTrigger);
        }

        private ControlOutput Action(Flow flow) {
            var vreq = flow.GetRRRequestInfo(); 
            var req = vreq.RestRequest;

            var statusCode = flow.GetValue<int>(inputStatusCode);

            var defaultHeaders = new HeaderBuilder();
            var headers = flow.TryGetValue<Dictionary<string, List<string>>>(inputHeaders, defaultHeaders);
            headers = new HeaderBuilder(headers).withSetIfNotExists(HttpHeader.CONTENT_TYPE, MimeType.TEXT_PLAIN_UTF_8);

            var content = flow.GetValue<string>(inputBody);

            req.CreateResponse()
                .Status(statusCode)
                .Headers(headers)
                .Body(content)
                .SendAsync();

            vreq.WaitForEndRequest.Set();

            return outputTrigger;
        }
    }
}

#endif