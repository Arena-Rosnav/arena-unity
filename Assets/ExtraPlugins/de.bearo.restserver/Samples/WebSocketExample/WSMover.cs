using System;
using RestServer;
using RestServer.WebSocket;
using UnityEngine;

namespace de.bearo.restserver.Samples.WebSocketExample {
    
    /// <summary>
    /// Handles the movement of a cube via a websocket. 
    /// </summary>
    public class WSMover : MonoBehaviour {
        public RestServer.RestServer Server;

        public GameObject ToMove;
        public Vector3 From;
        public Vector3 To;
        public float Speed = 2f;

        public bool FromOrTo;
        private bool Move = true;

        /// <summary>Endpoint id to identify the clients connected to the registered endpoint, needed to send responses back to clients registered</summary>
        private WSEndpointId _endpointId;

        public void Start() {
            // Register the endpoint, and not the endpointId to send back information to the client 
            _endpointId = Server.EndpointCollection.RegisterWebsocketEndpoint("/websocket/cube_updates", message => {
                var cmd = message.ToJson<WebsocketMessage>();
                Debug.Log("Received command: " + cmd.Command + " from: " + message.Session.Socket.RemoteEndPoint);

                var newMoveValue = false;
                switch (cmd.Command) {
                    case "stop":
                        newMoveValue = false;
                        break;
                    case "start":
                        newMoveValue = true;
                        break;
                }

                ThreadingHelper.Instance.ExecuteAsync(() => { Move = newMoveValue; });

            });
        }
        
        void Update() {
            if (Move) {
                if (FromOrTo) {
                    ToMove.transform.position = Vector3.Lerp(ToMove.transform.position, To, Time.deltaTime * Speed);
                    if ((ToMove.transform.position - To).sqrMagnitude < 1) {
                        FromOrTo = !FromOrTo;
                    }
                }
                else {
                    ToMove.transform.position = Vector3.Lerp(ToMove.transform.position, From, Time.deltaTime * Speed);
                    if ((ToMove.transform.position - From).sqrMagnitude < 1) {
                        FromOrTo = !FromOrTo;
                    }
                }
            }

            var now = DateTime.Now;
            var wsm = new WebsocketMessage() {
                Command = null,
                Position = ToMove.transform.position,
                Time = now.ToString("HH:mm:ss:fff")
            };

            var json = JsonUtility.ToJson(wsm);
            Server.WsSend(_endpointId, json);
        }

        /// <summary>
        /// Custom json structure to send over the websocket to the client (used to generate the json string)
        /// </summary>
        public struct WebsocketMessage {
            public string Command;
            public Vector3 Position;
            public string Time;
        }
    }
}