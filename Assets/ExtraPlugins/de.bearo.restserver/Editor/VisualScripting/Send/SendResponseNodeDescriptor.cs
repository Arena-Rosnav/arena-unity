#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(SendResponseNode))]
    public class SendResponseNodeDescriptor : UnitDescriptor<SendResponseNode> {
        public SendResponseNodeDescriptor(SendResponseNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(SendResponseNode.inputStatusCode):
                    description.label = "Status Code";
                    description.summary =
                        "Http Status code to send back (200 for ok, 4xx if something is wrong with the request and 5xx for internal errors).";
                    break;
                case nameof(SendResponseNode.inputBody):
                    description.label = "Body";
                    description.summary = "Body string to send back to the caller.";
                    break;
                case nameof(SendResponseNode.inputHeaders):
                    description.label = "Headers";
                    description.summary =
                        "Headers for the response. By default the 'Content-Type: text/plain'" +
                        " header will be sent back if not overridden.";
                    break;
            }
        }
    }
}
#endif