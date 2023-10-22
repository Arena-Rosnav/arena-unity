#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(RemoveHeaderCustomNode))]
    public class RemoveHeaderCustomNodeDescriptor: UnitDescriptor<RemoveHeaderCustomNode> {
        public RemoveHeaderCustomNodeDescriptor(RemoveHeaderCustomNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);
            
            switch (port.key) {
                case nameof(RemoveHeaderCustomNode.valueHeaderInput):
                    description.label = "Header Input";
                    description.summary = "Existing header collection.";
                    break;
                case nameof(RemoveHeaderCustomNode.valueHeaderName):
                    description.label = "Header Name";
                    description.summary = "Name of the header to remove.";
                    break;
                case nameof(RemoveHeaderCustomNode.valueHeaderValue):
                    description.label = "Header Value";
                    description.summary = "Value to remove, or empty to remove all values for the header.";
                    break;
                case nameof(RemoveHeaderCustomNode.valueHeaderOutput):
                    description.label = "Header Output";
                    description.summary = "Output of the header collection, can be used in the next node for further modification.";
                    break;
            }
        }

    }
}
#endif