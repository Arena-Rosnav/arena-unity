using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using RestServer.Helper;
using UnityEngine;

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Endpoint implementation that registers all occurrences of <see cref="EndpointAttribute" /> as endpoints to the server.
    /// </summary>
    [AddComponentMenu("Rest Server/Attribute Auto Endpoint")]
    [HelpURL("https://markus-seidl.de/unity-restserver/doc/60_autoendpoints/#attribute-auto-endpoint")]
    public class AttributeAutoEndpoint : AbstractAutoEndpoint {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        /// <summary>
        /// Should all MonoBehaviours on all GameObjects in this scene dynamically inspected for EndpointAttributes? Otherwise only GameObjects specified
        /// in inspectGameObjects are considered.
        /// </summary>
        public bool inspectCompleteScene = true;

        /// <summary>
        /// GameObjects to inspect, if inspectCompleteScene is false.
        /// </summary>
        public List<GameObject> inspectGameObjects;

#if UNITY_EDITOR
        /// <summary>
        /// List of all found attributes in the scene or game objects to inspect. For performance reasons this is disabled
        /// when compiling for release.
        /// </summary>
        [NonSerialized]
        public List<AttributeInformation> FoundAttributes;
#endif

        public bool IsInitDone { get; protected set; }

        public override void Register() {
            _logger.logEnabled = restServer.DebugLog;

            var gameObjectsToInvestigate = inspectGameObjects;
            if (inspectCompleteScene) {
                gameObjectsToInvestigate = new List<GameObject>(UnityEngine.Object.FindObjectsOfType<GameObject>());
            }

#if UNITY_EDITOR
            FoundAttributes = new List<AttributeInformation>();
#endif

            foreach (var declaringGameObject in gameObjectsToInvestigate) {
                var foundComponents = new List<MonoBehaviour>();
                declaringGameObject.GetComponents(foundComponents);
                _logger.Log($"Inspecting game object {declaringGameObject.name}...");

                foreach (var foundComponent in foundComponents) {
                    foreach (var method in foundComponent.GetType().GetMethods()) {
                        var attributes = method.GetCustomAttributes(typeof(EndpointAttribute), false);

                        foreach (var cAttr in attributes) {
                            var attr = (EndpointAttribute)cAttr;
#if UNITY_EDITOR
                            FoundAttributes.Add(new AttributeInformation(declaringGameObject, attr));
#endif

                            RegisterAttribute(attr, foundComponent, method, declaringGameObject.name);
                        }
                    }
                }
            }

#if UNITY_EDITOR
            FoundAttributes.Sort((a, b) => {
                var am = a.EndpointAttribute.Method.ToString();
                var bm = b.EndpointAttribute.Method.ToString();

                if (am != bm) {
                    return string.Compare(am, bm, StringComparison.Ordinal);
                }

                return string.Compare(
                    a.EndpointAttribute.SubPath,
                    b.EndpointAttribute.SubPath,
                    StringComparison.Ordinal
                );
            });
#endif

            IsInitDone = true;
        }

        private void RegisterAttribute(EndpointAttribute attr, MonoBehaviour foundComponent, MethodInfo method, string declaringGameObjectName) {
            var effectiveEndpointPath = PathHelper.ConcatPath(endpointPath, attr.SubPath);
            _logger.Log(
                $"Registering endpoint {attr.Method}#{attr.SubPath} registered on {foundComponent} at path {effectiveEndpointPath}"
            );
            var methodParameters = method.GetParameters();
            if (methodParameters.Length > 1) {
                _logger.LogError($"Method {method.Name} on component {foundComponent} and game object {declaringGameObjectName} has the " +
                                 $"wrong number of arguments. At max one argument is allowed (RestRequest).", this);
                return;
            }

            var hasArgument = false;
            if (methodParameters.Length == 1) {
                if (methodParameters[0].ParameterType != typeof(RestRequest)) {
                    _logger.LogError($"Method {method.Name} on component {foundComponent} and game object {declaringGameObjectName} " +
                                     $"has the wrong number parameter type. At max one argument is allowed (RestRequest).", this);
                }

                hasArgument = true;
            }

            var requestHandler = new Action<RestRequest>(request => {
                object[] parameters = null;
                if (hasArgument) {
                    parameters = new object[] { request };
                }

                if (attr.Synchronize) {
                    ThreadingHelper.Instance.ExecuteSync((() => method.Invoke(foundComponent, parameters)));
                }
                else {
                    method.Invoke(foundComponent, parameters);
                }


                if (!request.ResponseLog.ResponseSent) {
                    // send a standard response back if the implementation didn't do this already - making it easier for implementers.
                    request.CreateResponse().SendAsync();
                }
            });

            if (attr.IsRegex) {
                restServer.EndpointCollection.RegisterEndpoint(attr.Method, new Regex(effectiveEndpointPath), requestHandler, this);
            }
            else {
                restServer.EndpointCollection.RegisterEndpoint(attr.Method, effectiveEndpointPath, requestHandler, this);
            }
            
        }

        public override void Deregister() {
            // do not trust the endpoints collection, as it could have been modified
            foreach (var method in HttpMethodExtension.All()) {
                var methodEndpoints = restServer.EndpointCollection.GetAllEndpoints(method);

                if (methodEndpoints == null) {
                    continue; // no endpoints found
                }

                foreach (var endpoint in methodEndpoints) {
                    var tag = endpoint.Tag;
                    if (tag == null) {
                        continue;
                    }

                    if (!object.ReferenceEquals(tag, this)) {
                        continue;
                    }

                    restServer.EndpointCollection.RemoveEndpoint(method, endpoint.EndpointString);
                }
            }

            IsInitDone = false;
        }

#if UNITY_EDITOR
        public sealed class AttributeInformation {
            public readonly GameObject GameObject;
            public readonly EndpointAttribute EndpointAttribute;

            public AttributeInformation(GameObject gameObject, EndpointAttribute endpointAttribute) {
                GameObject = gameObject;
                EndpointAttribute = endpointAttribute;
            }
        }
#endif
    }

    /// <summary>
    /// Allows to easily register endpoints to the server by annotating methods with this attribute.
    /// Methods have to be public and have at max one parameter of type <see cref="RestRequest" />.
    /// The AttributeAutoEndpoint is needed in the scene and will automatically register all methods marked with this attribute as endpoint.
    ///
    /// Endpoints are synchronized by default with the unity main thread. This can be adjusted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [Serializable]
    public sealed class EndpointAttribute : Attribute {
        /// <summary>
        /// The HTTP method to register the endpoint for.
        /// </summary>
        public readonly HttpMethod Method;

        /// <summary>
        /// The path to register the endpoint for. The path is relative to the path of the AttributeAutoEndpoint. Can be a regex if IsRegex is set to true.
        /// </summary>
        public readonly string SubPath;

        /// <summary>
        /// Synchronize every invocation of the endpoint with the main thread. This is useful if you want to access Unity objects. If false
        /// manual synchronization is required, when accessing Unity objects.
        /// </summary>
        public readonly bool Synchronize;

        /// <summary>
        /// The specified sub path is a regular expression. 
        /// </summary>
        public readonly bool IsRegex;

        /// <summary>
        /// Creates a new endpoint.
        /// </summary>
        /// <param name="method">The HTTP method to register the endpoint for</param>
        /// <param name="subPath">The path to register the endpoint for. The path is relative to the path of the AttributeAutoEndpoint. Can be a regex if IsRegex is set to true.</param>
        /// <param name="synchronize">Synchronize every invocation of the endpoint with the main thread. This is useful if you want to access Unity objects. If false
        /// manual synchronization is required, when accessing Unity objects.</param>
        /// <param name="isRegex">The specified sub path is a regular expression.</param>
        public EndpointAttribute(HttpMethod method, string subPath, bool synchronize = true, bool isRegex = false) {
            Method = method;
            SubPath = subPath;
            Synchronize = synchronize;
            IsRegex = isRegex;
        }

        /// <summary>
        /// Creates a new GET endpoint.
        /// </summary>
        /// <param name="subPath">The path to register the endpoint for. The path is relative to the path of the AttributeAutoEndpoint. Can be a regex if IsRegex is set to true.</param>
        /// <param name="synchronize">Synchronize every invocation of the endpoint with the main thread. This is useful if you want to access Unity objects. If false
        /// manual synchronization is required, when accessing Unity objects.</param>
        /// <param name="isRegex">The specified sub path is a regular expression.</param>
        public EndpointAttribute(string subPath, bool synchronize = true, bool isRegex = false) {
            Method = HttpMethod.GET;
            SubPath = subPath;
            Synchronize = synchronize;
            IsRegex = isRegex;
        }
    }
}