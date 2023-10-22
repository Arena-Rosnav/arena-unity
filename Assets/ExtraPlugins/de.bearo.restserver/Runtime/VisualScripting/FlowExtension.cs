#if RESTSERVER_VISUALSCRIPTING

using Unity.VisualScripting;
using UnityEngine;

namespace RestServer.VisualScripting {
    /// <summary>
    /// Flow extension that helps in developing visual scripting nodes.
    /// </summary>
    public static class FlowExtension {
        public static T TryGetValue<T>(this Flow flow, ValueInput input, T defaultValue) {
            try {
                return flow.GetValue<T>(input);
            } catch (MissingValuePortInputException) {
                return defaultValue;
            }
        }

        #region RequestInfo

        public static void SetRRRequestInfo(this Flow flow, VisualRestRequest request) {
            flow.variables.Set(VisualScriptingConstants.RequestReferenceVariableName, request);
        }

        public static VisualRestRequest GetRRRequestInfo(this Flow flow) {
            try {
                return flow.variables.Get<VisualRestRequest>(VisualScriptingConstants.RequestReferenceVariableName);
            } catch {
                Debug.LogError("Can not find request information. Is the current execution path connected to a 'Incoming Request Event'?");

                throw; // rethrow original exception
            }
        }

        #endregion

        #region RestServer

        public static void SetRRRestServer(this Flow flow, RestServer request) {
            flow.variables.Set(VisualScriptingConstants.RestServerVariableName, request);
        }

        public static RestServer GetRRRestServer(this Flow flow) {
            try {
                return flow.variables.Get<RestServer>(VisualScriptingConstants.RestServerVariableName);
            } catch {
                Debug.LogError("Can not find the reference to the rest server. Is there a 'Rest Server Reference' node in the execution path?");

                throw; // rethrow original exception
            }
        }

        #endregion
    }
}
#endif