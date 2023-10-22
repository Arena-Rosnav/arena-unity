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
    public class TestSpecialHandlers {
        
        [UnityTest]
        public IEnumerator Test_Custom_AuthHandler() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.SpecialHandlers.AuthHandler = request => {
                request.CreateResponse().NotAuthenticated().SendAsync();
                return false;
            };

            yield return th.HttpAsyncGet("/position2");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.Unauthorized, r.StatusCode);

            Assert.AreEqual("Basic", r.Headers.WwwAuthenticate.First().Scheme);
        }
        
        [UnityTest]
        public IEnumerator Test_Custom_AuthHandler_New() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.SpecialHandlers.AuthHandler = request => {
                request.CreateResponse().NotAuthenticated().SendAsync();
                return false;
            };

            yield return th.HttpAsyncGet("/position2");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.Unauthorized, r.StatusCode);

            Assert.AreEqual("Basic", r.Headers.WwwAuthenticate.First().Scheme);
        }
        
        [UnityTest]
        public IEnumerator Test_DebugLog() {
            // 127.0.0.1 - - [02/10/2022:14:47:39 +02:00] "GET /position2 HTTP/1.1" 404
            LogAssert.Expect(LogType.Log, new Regex("127.0.0.1 - - \\[.*\\] \"GET /position2 HTTP/1.1\" 404"));
            
            using var th = new TestHelper();

            th.RestServer.SpecialHandlers.AccessLog = DefaultRequestHandlerImpl.DebugLogAccessLog;
            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/position2");
            yield return th.HttpAsyncGet("/position2"); // call multiple times, so LogAssert picks this up. Maybe WaitForNextFrame would help as well?
        }
        
        [UnityTest]
        public IEnumerator Test_Custom_NoEndpointFound() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.SpecialHandlers.NoEndpointFoundHandler = request => {
                request.CreateResponse().Status(414).SendAsync();
            };

            yield return th.HttpAsyncGet("/position2");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.RequestUriTooLong, r.StatusCode);
        }

        private Exception _Test_Custom_ErrorHandler_Exception;
        [UnityTest]
        public IEnumerator Test_Custom_ErrorHandler() {
            using var th = new TestHelper();
            yield return th.DoStartup();
            
            _Test_Custom_ErrorHandler_Exception = null;

            th.RestServer.SpecialHandlers.EndpointErrorHandler = (request, exception) => {
                _Test_Custom_ErrorHandler_Exception = exception;
                request.CreateResponse().InternalServerError().SendAsync();
            };
            
            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => throw new SystemException("Custom."));

            yield return th.HttpAsyncGet("/position");

            var r = th.LastAsyncResponse;

            Assert.AreEqual(HttpStatusCode.InternalServerError, r.StatusCode);
            Assert.NotNull( _Test_Custom_ErrorHandler_Exception);
            Assert.AreEqual("Custom.", _Test_Custom_ErrorHandler_Exception.Message);
        }
    }
}