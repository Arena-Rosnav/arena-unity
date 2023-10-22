using System;
using System.Collections.Generic;
using System.Threading;

namespace RestServer.WebSocket {
    /// <summary>Websocket Session collection keeps book of all websocket sessions in order to send messages to specific endpoint connected clients</summary>
    public class WsSessionCollection {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<WSEndpointId, Dictionary<Guid, LowLevelSession>> _endpointSessions = new Dictionary<WSEndpointId, Dictionary<Guid, LowLevelSession>>();

        /// <summary>Registers a new session for the given endpoint id.</summary>
        public void RegisterSession(WSEndpointId wsEndpointId, LowLevelSession session) {
            _lock.EnterWriteLock();
            try {
                if (_endpointSessions.TryGetValue(wsEndpointId, out var sessions)) {
                    sessions.Add(session.Id, session);
                }
                else {
                    _endpointSessions.Add(wsEndpointId, new Dictionary<Guid, LowLevelSession> {
                        { session.Id, session }
                    });
                }
            }
            finally {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>Removes the session from the collection for the given endpoint id.</summary>
        public void DeregisterSession(WSEndpointId wsEndpointId, LowLevelSession session) {
            _lock.EnterWriteLock();
            try {
                if (_endpointSessions.TryGetValue(wsEndpointId, out var sessions)) {
                    sessions.Remove(session.Id);
                }
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>All connected sessions for the given endpoint id.</summary>
        public IEnumerable<LowLevelSession> GetSessions(WSEndpointId wsEndpointId) {
            _lock.EnterReadLock();
            try {
                if (_endpointSessions.TryGetValue(wsEndpointId, out var sessions)) {
                    return new List<LowLevelSession>(sessions.Values);
                }
                return Array.Empty<LowLevelSession>();
            }
            finally {
                _lock.ExitReadLock();
            }
        }
    }
}