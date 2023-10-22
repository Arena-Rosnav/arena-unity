using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RestServer.Helper {
    public static class PathParamHelper {
        /// <summary>
        /// Parses the endpoint string to a dictionary of path parameters.
        ///
        /// Allowed: "/path/{param}/path2/{param2}"
        /// Not-Allowed: "/path/not{param}/path2/nah{param2}" or "/path/{param/invalid}/path2/{param2}/"
        /// </summary>
        /// <param name="endpoint">String with path parameters like "/path/{param}/path2/{param2}"</param>
        /// <param name="regex">The regex that has to be used to match the given endpoint string. If null, no path parameter was present in endpoint.</param>
        /// <returns>Null, if no path parameter is there, or a dictionary about all path parameters.</returns>
        public static Dictionary<string, PathParamDescription> ParseEndpoint(string endpoint, out Regex regex) {
            if (!endpoint.Contains("{")) {
                regex = null;
                return null; // no path parameters
            }

            // split path into parts
            var parts = endpoint.Substring(1).Split('/');

            // create dictionary for path parameters
            var pathParams = new Dictionary<string, PathParamDescription>();

            var regexString = "^/";
            // iterate over all parts
            for (var i = 0; i < parts.Length; i++) {
                var part = parts[i];

                if (!part.StartsWith("{")) {
                    // not a path parameter
                    if (part.EndsWith("}")) {
                        throw new ArgumentException(
                            $"Not a path parameter at <{i}> for endpoint <{endpoint}>"
                            + ", but does start with a { and not end with a }. Please balance your {}."
                        );
                    }

                    regexString += part + "/";

                    continue;
                }

                if (!part.EndsWith("}")) {
                    throw new ArgumentException(
                        $"Not a path parameter at <{i}> for endpoint <{endpoint}>"
                        + ", but does end with a } and not start with a }. Please balance your {}."
                    );
                }

                // remove brackets
                part = part.Substring(1, part.Length - 2);

                // check if path parameter has a name
                if (string.IsNullOrEmpty(part)) {
                    throw new ArgumentException($"Path parameter at <{i}> for endpoint <{endpoint}> has no name.");
                }

                // parse type, even if not supported.
                if (part.Contains(":")) {
                    var nameAndType = part.Split(':');
                    var name = nameAndType[0];
                    var type = nameAndType[1];

                    if (pathParams.ContainsKey(name)) {
                        throw new ArgumentException($"Path parameter with name <{name}> already exists.");
                    }

                    pathParams.Add(name, new PathParamDescription(name, i, type));
                } else {
                    if (pathParams.ContainsKey(part)) {
                        throw new ArgumentException($"Path parameter with name <{part}> already exists.");
                    }

                    pathParams.Add(part, new PathParamDescription(part, i, "string"));
                }

                regexString += "[^/]+/";
            }

            if (regexString.Length > 2) {
                regexString = regexString.Substring(0, regexString.Length - 1); // remove last slash
            }
            regexString += "$";

            regex = new Regex(regexString);
            return pathParams;
        }

        /// <summary>
        /// Parse the given request into path parameters, if possible.
        /// </summary>
        /// <returns>Null if no PathParams are defined or a dictionary if at least one is defined</returns>
        public static Dictionary<string, PathParamValue> ParseRequestUri(Uri requestUri, Dictionary<string, PathParamDescription> pathParams) {
            if (pathParams == null || pathParams.Count == 0 || requestUri == null) {
                return new Dictionary<string, PathParamValue>();
            }

            var parts = requestUri.AbsolutePath.Substring(1).Split('/');

            var result = new Dictionary<string, PathParamValue>();
            foreach (var pathParam in pathParams) {
                var part = parts[pathParam.Value.Index];
                result.Add(pathParam.Key, new PathParamValue(pathParam.Key, part, pathParam.Value.Type));
            }

            return result;
        }
    }
}