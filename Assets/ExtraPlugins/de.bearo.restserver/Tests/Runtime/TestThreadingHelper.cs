using System;
using System.Collections;
using System.Net;
using System.Threading;
using NUnit.Framework;
using RestServer;
using UnityEngine;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestThreadingHelper {
        
        [UnityTest]
        public IEnumerator Test_MainThreadDetection() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            Assert.Throws<InvalidOperationException>(() => ThreadingHelper.Instance.ExecuteSync(() => "unused"));
        }
        
        [UnityTest]
        public IEnumerator Test_ExecuteSync() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    Debug.Log("Request received");
                    ThreadingHelper.Instance.ExecuteSync<object>(() => {
                        Debug.Log("Sync Executed");
                        th.GoServer.transform.position = Vector3.forward;
                        return null;
                    });
                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            Assert.AreEqual(HttpStatusCode.OK, th.LastAsyncResponse.StatusCode);
            Assert.AreEqual(Vector3.forward, th.GoServer.transform.position);
        }

        [UnityTest]
        public IEnumerator Test_ExecuteAsync() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    Debug.Log("Request received");
                    ThreadingHelper.Instance.ExecuteAsync(() => {
                        Debug.Log("Async Executed");
                        th.GoServer.transform.position = Vector3.forward;
                    });
                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncGet("/position");

            Assert.AreEqual(HttpStatusCode.OK, th.LastAsyncResponse.StatusCode);
            Assert.AreEqual(Vector3.forward, th.GoServer.transform.position);
        }

        [UnityTest]
        public IEnumerator Test_Timeout() {
            var t = ThreadingHelper.Instance;

            // This times out, since there is no RestServer executing the workload.
            Assert.Catch(() => t.ExecuteSync(() => Vector3.forward), "System.TimeoutException : Execution couldn't be finished on main-thread.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_EnqueueCoRoutine() {
            var t = ThreadingHelper.Instance;

            var workloadEnqueued = false;
            var backgroundThread = new Thread(o => {
                t.ExecuteAsyncCoroutine(DummyCoRoutine);
                workloadEnqueued = true;
            });
            backgroundThread.Start();

            yield return new WaitUntil(() => workloadEnqueued);

            var w = t.DequeueWork();
            Assert.NotNull(w.HandlerCoroutine);

            yield return null;
        }

        private IEnumerator DummyCoRoutine() {
            yield return null;
        }
    }
}