#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(RemoveHeaderEnumNode))]
    public class RemoveHeaderEnumNodeDescriptor : UnitDescriptor<RemoveHeaderEnumNode> {
        public RemoveHeaderEnumNodeDescriptor(RemoveHeaderEnumNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(RemoveHeaderEnumNode.valueHeaderInput):
                    description.label = "Header Input";
                    description.summary = "Existing header collection.";
                    break;
                case nameof(RemoveHeaderEnumNode.valueHeaderName):
                    description.label = "Header Name";
                    description.summary =
                        "Name of the header to remove."
                        + " If the header to add is not in the list, use 'Remove Header (Custom)'.";
                    break;
                case nameof(RemoveHeaderEnumNode.valueHeaderValue):
                    description.label = "Header Value";
                    description.summary = "Value to remove, or empty to remove all values for the header.";
                    break;
                case nameof(RemoveHeaderEnumNode.valueHeaderOutput):
                    description.label = "Header Output";
                    description.summary = "Output of the header collection, can be used in the next node for further modification.";
                    break;
            }
        }
    }
}
#endif