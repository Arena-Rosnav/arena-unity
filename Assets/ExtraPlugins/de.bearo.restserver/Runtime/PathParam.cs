namespace RestServer {
    /// <summary>
    /// Internal description of a path parameter for further processing.
    /// </summary>
    public struct PathParamDescription {
        /// <summary>
        /// Name or key of the path parameter
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Numeric index at which / index the path parameter was encountered.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Type to convert the path parameter to (not supported yet).
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Create a new path parameter description.
        /// </summary>
        /// <param name="name">Name of the path parameter</param>
        /// <param name="index">Numeric index at which / index the path parameter was encountered</param>
        /// <param name="type">Type to convert the path parameter to (not supported yet)</param>
        public PathParamDescription(string name, int index, string type) {
            Name = name;
            Index = index;
            Type = type;
        }
    }

    /// <summary>
    /// Value of a path parameter after parsing.
    /// </summary>
    public struct PathParamValue {
        /// <summary>
        /// Name of the path parameter.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Parsed value of the path parameter as string.
        /// </summary>
        public readonly string ValueString;

        /// <summary>
        /// Defined type of the path parameter in the endpoint definition. (Unused yet)
        /// </summary>
        public readonly string Type;
        
        /// <summary>
        /// Create a new path parameter value.
        /// </summary>
        /// <param name="name">Name of the path parameter</param>
        /// <param name="valueString">Parsed string value of the path parameter</param>
        /// <param name="type">Defined type of the path parameter in the endpoint definition (unused).</param>
        public PathParamValue(string name, string valueString, string type) {
            Name = name;
            ValueString = valueString;
            Type = type;
        }
    }
}