using System.Collections;
using NUnit.Framework;
using RestServer;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestInternalRedirect {
        [UnityTest]
        public IEnumerator Test_Redirect() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            void RequestHandler1(RestRequest request) {
                request.ScheduleInternalRedirect("/2");
            }

            void RequestHandler2(RestRequest request) {
                request.CreateResponse()
                    .Body("redirected")
                    .SendAsync();
                Assert.AreEqual("/1", request.RedirectHelper.OriginalPath);
            }

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/1",
                RequestHandler1);

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/2",
                RequestHandler2);

            yield return th.HttpAsyncGet("/1");

            var r = th.LastAsyncResponse;

            var responseStr = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("redirected", responseStr);

            th.RestServer.StopServer(); // make this covered by coverage
        }
    }
}