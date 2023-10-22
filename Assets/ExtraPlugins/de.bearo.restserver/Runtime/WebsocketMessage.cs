using System.Text;
using UnityEngine;

namespace RestServer {
    /// <summary>Websocket Message received by a client</summary>
    public class WebsocketMessage {
        /// <summary>Binary Message data from the client.</summary>
        public readonly byte[] Data;

        /// <summary>Reference to the server-client session that initiated the request.</summary>
        public readonly LowLevelSession Session;

        public WebsocketMessage(byte[] data, LowLevelSession session) {
            Data = data;
            Session = session;
        }

        /// <summary>Convert binary data to string with given encoding.</summary>
        public string ToString(Encoding encoding) {
            return encoding.GetString(Data);
        }

        /// <summary>Assume that the binary data is a UTF-8 encoded string that can be parsed by Unity's json utility</summary>
        public T ToJson<T>() {
            return JsonUtility.FromJson<T>(ToString());
        }

        /// <summary>Convert binary data to string with UTF-8 encoding.</summary>
        public override string ToString() {
            return Encoding.UTF8.GetString(Data);
        }
    }
}