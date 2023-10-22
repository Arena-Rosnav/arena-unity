using System.Collections;
using System.Net;
using RestServer;
using RestServer.Helper;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestUnityNetHelper {
        
        [UnityTest]
        public IEnumerator Test_GetPosition() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.GoServer.transform.position = Vector3.down;
            
            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                "/position",
                request => {
                    
                    var position = UnityNetHelper.GetPosition(th.RestServer);
                    
                    request.CreateResponse()
                        .BodyJson(position)
                        .SendAsync();
                });

            yield return th.HttpAsyncGet("/position");
            
            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
            Assert.AreEqual(Vector3.down,  JsonUtility.FromJson<Vector3>(r.Content.ReadAsStringAsync().Result));
        }
        
        [UnityTest]
        public IEnumerator Test_SetPosition() {
            using var th = new TestHelper();
            yield return th.DoStartup();

            th.RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST,
                "/position",
                request => {
                    UnityNetHelper.SetPosition(th.RestServer, request.JsonBody<Vector3>());
                    request.CreateResponse().SendAsync();
                });

            yield return th.HttpAsyncPost("/position", JsonUtility.ToJson(Vector3.down));
            
            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);
            Assert.AreEqual(Vector3.down,  th.GoServer.transform.position);
        }
    }
}