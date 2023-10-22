#if RESTSERVER_VISUALSCRIPTING
using RestServer.AutoEndpoints;
using Unity.VisualScripting;
using UnityEngine;

namespace RestServer.VisualScripting {
    [UnitTitle("Transform Auto Endpoint")]
    [UnitCategory("RestServer/Auto Endpoint")]
    [TypeIcon(typeof(IStateTransition))]
    public class TransformAutoEndpointNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput inputGameObject;

        [DoNotSerialize]
        public ValueInput inputEndpointPath;

        private string oldEndpointPath;
        
        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            inputGameObject = ValueInput<GameObject>(nameof(inputGameObject));
            inputEndpointPath = ValueInput<string>(nameof(inputEndpointPath), "/");

            Succession(inputTrigger, outputTrigger);
            Requirement(inputGameObject, inputTrigger);
            Requirement(inputEndpointPath, inputTrigger);
        }

        private ControlOutput Action(Flow flow) {
            var restServer = flow.GetRRRestServer();

            var gameObject = flow.GetValue<GameObject>(inputGameObject);
            var endpointPath = flow.GetValue<string>(inputEndpointPath);

            if (oldEndpointPath != null && !string.IsNullOrEmpty(oldEndpointPath)) {
                TransformAutoEndpointImpl.Deregister(restServer, endpointPath);    
            }

            oldEndpointPath = endpointPath;
            if (endpointPath != null && !string.IsNullOrEmpty(endpointPath) && gameObject != null) {
                TransformAutoEndpointImpl.Register(restServer, gameObject, endpointPath);
            }

            return outputTrigger;
        }
    }
}

#endif