using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RestServer;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestRestRequest {

        private struct Temp {
            public string Ham;
            public string Spam;
        }
        
        [UnityTest]
        public IEnumerator Test_JsonBody_Roundtrip_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();
            
            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST,
                "/position",
                request => {
                    var received = request.JsonBody<Temp>();
                    request.CreateResponse().BodyJson(received).SendAsync();
                });

            var payload = new Temp() {
                Ham = "01",
                Spam = "02"
            };
            var payloadStr = JsonUtility.ToJson(payload);
            
            yield return th.HttpAsyncPost("/position", payloadStr);

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual(payloadStr, r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual(MimeType.APPLICATION_JSON_UTF_8, values.First());
        }
        
        [UnityTest]
        public IEnumerator Test_SendAsyncGetResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content", "contenttype").SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype", values.First());
        }
        
        [UnityTest]
        public IEnumerator Test_SendAsyncGetResponse_02() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    var headers = new Dictionary<string, List<string>> { { HttpHeader.CONTENT_TYPE, new List<string>(new []{ "contenttype"}) } };
                    request.CreateResponse()
                        .Body("content")
                        .Headers(headers)
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype", values.First());
        }
        
        [UnityTest]
        public IEnumerator Test_SendAsyncGetResponse_03() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    var headers = new Dictionary<string, List<string>> { { HttpHeader.CONTENT_TYPE, new List<string>(new []{ "contenttype"}) } };
                    request.CreateResponse()
                        .Body(Encoding.UTF8.GetBytes("content"))
                        .Headers(headers)
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype", values.First());
        }

        [UnityTest]
        public IEnumerator Test_SendAsyncErrorResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().StatusError().Body("content", "contenttype").SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.InternalServerError, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype", values.First());
        }

        [UnityTest]
        public IEnumerator Test_SendAsyncHeadResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("", r.Content.ReadAsStringAsync().Result);
        }


        [UnityTest]
        public IEnumerator Test_SendAsyncOkResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Status(201).SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.Created, r.StatusCode);
            Assert.AreEqual("", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test_SendAsyncOptionsResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Header(HttpHeader.ALLOW, "allow-only-these").SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
            
            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.ALLOW, out var values));
            Assert.AreEqual("allow-only-these", values.First());
            Assert.AreEqual("", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test_SendAsyncTraceResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content").Header(HttpHeader.CONTENT_TYPE, MimeType.MESSAGE_HTTP).SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test_SendAsyncTraceResponse_02() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Header(HttpHeader.CONTENT_TYPE, "content").Body(Encoding.UTF8.GetBytes("content")).SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test_SendAsyncGetJsonResponse_01() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            var payload = new Temp() {
                Ham = "01",
                Spam = "02"
            };
            var payloadStr = JsonUtility.ToJson(payload);

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().BodyJson(payload).SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
            Assert.AreEqual(payloadStr, r.Content.ReadAsStringAsync().Result);
            
            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual(MimeType.APPLICATION_JSON_UTF_8, values.First());
        }
        
        [UnityTest]
        public IEnumerator Test_SendAsyncGetJsonResponse_02() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            var payload = new Temp() {
                Ham = "01",
                Spam = "02"
            };
            var payloadStr = JsonUtility.ToJson(payload);

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Status(201).BodyJson( payload, "contenttype").SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.Created, r.StatusCode);
            Assert.AreEqual(payloadStr, r.Content.ReadAsStringAsync().Result);
            
            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype", values.First());
        }
    }
}