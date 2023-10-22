using UnityEngine;

namespace RestServer.Helper {

    public static class UnityNetHelper {
        
        /// <summary>
        /// Retrieve the position of this game object from the render thread synchronously.
        /// </summary>
        /// <param name="behaviour">MonoBehaviour to retrieve the position form</param>
        public static Vector3 GetPosition(MonoBehaviour behaviour) {
            return ThreadingHelper.Instance.ExecuteSync(() => behaviour.gameObject.transform.position);
        }

        /// <summary>
        /// Set the position of this game object from the render thread synchronously.
        /// </summary>
        /// <param name="behaviour">MonoBehaviour to set the position</param>
        public static void SetPosition(MonoBehaviour behaviour, Vector3 position) {
            ThreadingHelper.Instance.ExecuteSync(() => behaviour.gameObject.transform.position = position);
        }

    }
}