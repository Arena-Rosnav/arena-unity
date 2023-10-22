#if RESTSERVER_VISUALSCRIPTING
using System;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace RestServer.VisualScripting {
    [UnitTitle("Endpoint Definition")]
    [UnitCategory("RestServer")]
    [UnitSubtitle("Define a method and path")]
    [TypeIcon(typeof(StateGraph))]
    public class EndpointDefinitionNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput notRequestTrigger;

        [DoNotSerialize]
        public ValueInput valueHttpMethod;

        [DoNotSerialize]
        public ValueInput valueEndpointType;

        [DoNotSerialize]
        public ValueInput valueEndpointPath;

        [DoNotSerialize]
        public ValueInput valueTimeout;

        [DoNotSerialize]
        public ValueOutput outputEndpointReference { get; private set; }


        private VisualEndpointDescription _visualEndpointDescription;

        private int _timeout;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            notRequestTrigger = ControlOutput(nameof(notRequestTrigger));

            valueHttpMethod = ValueInput<HttpMethod>(nameof(valueHttpMethod), HttpMethod.GET);

            valueEndpointType = ValueInput<VisualEndpointType>(nameof(valueEndpointType), VisualEndpointType.PLAIN);

            valueEndpointPath = ValueInput<string>(nameof(valueEndpointPath), "/");

            valueTimeout = ValueInput<int>(nameof(valueTimeout), 2000);

            outputEndpointReference = ValueOutput<VisualEndpointDescription>(nameof(outputEndpointReference), CreateOutput);

            Succession(inputTrigger, notRequestTrigger);
            Assignment(inputTrigger, outputEndpointReference);
        }

        private ControlOutput Action(Flow flow) {
            var rs = flow.GetRRRestServer();
            var method = flow.GetValue<HttpMethod>(valueHttpMethod);
            var type = flow.GetValue<VisualEndpointType>(valueEndpointType);
            var path = flow.GetValue<string>(valueEndpointPath);

            _visualEndpointDescription = new VisualEndpointDescription(rs, method, type, path);
            _timeout = flow.GetValue<int>(valueTimeout);

            if (rs == null) {
                Debug.LogError($"Unable to register endpoint {_visualEndpointDescription.EndpointPath}, rest server reference is missing.");
                return null;
            }

            switch (type) {
                case VisualEndpointType.PLAIN:
                    rs.EndpointCollection.RegisterEndpoint(method, path, RequestHandler);
                    break;
                case VisualEndpointType.REGEX:
                    rs.EndpointCollection.RegisterEndpoint(method, new Regex(path), RequestHandler);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown type {type}");
            }

            return notRequestTrigger;
        }

        private void RequestHandler(RestRequest request) {
            var vreq = new VisualRestRequest(request, _visualEndpointDescription);

            void RenderThread() {
                EventBus.Trigger(VisualScriptingConstants.IncomingRequestEvent, vreq);
                vreq.WaitForRequestTriggered.Set();
            }

            ThreadingHelper.Instance.ExecuteAsync(RenderThread);

            if (!vreq.WaitForRequestTriggered.WaitOne(200)) {
                Debug.LogError("Couldn't execute the flow in the render thread.");
            }

            if (!vreq.WaitForEndRequest.WaitOne(_timeout)) {
                Debug.LogWarning("Request not finished on render thread, sending back 'Internal Server Error'.");
            }
        }

        private VisualEndpointDescription CreateOutput(Flow flow) {
            return _visualEndpointDescription;
        }
    }
}

#endif