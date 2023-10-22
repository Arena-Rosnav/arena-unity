using System.Collections;
using System.Net;
using RestServer;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestRestServer {
        [UnityTest]
        public IEnumerator TestAddressLogic_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            var rs = th.RestServer;
            rs.StopServer();

            // ListenAddress --> ListenAddressUnity
            rs.ListenAddress = IPAddress.Any;
            Assert.AreEqual(ListenAddressUnity.Any, rs.ListenAddressUnity);

            rs.ListenAddress = IPAddress.IPv6Any;
            Assert.AreEqual(ListenAddressUnity.AnyIPv6, rs.ListenAddressUnity);

            rs.ListenAddress = IPAddress.Loopback;
            Assert.AreEqual(ListenAddressUnity.Loopback, rs.ListenAddressUnity);

            rs.ListenAddress = IPAddress.None;
            Assert.AreEqual(ListenAddressUnity.Unknown, rs.ListenAddressUnity);

            // ListenAddressUnity --> ListenAddress
            rs.ListenAddressUnity = ListenAddressUnity.Any;
            Assert.AreEqual(rs.ListenAddress, IPAddress.Any);

            rs.ListenAddressUnity = ListenAddressUnity.Loopback;
            Assert.AreEqual(rs.ListenAddress, IPAddress.Loopback);

            rs.ListenAddressUnity = ListenAddressUnity.AnyIPv6;
            Assert.AreEqual(rs.ListenAddress, IPAddress.IPv6Any);

            var expected = new IPAddress(new byte[] { 100, 100, 100, 100 });
            rs.ListenAddress = expected;
            rs.ListenAddressUnity = ListenAddressUnity.Unknown; // should NOT change the ListenAddress!
            Assert.AreEqual(rs.ListenAddress, expected);

            // Special Cases
            rs.ListenAddress = null;
            Assert.AreEqual(rs.ListenAddress, IPAddress.Loopback);
            Assert.AreEqual(rs.ListenAddressUnity, ListenAddressUnity.Loopback);
        }

        [UnityTest]
        public IEnumerator TestAdvancedCallbacks() {
            using var th = new TestHelper();
            
            var rs = th.RestServer;
            
            var serverConfig = false;
            var sessionConfig = false;
            rs.AdditionalSocketConfigurationServer = socket => { serverConfig = true; };

            rs.AdditionalSocketConfigurationSession = socket => { sessionConfig = true; };

            // we have to do a dummy call, so that a session is created
            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => { request.CreateResponse().SendAsync(); });
            
            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/position");

            Assert.IsTrue(serverConfig, "Configuration for server wasn't called.");
            Assert.IsTrue(sessionConfig, "Configuration for session wasn't called.");
        }
    }
}