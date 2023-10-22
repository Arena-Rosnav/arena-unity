using System;
using System.Collections.Generic;

namespace RestServer.MultipartFormData {
    public static class SimpleHeaderValueParser {
        /// <summary>
        /// A very simple parser, that parses the header value part of the format #item#;#item-name#: #item-value#; ...
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static MultipartFormDataHeaders Parse(string headerValue) {
            if (string.IsNullOrEmpty(headerValue)) {
                throw new ArgumentNullException(nameof(headerValue));
            }

            // Single item
            if (!headerValue.Contains(";")) {
                return new MultipartFormDataHeaders(headerValue);
            }

            // Multiple Items
            var ret = new MultipartFormDataHeaders();
            var elements = headerValue.Split(';');
            foreach (var element in elements) {
                if (element.Contains("=")) {
                    var leftRight = element.Split('=');
                    if (leftRight.Length == 2) {
                        // strip " if they are at the beginning/end of the value. It seems that these are not mandatory
                        var right = leftRight[1].Trim();
                        if (right.StartsWith("\"") && right.EndsWith("\"")) {
                            right = right.Substring(1, right.Length - 2);
                        }

                        ret.AddPart(leftRight[0].Trim(), right);
                        continue;
                    }
                }

                // not in expected format, maybe doesn't have key=value?
                ret.AddPart(element, null);
            }

            return ret;
        }
    }

    /// <summary>
    /// Header parse result which contains the key-value parts of the parsed header-value.
    /// </summary>
    public class MultipartFormDataHeaders {
        private readonly Dictionary<string, string> _parts = new Dictionary<string, string>();

        public MultipartFormDataHeaders(string value) {
            _parts[value.Trim().ToLower()] = null;
        }

        public MultipartFormDataHeaders() { }

        public bool HasPart(string name) {
            return _parts.ContainsKey(name.ToLower());
        }

        public string GetPart(string name) {
            name = name.ToLower();
            return _parts[name];
        }

        public void AddPart(string name, string value) {
            name = name.Trim().ToLower();
            _parts.Add(name, value);
        }

        public Dictionary<string, string> GetParts() {
            var ret = new Dictionary<string, string>();
            foreach (var part in _parts) {
                ret.Add(part.Key, part.Value);
            }

            return ret;
        }
    }
}