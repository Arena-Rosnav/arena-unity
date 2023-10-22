using System.Linq;
using System.Net;
using RestServer;
using RestServer.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace de.bearo.restserver.Samples.LoadTestExample {
    public class ExampleLoadTestController : MonoBehaviour {
        public RestServer.RestServer server;

        public Text textRequestCounter;
        public Text textIPInfo;
        public Text textFpsCounter;

        public ulong requestCount;

        void Start() {
            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/api/echo", request => {
                requestCount++;
                request.CreateResponse()
                    .Body(request.RequestUri.Query)
                    .SendAsync();
            });

            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/api/echo/unity", request => {
                requestCount++;
                var body = ThreadingHelper.Instance.ExecuteSync(() => request.RequestUri.Query);
                request.CreateResponse()
                    .Body(body)
                    .SendAsync();
            });
        }

        private ulong _frameNo = 0;
        private bool _ipsUpdated = false;

        private void Update() {
            if (_frameNo++ % 60 <= 58) {
                return;
            }

            if (!_ipsUpdated) {
                _ipsUpdated = true;

                var ipList = NetworkHelper.GetPossibleListenIPs(server);
                var ips = "";
                foreach (var ip in ipList) {
                    ips += ip.IPAddress + "; ";
                }

                textIPInfo.text = "IP(s): " + ips;
            }

            textRequestCounter.text = "Un-synchronized request count: " + requestCount +
                                      " Threading Helper Backlog: " + ThreadingHelper.Instance.WorkloadBacklogLength;
            textFpsCounter.text = $"{Mathf.Round(1.0f / Time.unscaledDeltaTime)} fps";
        }
    }
}