#if RESTSERVER_VISUALSCRIPTING && UNITY_2021
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RestServer.AutoEndpoints;
using RestServer.VisualScripting;
using UnityEngine;
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [UnitTitle("Material Auto Endpoint")]
    [UnitCategory("RestServer/Auto Endpoint")]
    [TypeIcon(typeof(IStateTransition))]
    public class MaterialAutoEndpointNode : Unit {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput inputMaterial;

        [DoNotSerialize]
        public ValueInput inputEndpointPath;

        [DoNotSerialize]
        public ValueInput inputExposedPropertyNames;

        private string oldEndpointPath;

        protected override void Definition() {
            inputTrigger = ControlInput(nameof(inputTrigger), Action);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            inputMaterial = ValueInput<Material>(nameof(inputMaterial));
            inputExposedPropertyNames = ValueInput<string[]>(nameof(inputExposedPropertyNames));
            inputEndpointPath = ValueInput<string>(nameof(inputEndpointPath), "/");

            Succession(inputTrigger, outputTrigger);
            Requirement(inputMaterial, inputTrigger);
            Requirement(inputEndpointPath, inputTrigger);
        }

        private ControlOutput Action(Flow flow) {
            var restServer = flow.GetRRRestServer();

            var material = flow.GetValue<Material>(inputMaterial);
            var exposedPropertyNames = flow.GetValue<string[]>(inputExposedPropertyNames);
            var endpointPath = flow.GetValue<string>(inputEndpointPath);

            if (oldEndpointPath != null && !string.IsNullOrEmpty(oldEndpointPath)) {
                MaterialAutoEndpointImpl.Deregister(restServer, endpointPath);
            }

            oldEndpointPath = endpointPath;
            if (endpointPath != null && !string.IsNullOrEmpty(endpointPath) && material != null && exposedPropertyNames != null && exposedPropertyNames.Length != 0) {
                var propInfo = MaterialAutoEndpointImpl.GeneratePropertiesInfo(material, exposedPropertyNames);
                MaterialAutoEndpointImpl.Register(restServer, material, propInfo, endpointPath);
            }

            return outputTrigger;
        }
    }
}
#endif