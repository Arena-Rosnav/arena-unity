using System.Collections;
using RestServer;
using RestServer.Helper;
using UnityEngine;

namespace de.bearo.restserver.Samples.InterlockExample {
    
    /// <summary>
    /// Simple example on how to use the SimpleInterlock class to prevent multiple animations running at the same time and overriding each other.
    /// </summary>
    public class IEMover : MonoBehaviour {
        public RestServer.RestServer server;
        public Material[] materials;
        public int materialIdx;

        public bool disableInterlock;

        private SimpleInterlock _lock = new SimpleInterlock();

        private MeshRenderer meshRenderer;

        void Start() {
            meshRenderer = GetComponent<MeshRenderer>();
            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/move", MoveHandler);
        }
        
        /// <summary>
        /// Called when /move is called and start the animation (or not, depending on the interlock).
        /// </summary>
        public void MoveHandler(RestRequest request) {
            if (_lock.isRunning) {
                request.CreateResponse()
                    .Status(409)
                    .Body("Animation is still running.")
                    .SendAsync();
                Debug.Log("Rejecting request, animation still running.");
            }
            else {
                ThreadingHelper.Instance.ExecuteAsyncCoroutine(DoAnimation);
                request.CreateResponse()
                    .SendAsync();
            }
        }

        /// <summary>
        /// Simple coroutine that serves as our long-running animation
        /// </summary>
        public IEnumerator DoAnimation() {
            if (disableInterlock) {
                yield return DoAnimation_Internal();
            }
            else {
                using (var interlock = _lock.DoWork()) {
                    yield return DoAnimation_Internal();
                }   
            }
        }
        
        private IEnumerator DoAnimation_Internal() {
            meshRenderer.material = materials[materialIdx % materials.Length];
            materialIdx++;
            meshRenderer.enabled = true;

            for (var i = 0; i < 5; i++) {
                var rndVector = new Vector3(
                    Random.value * (Random.value < 0.5 ? -1.0f : 1.0f),
                    Random.value * (Random.value < 0.5 ? -1.0f : 1.0f),
                    0.0f //Random.value * (Random.value < 0.5 ? -1.0f : 1.0f)
                );

                var start = transform.position;
                var target = transform.position + rndVector;
                var step = 0.01f;
                var length = 0.5f;
                for (var t = 0.0f; t < length; t += step) {
                    transform.position = Vector3.Lerp(start, target, t / length);

                    yield return new WaitForSeconds(step);
                }
            }

            meshRenderer.enabled = false;
        }
    }
}