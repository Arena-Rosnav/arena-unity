#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(EndpointDefinitionNode))]
    public class EndpointDefinitionNodeDescriptor : UnitDescriptor<EndpointDefinitionNode> {
        public EndpointDefinitionNodeDescriptor(EndpointDefinitionNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(EndpointDefinitionNode.notRequestTrigger):
                    description.label = "Not Request Trigger";
                    description.summary =
                        "Trigger executed on start, useful to register multiple endpoints. " +
                        "This trigger is not called when a rest request is incoming. Use the Incoming Request Event for that.";
                    break;
                case nameof(EndpointDefinitionNode.valueHttpMethod):
                    description.label = "Method";
                    description.summary = "The http method to respond to.";
                    break;
                case nameof(EndpointDefinitionNode.valueEndpointType):
                    description.label = "Type";
                    description.summary = "How should the endpoint path match? Use string if unsure.";
                    break;
                case nameof(EndpointDefinitionNode.valueTimeout):
                    description.label = "Timeout (ms)";
                    description.summary =
                        "Milliseconds the server will wait until it terminates the client's request. " +
                        "A send response has to be executed before the timeout is reached, otherwise the " +
                        "client will see an error.";
                    break;
                case nameof(EndpointDefinitionNode.valueEndpointPath):
                    description.label = "Path";
                    description.summary = "The endpoint path directly (type=string) or as regex (type=regex).";
                    break;
                case nameof(EndpointDefinitionNode.outputEndpointReference):
                    description.label = "Endpoint Reference";
                    description.summary =
                        "Endpoint reference needed by the Incoming Request Event to connect the" +
                        " incoming request to the correct visual graph flow.";
                    break;
            }
        }
    }
}
#endif