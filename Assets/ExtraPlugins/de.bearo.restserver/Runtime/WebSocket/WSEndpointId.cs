using System;
using System.Threading;

namespace RestServer.WebSocket {
    /// <summary>
    /// WebSocket Endpoint id is used to link WebSocket Sessions (client-server connections) with the endpoint definitions/registrations. This link allows to send data to specific websocket endpoint registrations/sessions.
    /// See <see cref="EndpointCollection"/>#RegisterWebsocketEndpoint.
    /// </summary>
    public struct WSEndpointId : IComparable, IComparable<WSEndpointId>, IEquatable<WSEndpointId> {
        public readonly long Id;
        private static long COUNTER = 0;

        private WSEndpointId(long id) {
            Id = id;
        }

        /// <summary>Generate next endpoint id (thread-safe)</summary>
        public static WSEndpointId NextId() {
            var id = Interlocked.Increment(ref COUNTER);

            return new WSEndpointId(id);
        }

        public override bool Equals(object obj) {
            return obj is WSEndpointId other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public int CompareTo(object obj) {
            if (ReferenceEquals(null, obj)) return 1;
            return obj is WSEndpointId other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(WSEndpointId)}");
        }

        public int CompareTo(WSEndpointId other) {
            return Id.CompareTo(other.Id);
        }

        public bool Equals(WSEndpointId other) {
            return Id == other.Id;
        }
    }
}