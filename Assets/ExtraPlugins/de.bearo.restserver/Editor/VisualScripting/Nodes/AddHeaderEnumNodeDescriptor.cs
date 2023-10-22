#if RESTSERVER_VISUALSCRIPTING
using Unity.VisualScripting;

namespace RestServer.VisualScripting {
    [Descriptor(typeof(AddHeaderEnumNode))]
    public class AddHeaderEnumNodeDescriptor : UnitDescriptor<AddHeaderEnumNode> {
        public AddHeaderEnumNodeDescriptor(AddHeaderEnumNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(AddHeaderEnumNode.valueHeaderInput):
                    description.label = "Header Input";
                    description.summary = "Existing header collection to add to, or empty to create a new header.";
                    break;
                case nameof(AddHeaderEnumNode.valueHeaderName):
                    description.label = "Header Name";
                    description.summary =
                        "Name of the header to add. If the header to add is not in the " +
                        "list, use 'Add Header (Custom)'. The same Header names can be added multiple times." +
                        " The library will not overwrite existing name value combinations.";
                    break;
                case nameof(AddHeaderEnumNode.valueHeaderValue):
                    description.label = "Header Value";
                    description.summary = "Value for the specified header name.";
                    break;
                case nameof(AddHeaderEnumNode.valueHeaderOutput):
                    description.label = "Header Output";
                    description.summary = "Output of the header collection, can be used in the next node for further modification.";
                    break;
            }
        }
    }
}

#endif