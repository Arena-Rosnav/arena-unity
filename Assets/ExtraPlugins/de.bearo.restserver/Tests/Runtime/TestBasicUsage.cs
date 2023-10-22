using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RestServer;
using RestServer.Helper;
using UnityEngine;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestBasicUsage {
        [UnityTest]
        public IEnumerator Test_Http_Details() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            void RequestHandler(RestRequest request) {
                Assert.AreEqual("content", request.Body);
                Assert.AreEqual(7, request.BodyBytes.Length);

                Assert.AreEqual(2, request.QueryParametersDict.Keys.Count);
                Assert.AreEqual("0", request.QueryParametersDict["a"][0]);
                Assert.AreEqual("1", request.QueryParametersDict["a"][1]);
                Assert.AreEqual("3", request.QueryParametersDict["b"][0]);

                Assert.AreEqual(2, request.QueryParameters.Count);
                Assert.AreEqual("0,1", request.QueryParameters["a"]);
                Assert.AreEqual("3", request.QueryParameters["b"]);

                Assert.True(request.Headers.Keys.Contains("Host"));
                Assert.True(request.Headers.Keys.Contains("Content-Length"));

                request.CreateResponse().SendAsync();
            }

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST,
                "/position",
                RequestHandler);

            yield return th.HttpAsyncPost("/position?a=0&a=1&b=3", "content");

            var r = th.LastAsyncResponse;

            th.RestServer.StopServer(); // make this covered by coverage
        }

        [UnityTest]
        public IEnumerator Test_Two_Endpoints() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position2",
                request => { request.CreateResponse().Body("content2", "contenttype2").SendAsync(); });

            yield return th.HttpAsyncGet("/position2");

            var r = th.LastAsyncResponse;

            th.RestServer.StopServer(); // make this covered by coverage

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content2", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype2", values.First());
        }

        [UnityTest]
        public IEnumerator Test_Regex_Endpoint() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                new Regex("/.*"),
                request => { request.CreateResponse().Body("content2", "contenttype2").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            yield return th.HttpAsyncGet("/position2");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content2", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype2", values.First());
        }

        [UnityTest]
        public IEnumerator Test_Regex_Endpoint_Clash() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                new Regex("/.*"),
                request => { request.CreateResponse().Body("content2", "contenttype2").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content2", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype2", values.First());
        }

        [UnityTest]
        public IEnumerator Test_Remove_Endpoint_01() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                new Regex("/.*"),
                request => { request.CreateResponse().Body("content2", "contenttype2").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RemoveEndpoint(HttpMethod.GET, new Regex("/.*"));

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content1", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype1", values.First());
        }

        [UnityTest]
        public IEnumerator Test_Remove_Endpoint_02() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                new Regex("/.*"),
                request => { request.CreateResponse().Body("content2", "contenttype2").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RemoveEndpoint(HttpMethod.GET, "/position");

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content2", r.Content.ReadAsStringAsync().Result);

            Assert.IsTrue(r.Content.Headers.TryGetValues(HttpHeader.CONTENT_TYPE, out var values));
            Assert.AreEqual("contenttype2", values.First());
        }

        [UnityTest]
        public IEnumerator Test_Not_Found() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/doesntexist");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.NotFound, r.StatusCode);
        }

        [UnityTest]
        public IEnumerator Test_Internal_Server_Error() {
            LogAssert.Expect(LogType.Error, new Regex(".*System.SystemException: Blunt Exception.*"));

            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => throw new SystemException("Blunt Exception"));

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.InternalServerError, r.StatusCode);

            Assert.AreEqual("Internal server error", r.Content.ReadAsStringAsync().Result);
        }

        private Exception _Test_Internal_Server_Error_ExecuteSync_Exception;

        [UnityTest]
        public IEnumerator Test_Internal_Server_Error_ExecuteSync_Exception() {
            _Test_Internal_Server_Error_ExecuteSync_Exception = null;
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    try {
                        ThreadingHelper.Instance.ExecuteSync<string>(() => throw new SystemException("Test_Internal_Server_Error_ExecuteSync_Exception"));
                    }
                    catch (Exception exception) {
                        _Test_Internal_Server_Error_ExecuteSync_Exception = exception;
                        request.CreateResponse().InternalServerError().SendAsync();
                        return;
                    }

                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.InternalServerError, r.StatusCode);
            Assert.AreEqual("Internal server error", r.Content.ReadAsStringAsync().Result);
            Assert.NotNull(_Test_Internal_Server_Error_ExecuteSync_Exception);
            Assert.AreEqual("Test_Internal_Server_Error_ExecuteSync_Exception", _Test_Internal_Server_Error_ExecuteSync_Exception.Message);
        }

        private Exception _Test_Internal_Server_Error_ExecuteAsync_Exception;

        [UnityTest]
        public IEnumerator Test_Internal_Server_Error_ExecuteAsync_Exception() {
            _Test_Internal_Server_Error_ExecuteAsync_Exception = null;
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.SpecialHandlers.AsynchronousExceptionHandler = exception => { _Test_Internal_Server_Error_ExecuteAsync_Exception = exception; };

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    ThreadingHelper.Instance.ExecuteAsync(() => throw new SystemException("_Test_Internal_Server_Error_ExecuteAsync_Exception"));
                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            yield return new WaitUntil(() => _Test_Internal_Server_Error_ExecuteAsync_Exception != null);

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.NotNull(_Test_Internal_Server_Error_ExecuteAsync_Exception);
            Assert.AreEqual("_Test_Internal_Server_Error_ExecuteAsync_Exception", _Test_Internal_Server_Error_ExecuteAsync_Exception.Message);
        }

        #region Test_Coroutine

        private bool _Test_Coroutine_finished = false;

        [UnityTest]
        public IEnumerator Test_Coroutine() {
            _Test_Coroutine_finished = false;
            LogAssert.Expect(LogType.Error, new Regex(".*Finished.*"));

            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    ThreadingHelper.Instance.ExecuteAsyncCoroutine(Test_Coroutine_Coroutine);
                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            while (!_Test_Coroutine_finished) {
                yield return CICD.SafeWaitForEndOfFrame();
            }

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
        }

        public IEnumerator Test_Coroutine_Coroutine() {
            Debug.LogError("Finished.");
            _Test_Coroutine_finished = true;
            yield return null;
        }

        #endregion

        #region Test_Coroutine_Exception

        [UnityTest]
        public IEnumerator Test_Coroutine_Exception() {
            LogAssert.Expect(LogType.Error, new Regex(".*Blunt exception.*"));

            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    ThreadingHelper.Instance.ExecuteAsyncCoroutine(Test_Coroutine_Exception_Coroutine);
                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            yield return CICD.SafeWaitForEndOfFrame();
            yield return CICD.SafeWaitForEndOfFrame();
            yield return CICD.SafeWaitForEndOfFrame();

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
        }

        public IEnumerator Test_Coroutine_Exception_Coroutine() {
            throw new SystemException("Blunt exception");
        }

        #endregion

        #region Test_PathParameters

        [UnityTest]
        public IEnumerator Test_PathParam_Two_Endpoints() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/{name}",
                request => { request.CreateResponse().Body(request.PathParams["name"].ValueString).SendAsync(); });

            yield return th.HttpAsyncGet("/position/value");

            var r = th.LastAsyncResponse;

            th.RestServer.StopServer(); // make this covered by coverage

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("value", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test_PathParam_Miss_Endpoint() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/{name}",
                request => { request.CreateResponse().Body(request.PathParams["name"].ValueString).SendAsync(); });

            yield return th.HttpAsyncGet("/position/value/"); // trailing slash --> should not hit any endpoint

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.NotFound, r.StatusCode);
        }

        [UnityTest]
        public IEnumerator Test_PathParam_Multiple_Parameters() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/{name}",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/{name1}/{name2}",
                request => { request.CreateResponse().Body(request.PathParams["name1"].ValueString).SendAsync(); });

            yield return th.HttpAsyncGet("/position/value1/value2");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("value1", r.Content.ReadAsStringAsync().Result);
        }

        [UnityTest]
        public IEnumerator Test_PathParam_Clash() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/{name}",
                request => { request.CreateResponse().Body("content1", "contenttype1").SendAsync(); });

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position/{name1}",
                request => { request.CreateResponse().Body(request.PathParams["name1"].ValueString).SendAsync(); });

            yield return th.HttpAsyncGet("/position/value1");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            Assert.AreEqual("content1", r.Content.ReadAsStringAsync().Result); // matches always the first registered one
        }

        #endregion
    }
}