using System;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RestServer;
using RestServer.AutoEndpoints;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestAutoEndpoint {
        [UnityTest]
        public IEnumerator TestTransformAutoEndpoint_POST() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<TransformAutoEndpoint>();
            ae.target = th.GoServer;
            ae.endpointPath = "/tae";

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            var payload = "{ \"position\": { \"x\": 1.0, \"y\": 2.0, \"z\": 3.0 }, " +
                          "\"rotation\": { \"x\": 4.0, \"y\": 5.0, \"z\": 6.0 }, " +
                          "\"scale\": { \"x\": 7.0, \"y\": 8.0, \"z\": 9.0 } }";
            yield return th.HttpAsyncPost("/tae", payload);

            var r = th.LastAsyncResponse;

            var position = th.GoServer.transform.position;
            Assert.AreEqual(1.0, position.x);
            Assert.AreEqual(2.0, position.y);
            Assert.AreEqual(3.0, position.z);

            var rotation = th.GoServer.transform.rotation.eulerAngles;
            Assert.AreEqual(4.0, rotation.x, 0.001f);
            Assert.AreEqual(5.0, rotation.y, 0.001f);
            Assert.AreEqual(6.0, rotation.z, 0.001f);

            var scale = th.GoServer.transform.localScale;
            Assert.AreEqual(7.0, scale.x);
            Assert.AreEqual(8.0, scale.y);
            Assert.AreEqual(9.0, scale.z);
        }

        [UnityTest]
        public IEnumerator TestTransformAutoEndpoint_GET() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var ae = th.GoServer.AddComponent<TransformAutoEndpoint>();
            ae.target = th.GoServer;
            ae.endpointPath = "/tae";

            ae.enabled = false;
            ae.enabled = true; // re-register endpoints

            var t = th.GoServer.transform;
            t.position = new Vector3(1.0f, 2.0f, 3.0f);
            t.rotation = Quaternion.Euler(4.0f, 5.0f, 6.0f);
            t.localScale = new Vector3(7.0f, 8.0f, 9.0f);

            yield return th.HttpAsyncGet("/tae");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;

            Assert.AreEqual(
                "{\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"rotation\":{\"x\":4.000000476837158,\"y\":5.0,\"z\":6.0},\"scale\":{\"x\":7.0,\"y\":8.0,\"z\":9.0}}",
                responseStr);
        }


        // Removed bc obsolete
        [UnityTest]
        [Obsolete]
        public IEnumerator TestMethodAutoEndpoint() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae
        
            using var th = new TestHelper();
            yield return th.DoStartup();
        
            var called = false;
            var ae = th.GoServer.AddComponent<MethodAutoEndpoint>();
            ae.endpointPath = "/mae";
            ae.Callee = new UnityEvent();
            ae.Callee.AddListener(() => { called = true; });
        
            ae.enabled = false;
            ae.enabled = true; // re-register endpoints
        
            yield return th.HttpAsyncPost("/mae", "no");
        
            var r = th.LastAsyncResponse;
        
            Assert.IsTrue(called);
        }
        
        [UnityTest]
        public IEnumerator TestMethodV2AutoEndpoint() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();

            var called = false;
            var ae = th.GoServer.AddComponent<MethodV2AutoEndpoint>();
            ae.endpointPath = "/";
            var desc = new MethodV2AutoEndpointDescription();
            desc.method = HttpMethod.GET;
            desc.subPath = "/mae";
            desc.callee = new UnityEvent<RestRequest>();
            desc.callee.AddListener((r) => { called = true; });
            ae.endpoints.Add(desc);

            ae.enabled = false;
            
            Assert.AreEqual(null, th.RestServer.EndpointCollection.GetAllEndpoints(HttpMethod.GET));
            
            ae.enabled = true; // re-register endpoints
            ae.enabled = false; // testing de-registering
            
            Assert.AreEqual(0, th.RestServer.EndpointCollection.GetAllEndpoints(HttpMethod.GET).Count);
            
            ae.enabled = true; // re-register endpoints

            yield return new WaitUntil(() => th.RestServer.EndpointCollection.FindEndpoint(HttpMethod.GET, "/mae").HasValue);
            
            yield return th.HttpAsyncGet("/mae");

            var r = th.LastAsyncResponse;

            Assert.IsTrue(called);
        }
        
        [UnityTest]
        public IEnumerator TestMethodV2AutoEndpoint_CustomResponse() {
            //LogAssert.Expect(LogType.Error, new Regex(".*No endpoint path specified.*")); // thrown on early init of ae

            using var th = new TestHelper();
            yield return th.DoStartup();
            
            var ae = th.GoServer.AddComponent<MethodV2AutoEndpoint>();
            ae.endpointPath = "/";
            var desc = new MethodV2AutoEndpointDescription();
            desc.method = HttpMethod.GET;
            desc.subPath = "/mae";
            desc.callee = new UnityEvent<RestRequest>();
            desc.callee.AddListener((r) => { r.CreateResponse().Status(202).SendAsync(); });
            ae.endpoints.Add(desc);

            ae.enabled = false;
            
            Assert.AreEqual(null, th.RestServer.EndpointCollection.GetAllEndpoints(HttpMethod.GET));
            
            ae.enabled = true; // re-register endpoints

            yield return new WaitUntil(() => th.RestServer.EndpointCollection.FindEndpoint(HttpMethod.GET, "/mae").HasValue);
            
            yield return th.HttpAsyncGet("/mae");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.Accepted, r.StatusCode);
        }
    }
}