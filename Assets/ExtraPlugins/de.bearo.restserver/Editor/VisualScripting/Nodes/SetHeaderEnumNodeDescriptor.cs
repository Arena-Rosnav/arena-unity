#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(SetHeaderEnumNode))]
    public class SetHeaderEnumNodeDescriptor : UnitDescriptor<SetHeaderEnumNode> {
        public SetHeaderEnumNodeDescriptor(SetHeaderEnumNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(SetHeaderEnumNode.valueHeaderInput):
                    description.label = "Header Input";
                    description.summary = "Existing header collection.";
                    break;
                case nameof(SetHeaderEnumNode.valueHeaderName):
                    description.label = "Header Name";
                    description.summary =
                        "Name of the header to overwrite to the value. " +
                        "If the header is not in the list, use 'Set Header (Custom)' instead.";
                    break;
                case nameof(SetHeaderEnumNode.valueHeaderValue):
                    description.label = "Header Value";
                    description.summary = "Value to set will overwrite all existing values.";
                    break;
                case nameof(SetHeaderEnumNode.valueHeaderOutput):
                    description.label = "Header Output";
                    description.summary = "Output of the header collection, can be used in the next node for further modification.";
                    break;
            }
        }
    }
}
#endif