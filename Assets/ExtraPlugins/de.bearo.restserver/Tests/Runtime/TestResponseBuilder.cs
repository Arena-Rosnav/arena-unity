using System.Collections;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using RestServer;
using RestServer.Helper;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestResponseBuilder {
        [UnityTest]
        public IEnumerator Test01() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => { request.CreateResponse().Body("content", "contenttype2").SendAsync(); });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype2", values.First());
        }

        [UnityTest]
        public IEnumerator Test02() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .Header(HttpHeader.CONTENT_TYPE, "willbeoverwritten")
                        .Body("content", "contenttype2")
                        .Header(HttpHeader.ALLOW, "custom-value")
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values1));
            Assert.AreEqual("contenttype2", values1.First());

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.ALLOW, out var values2));
            Assert.AreEqual("custom-value", values2.First());
        }

        [UnityTest]
        public IEnumerator Test03() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .Status(201)
                        .Body("content", "contenttype2")
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.Created, r.StatusCode);

            Assert.AreEqual("content", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values1));
            Assert.AreEqual("contenttype2", values1.First());
        }

        [UnityTest]
        public IEnumerator Test04() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .Body(Encoding.UTF8.GetBytes("blah"))
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("blah", r.Content.ReadAsStringAsync().Result);
        }

        private struct Test05Temp {
            public string Ham;
            public string Spam;
        }

        [UnityTest]
        public IEnumerator Test05() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .BodyJson(new Test05Temp {
                            Ham = "HAM",
                            Spam = "SPAM"
                        })
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("{\"Ham\":\"HAM\",\"Spam\":\"SPAM\"}", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test06() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .StatusError()
                        .Body(Encoding.UTF8.GetBytes("blah"))
                        .Header(HttpHeader.ALLOW, "custom-value1")
                        .Headers(new HeaderBuilder(HttpHeader.ALLOW, "custom-value2"))
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.InternalServerError, r.StatusCode);

            Assert.AreEqual("blah", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.ALLOW, out var values2));
            Assert.AreEqual("custom-value2", values2.First());
        }

        [UnityTest]
        public IEnumerator Test07() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .StatusError()
                        .Body(Encoding.UTF8.GetBytes("blah"))
                        .ScheduleInternalRedirect("/2");
                });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/2",
                request => {
                    request.CreateResponse()
                        .Body("BLAH from redirect")
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("BLAH from redirect", r.Content.ReadAsStringAsync().Result);
        }
        
        [UnityTest]
        public IEnumerator Test08() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/",
                request => {
                    request.CreateResponse()
                        .BodyJson(null)
                        .SendAsync();
                });


            yield return th.HttpAsyncGet("/");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual(string.Empty, r.Content.ReadAsStringAsync().Result);
        }
    }
}