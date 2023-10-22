using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using NUnit.Framework;
using RestServer;
using RestServer.Helper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestHelper : IDisposable {
        public readonly GameObject GoServer;
        public readonly RestServer.RestServer RestServer;
        public readonly HttpClient DefaultClient;

        public HttpResponseMessage LastAsyncResponse;

        public TestHelper(RestServer.RestServer restServer = null) {
            if (restServer == null) {
                GoServer = new GameObject();
                RestServer = GoServer.AddComponent<RestServer.RestServer>();
                RestServer.DebugLog = true;
                RestServer.port = 0;
                GoServer.transform.position = Vector3.zero;
            }
            else {
                RestServer = restServer;
            }
            
            DefaultClient = new HttpClient();
        }

        public IEnumerator DoStartup() {
            yield return CICD.SafeWaitForEndOfFrame();

            RestServer.enabled = false;
            RestServer.enabled = true;
            
            yield return CICD.SafeWaitForEndOfFrame();

            Debug.Log($"Rest server initialized on port {RestServer.EffectivePort}");
            DefaultClient.BaseAddress = new Uri($"http://localhost:{RestServer.EffectivePort}");

            yield return null;
        }

        public IEnumerator HttpAsyncPost(string requestUri, string content) {
            var payload = new ByteArrayContent(Encoding.UTF8.GetBytes(content));

            if (RestServer != null) {
                yield return new WaitUntil(() => RestServer.IsStarted);
            }

            var task = DefaultClient.PostAsync(requestUri, payload);

            while (!task.IsCompleted) {
                yield return new WaitForSeconds(0.001f);
            }

            LastAsyncResponse = task.Result;
        }

        public IEnumerator HttpAsyncGet(string requestUri) {
            var task = DefaultClient.GetAsync(requestUri);

            if (RestServer != null) {
                yield return new WaitUntil(() => RestServer.IsStarted);
            }

            while (!task.IsCompleted) {
                yield return new WaitForSeconds(0.001f);
            }

            LastAsyncResponse = task.Result;
        }

        public void Dispose() {
            Assert.IsFalse(ThreadingHelper.Instance.HasWorkload());
            if (RestServer != null) {
                RestServer.Server?.Stop();
            }

            DefaultClient?.Dispose();
            LastAsyncResponse?.Dispose();

            if (GoServer != null) {
                Object.DestroyImmediate(GoServer);
            }
        }
    }
}