using UnityEngine;

namespace de.bearo.restserver.Samples.StreamDeck {
    /// <summary>
    /// Example despawner of a dropped spheres. Despawns the sphere if it is too far away from the origin or if it exists for too long.
    /// </summary>
    public class SDSphereTemplateDespawner : MonoBehaviour {
        public float maxRadius = 100.0f;
        public float maxTime = 20.0f;

        private float creationTime;

        void Start() {
            creationTime = Time.time;
        }

        void Update() {
            if (transform.position.magnitude > maxRadius || Time.time > creationTime + maxTime) {
                Destroy(gameObject);
            }
        }
    }
}