using RestServer;
using UnityEngine;

namespace de.bearo.restserver.Samples.PathParamExample {
    public class PPExample : MonoBehaviour {
        public RestServer.RestServer RestServer;

        // Start is called before the first frame update
        void Start() {
            RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/position/{name}",
                request => { request.CreateResponse().Body(request.PathParams["name"].ValueString).SendAsync(); });
        }

        // Update is called once per frame
        void Update() { }
    }
}