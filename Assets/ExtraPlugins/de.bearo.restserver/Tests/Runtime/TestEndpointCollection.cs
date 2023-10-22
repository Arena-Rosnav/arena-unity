using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RestServer;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestEndpointCollection {
        [UnityTest]
        public IEnumerator Test_RegisterEndpoint_ArgumentException_01() {
            var epc = new EndpointCollection();

            Assert.Catch(() => epc.RegisterEndpoint(new Endpoint()), "ArgumentNullException");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_RegisterEndpoint_ArgumentException_02() {
            var epc = new EndpointCollection();

            var e = new Endpoint() {
                EndpointString = "/",
                Method = HttpMethod.GET,
            };
            Assert.Catch(() => epc.RegisterEndpoint(e), "ArgumentNullException");

            yield return null;
        }
        
        [Test]
        public void Test_RegisterEndpoint_FindEndpoint_01() {
            var epc = new EndpointCollection();
            
            var e = new Endpoint() {
                EndpointRegex = new Regex("/asdf"),
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            var foundEndpoint = epc.FindEndpoint(HttpMethod.GET, "/basf");

            Assert.IsFalse(foundEndpoint.HasValue);
        }

        [Test]
        public void Test_GetAllEndpoints() {
            var epc = new EndpointCollection();

            var e = new Endpoint() {
                EndpointString = "/1",
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            e = new Endpoint() {
                EndpointString = "/2",
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            e = new Endpoint() {
                EndpointString = "/3",
                Method = HttpMethod.PUT,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            var allEndpoints = epc.GetAllEndpoints(HttpMethod.GET);

            Assert.AreEqual(2, allEndpoints.Count);
            var found1 = false;
            var found2 = false;

            foreach (var endpoint in allEndpoints) {
                switch (endpoint.EndpointString) {
                    case "/1":
                        found1 = true;
                        break;
                    case "/2":
                        found2 = true;
                        break;
                }
            }

            Assert.True(found1);
            Assert.True(found2);
        }

        [UnityTest]
        public IEnumerator Test_RegisterEndpoint_NoExceptionThrown() {
            var epc = new EndpointCollection();

            var e = new Endpoint() {
                EndpointString = "/",
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            Assert.NotNull(epc.FindEndpoint(HttpMethod.GET, "/"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_RemoveEndpoint_NotFound_01() {
            var epc = new EndpointCollection();

            Assert.Null(epc.RemoveEndpoint(HttpMethod.GET, "/"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_RemoveEndpoint_NotFound_02() {
            var epc = new EndpointCollection();

            var e = new Endpoint() {
                EndpointString = "/",
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            Assert.Null(epc.RemoveEndpoint(HttpMethod.GET, "/asdf"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_RemoveEndpointRegex_NotFound_01() {
            var epc = new EndpointCollection();

            Assert.Null(epc.RemoveEndpoint(HttpMethod.GET, new Regex(".*")));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_RemoveEndpointRegex_NotFound_02() {
            var epc = new EndpointCollection();

            var e = new Endpoint {
                EndpointRegex = new Regex("somethingelse"),
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            e = new Endpoint {
                EndpointString = "/aa",
                Method = HttpMethod.GET,
                RequestHandler = request => { }
            };
            epc.RegisterEndpoint(e);

            Assert.Null(epc.RemoveEndpoint(HttpMethod.GET, new Regex(".*")));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_FindEndpoint_NotFound_01() {
            var epc = new EndpointCollection();

            Assert.Null(epc.FindEndpoint(HttpMethod.GET, "/"));

            yield return null;
        }
    }
}