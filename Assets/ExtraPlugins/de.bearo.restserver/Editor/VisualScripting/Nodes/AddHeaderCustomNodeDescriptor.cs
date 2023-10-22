#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(AddHeaderCustomNode))]
    public class AddHeaderCustomNodeDescriptor : UnitDescriptor<AddHeaderCustomNode> {
        public AddHeaderCustomNodeDescriptor(AddHeaderCustomNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(AddHeaderCustomNode.valueHeaderInput):
                    description.label = "Header Input";
                    description.summary = "Existing header collection to add to, or empty to create a new header.";
                    break;
                case nameof(AddHeaderCustomNode.valueHeaderName):
                    description.label = "Header Name";
                    description.summary =
                        "Name of the header to add. The same Header names can be added multiple times." +
                        " The library will not overwrite existing name value combinations.";
                    break;
                case nameof(AddHeaderCustomNode.valueHeaderValue):
                    description.label = "Header Value";
                    description.summary = "Value for the specified header name.";
                    break;
                case nameof(AddHeaderCustomNode.valueHeaderOutput):
                    description.label = "Header Output";
                    description.summary = "Output of the header collection, can be used in the next node for further modification.";
                    break;
            }
        }
    }
}

#endif