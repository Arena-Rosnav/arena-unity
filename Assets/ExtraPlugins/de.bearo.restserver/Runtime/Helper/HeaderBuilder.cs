using System;
using System.Collections.Generic;

namespace RestServer.Helper {
    /// <summary>
    /// Helper class to easily build headers for RestRequest.
    /// </summary>
    // ReSharper disable PossibleMultipleEnumeration
    public class HeaderBuilder {
        private readonly Dictionary<string, List<string>> _headers = new Dictionary<string, List<string>>();

        /// <summary>
        /// Default ctor.
        /// </summary>
        public HeaderBuilder() { }

        /// <summary>
        /// Uses the given dictionary as initial builder state. The given dictionary is not copied, so any modification
        /// outside also modifies the internal state.
        ///
        /// If that is not intended use <see cref="DeepClone"/> to clone the dictionary beforehand.
        /// </summary>
        /// <param name="headers">Headers to use as initial state.</param>
        public HeaderBuilder(Dictionary<string, List<string>> headers) {
            if (headers != null) {
                _headers = headers;
            }
        }

        /// <summary>
        /// Register a header and a value for that header. See HttpHeader for header name constants.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <param name="value">Value for that header</param>
        public HeaderBuilder(string name, string value) {
            withHeader(name, value);
        }

        /// <summary>
        /// Register a header and a value for that header. See HttpHeader for header name constants.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <param name="value">Value for that header</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withHeader(string name, string value) {
            ValidateArguments(name, value);
            if (_headers.TryGetValue(name, out var currentValues)) {
                currentValues.Add(value);
            } else {
                _headers.Add(name, new List<string>(new[] {value}));
            }

            return this;
        }

        /// <summary>
        /// Register a header and values for that header. See HttpHeader for header name constants.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <param name="values">Values for that header</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withHeader(string name, IEnumerable<string> values) {
            ValidateArguments(name, values);
            if (_headers.TryGetValue(name, out var currentValues)) {
                currentValues.AddRange(values);
            } else {
                _headers.Add(name, new List<string>(values));
            }

            return this;
        }

        /// <summary>
        /// Set the header 'name' to 'value' if there is currently no 'name' header in the collection.
        /// </summary>
        /// <param name="name">Name of the header to check / set.</param>
        /// <param name="value">Value to set if the header name is not existing.</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withSetIfNotExists(string name, string value) {
            ValidateArguments(name, value);
            return _headers.ContainsKey(name) ? this : withHeader(name, value);
        }

        /// <summary>
        /// Set the header 'name' to 'value' if there is currently no 'name' header in the collection.
        /// </summary>
        /// <param name="name">Name of the header to check / set.</param>
        /// <param name="values">Value to set if the header name is not existing.</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withSetIfNotExists(string name, IEnumerable<string> values) {
            ValidateArguments(name, values);
            return _headers.ContainsKey(name) ? this : withHeader(name, values);
        }

        /// <summary>
        /// Set the header 'name' to 'value' and overwrite <b>all</b> existing 'values' for the given 'name'.
        /// </summary>
        /// <param name="name">Name of the header to clear and set.</param>
        /// <param name="value">Value to overwrite.</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withOverwriteHeader(string name, IEnumerable<string> value) {
            ValidateArguments(name, value);
            return withRemoveHeaderName(name).withHeader(name, value);
        }

        /// <summary>
        /// Set the header 'name' to 'value' and overwrite <b>all</b> existing 'values' for the given 'name'.
        /// </summary>
        /// <param name="name">Name of the header to clear and set.</param>
        /// <param name="value">Value to overwrite.</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withOverwriteHeader(string name, string value) {
            ValidateArguments(name, value);
            return withRemoveHeaderName(name).withHeader(name, value);
        }

        /// <summary>
        /// Remove header with the given name
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withRemoveHeaderName(string name) {
            ValidateArgument(name);
            _headers.Remove(name);

            return this;
        }

        /// <summary>
        /// Remove the header name and value combination, if exists. If there are multiple values under this header name, only the value
        /// will be removed.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <param name="values">Values to remove</param>
        /// <returns>This class.</returns>
        public HeaderBuilder withRemoveHeader(string name, string value) {
            ValidateArguments(name, value);
            if (_headers.TryGetValue(name, out var currentValues)) {
                currentValues.RemoveAll(s => s == value);
            }

            return this;
        }

        private void ValidateArgument(string name) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException(nameof(name));
            }
        }
        
        private void ValidateArguments(string name, string value) {
            ValidateArgument(name);

            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException(nameof(value));
            }
        }
        
        private void ValidateArguments(string name, IEnumerable<string> value) {
            ValidateArgument(name);

            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
        }

        public static implicit operator Dictionary<string, List<string>>(HeaderBuilder hb) {
            return hb._headers;
        }

        /// <summary>
        /// Deep clone helper method. Useful if the HeaderBuilder class is initialized with a dictionary.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> DeepClone(Dictionary<string, List<string>> original) {
            var ret = new Dictionary<string, List<string>>();
            foreach (var entry in original) {
                var listCopy = new List<string>(entry.Value);
                ret.Add(entry.Key, listCopy);
            }

            return ret;
        }
    }
}