#if RESTSERVER_VISUALSCRIPTING
using System.Collections;
using System.Linq;
using System.Net;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestVisualScripting {
        [UnityTest]
        public IEnumerator Test_Get_Path() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();

            using var th = new TestHelper(restServer: Object.FindObjectOfType<RestServer.RestServer>());
            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/?a=muhmuh");

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            var responseContent = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("muhmuh", responseContent);

            Assert.AreEqual("DoNotRemove", r.Headers.GetValues("AddCustom").First());
            Assert.AreEqual("SetHeaderCustom", r.Headers.GetValues("SetHeaderCustom").First());
            Assert.AreEqual("SetHeaderETag", r.Headers.GetValues("ETag").First());
        }

        [UnityTest]
        public IEnumerator Test_Post_Path() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: Object.FindObjectOfType<RestServer.RestServer>());
            yield return th.DoStartup();

            const string content = "please send this back";
            yield return th.HttpAsyncPost("/", content);

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.Created, r.StatusCode);

            var responseContent = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(content, responseContent);

            Assert.True(r.Headers.Contains("Host"));
            Assert.AreEqual("localhost:8080", r.Headers.GetValues("Host").First());
        }

        [UnityTest]
        public IEnumerator Test_SimpleParameters_01() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: Object.FindObjectOfType<RestServer.RestServer>());
            yield return th.DoStartup();

            const string content = "Unused";
            yield return th.HttpAsyncGet($"/simpleParameters_01?a={content}");

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            var responseContent = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(content, responseContent);
        }
        
        [UnityTest]
        public IEnumerator Test_TransformAutoEndpoint_01() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: Object.FindObjectOfType<RestServer.RestServer>());
            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/transformAutoEndpoint");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;
            
            Assert.AreEqual("{\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"rotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"scale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0}}", responseStr);
        }
        
        // [UnityTest]
        // public IEnumerator Test_MaterialAutoEndpoint_01() {
        //     SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);
        //
        //     yield return new WaitForFixedUpdate();
        //     using var th = new TestHelper(false);
        //
        //     yield return th.HttpAsyncGet("/materialAutoEndpoint");
        //
        //     var r = th.LastAsyncResponse;
        //     var responseStr = r.Content.ReadAsStringAsync().Result;
        //     
        //     Assert.AreEqual("{\"colors\":[{\"name\":\"_Color\",\"value\":{\"r\":0.0,\"g\":0.0,\"b\":1.0,\"a\":1.0}}],\"floats\":[],\"ints\":[],\"matrixs\":[],\"vectors\":[],\"textureOffsets\":[],\"textureScales\":[]}", responseStr);
        // }
    }
}

#endif