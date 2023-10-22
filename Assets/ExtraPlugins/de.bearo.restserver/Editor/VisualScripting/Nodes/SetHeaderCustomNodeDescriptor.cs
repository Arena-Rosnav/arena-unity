#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(SetHeaderCustomNode))]
    public class SetHeaderCustomNodeDescriptor: UnitDescriptor<SetHeaderCustomNode> {
        public SetHeaderCustomNodeDescriptor(SetHeaderCustomNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);
            
            switch (port.key) {
                case nameof(SetHeaderCustomNode.valueHeaderInput):
                    description.label = "Header Input";
                    description.summary = "Existing header collection.";
                    break;
                case nameof(SetHeaderCustomNode.valueHeaderName):
                    description.label = "Header Name";
                    description.summary = "Name of the header to overwrite to the value.";
                    break;
                case nameof(SetHeaderCustomNode.valueHeaderValue):
                    description.label = "Header Value";
                    description.summary = "Value to set will overwrite all existing values.";
                    break;
                case nameof(SetHeaderCustomNode.valueHeaderOutput):
                    description.label = "Header Output";
                    description.summary = "Output of the header collection, can be used in the next node for further modification.";
                    break;
            }
        }

    }
}
#endif