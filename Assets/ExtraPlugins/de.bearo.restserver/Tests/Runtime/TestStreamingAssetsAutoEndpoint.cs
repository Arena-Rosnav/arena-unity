using System.Collections;
using NUnit.Framework;
using RestServer;
using RestServer.AutoEndpoints;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestStreamingAssetsAutoEndpoint {
        [UnityTest]
        public IEnumerator TestTryFiles() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            var sae = th.GoServer.AddComponent<StreamingAssetsAutoEndpoint>();
            
            sae.enabled = false;
            sae.enabled = true; // re-register endpoints

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual(200, (int) r.StatusCode);
            Assert.True(responseStr.StartsWith("<html>"));
        }
        
        [UnityTest]
        public IEnumerator Test404Redirect() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            var sae = th.GoServer.AddComponent<StreamingAssetsAutoEndpoint>();
            var restServer = th.RestServer;

            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/api/blah", request => {
                request.CreateResponse().Body("/api/blah").SendAsync();
            });
            
            sae.enabled = false;
            sae.enabled = true; // re-register endpoints

            // Normal index.html request
            yield return th.HttpAsyncGet("/");
            
            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;
            
            Assert.AreEqual(200, (int) r.StatusCode);
            Assert.True(responseStr.StartsWith("<html>"));

            // "API" request
            yield return th.HttpAsyncGet("/api/blah");
            
            r = th.LastAsyncResponse;
            responseStr = r.Content.ReadAsStringAsync().Result;
            
            Assert.AreEqual(200, (int) r.StatusCode);
            Assert.AreEqual("/api/blah", responseStr);
            
        }
    }
}