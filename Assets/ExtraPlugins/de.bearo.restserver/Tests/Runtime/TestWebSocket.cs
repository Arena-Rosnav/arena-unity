using System.Collections;
using System.Net;
using NUnit.Framework;
using RestServer;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestWebSocket {
        [UnityTest]
        public IEnumerator Test_Get_Against_Websocket_Endpoint() {
            using var th = new TestHelper();

            yield return th.DoStartup();

            void NoopWebsocketHandler(WebsocketMessage message) { }

            var wsEndpointId = th.RestServer.EndpointCollection.RegisterWebsocketEndpoint("/ws", NoopWebsocketHandler);

            yield return th.HttpAsyncGet("/ws");

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.BadRequest, r.StatusCode);

            th.RestServer.StopServer(); // make this covered by coverage
        }

        // private class TestWsClient : WsClient {
        //     public TestWsClient(string address, int port) : base(address, port) { }
        //
        //     public void DisconnectAndStop() {
        //         CloseAsync(1000);
        //         while (IsConnected)
        //             Thread.Yield();
        //     }
        //
        //     public override void OnWsConnecting(HttpRequest request) {
        //         request.SetBegin("GET", "/ws");
        //         request.SetHeader("Host", "localhost");
        //         request.SetHeader("Origin", "http://localhost");
        //         request.SetHeader("Upgrade", "websocket");
        //         request.SetHeader("Connection", "Upgrade");
        //         request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
        //         request.SetHeader("Sec-WebSocket-Protocol", "chat, superchat");
        //         request.SetHeader("Sec-WebSocket-Version", "13");
        //         request.SetBody();
        //     }
        // }

        // [UnityTest]
        // public IEnumerator Test_Websocket_Endpoint() {
        //     using var th = new TestHelper();
        //
        //     yield return th.DoStartup();
        //
        //     var messageReceived = false;
        //
        //     void NoopWebsocketHandler(WebsocketMessage message) {
        //         Debug.Log("Received message: " + message);
        //         messageReceived = true;
        //     }
        //
        //     var wsEndpointId = th.RestServer.EndpointCollection.RegisterWebsocketEndpoint("/ws", NoopWebsocketHandler);
        //
        //     var wsClient = new TestWsClient("127.0.0.1", th.RestServer.EffectivePort);
        //
        //     while (!wsClient.ConnectAsync()) {
        //         yield return new WaitForEndOfFrame();
        //     }
        //     
        //     wsClient.SendText("Hello world!");
        //
        //     while (!messageReceived) {
        //         yield return new WaitForEndOfFrame();
        //     }
        //
        //     th.RestServer.StopServer(); // make this covered by coverage
        // }
    }
}