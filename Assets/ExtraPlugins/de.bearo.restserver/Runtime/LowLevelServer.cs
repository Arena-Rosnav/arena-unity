using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RestServer.NetCoreServer;
using RestServer.WebSocket;
using UnityEngine;

namespace RestServer {
    public class LowLevelServer : WsServer {
        #region Properties

        private readonly EndpointCollection _endpointCollection;
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);
        private readonly WsSessionCollection _wsSessions;

        /// <summary>
        /// Enable debug logging on this server instance.
        ///
        /// Please set via RestServer.debugLog. Leaving this open for users that want to modify or have a separate RestServer implementation.
        /// </summary>
        public bool DebugLog {
            get => _logger.logEnabled;
            set => _logger.logEnabled = value;
        }

        /// <summary>
        /// Special handlers allow to control server behaviour in case of endpoint exceptions, no endpoint found, etc.
        ///
        /// This shouldn't be set directly. Please set via RestServer.SpecialHandlers. Leaving this open for users that want to
        /// modify or have a separate RestServer implementation.
        /// </summary>
        public SpecialHandlers SpecialHandlers;

        #endregion

        #region Constructor

        public LowLevelServer(EndpointCollection endpointCollection, WsSessionCollection wsSessions, IPAddress address, int port,
            SpecialHandlers specialHandlers = null, bool debugLog = false, Action<Socket> additionalSocketConfigurationServer = null,
            Action<Socket> additionalSocketConfigurationSession = null)
            : base(address, port) {
            _endpointCollection = endpointCollection;
            _wsSessions = wsSessions;

            SpecialHandlers = specialHandlers ?? new SpecialHandlers();
            DebugLog = debugLog;
            AdditionalSocketConfigurationServer = additionalSocketConfigurationServer;
            AdditionalSocketConfigurationSession = additionalSocketConfigurationSession;
        }

        #endregion

        #region NetCoreServer Callbacks

        protected override TcpSession CreateSession() {
            _logger.Log("CreateSession for new incoming request");
            return new LowLevelSession(this, SpecialHandlers, DebugLog);
        }

        protected override void OnError(SocketError error) {
            _logger.Log($"OnError for underlying socket error {error}");
            SpecialHandlers?.LowLevelOnError?.Invoke(error);
        }

        #endregion

        #region WebSocket

        /// <summary>
        /// Internal: Register WebSocket sessions
        /// </summary>
        public void RegisterWsSession(WSEndpointId endpointId, LowLevelSession session) {
            _wsSessions.RegisterSession(endpointId, session);
        }

        /// <summary>
        /// Internal: Register WebSocket sessions
        /// </summary>
        public void DeregisterWsSession(WSEndpointId endpointId, LowLevelSession session) {
            _wsSessions.DeregisterSession(endpointId, session);
        }

        /// <summary>Send string to the clients on the given endpointId.</summary>
        public void WsSend(WSEndpointId endpointId, string message) {
            // prepare buffer
            var data = Encoding.UTF8.GetBytes(message);
            var dummyMask = new byte[4] { 0, 0, 0, 0 };

            var buffer = ThreadSafePrepareSendFrame(NetCoreServer.WebSocket.WS_FIN | NetCoreServer.WebSocket.WS_TEXT, false, data, 0, data.Length, dummyMask)
                .ToArray();

            foreach (var session in _wsSessions.GetSessions(endpointId)) {
                session.SendAsync(buffer);
            }
        }

        /// <summary>Send string to the clients on the given endpointId.</summary>
        public void WsSend(WSEndpointId endpointId, byte[] data) {
            // prepare buffer
            var dummyMask = new byte[4] { 0, 0, 0, 0 };

            var buffer = ThreadSafePrepareSendFrame(NetCoreServer.WebSocket.WS_FIN | NetCoreServer.WebSocket.WS_TEXT, false, data, 0, data.Length, dummyMask)
                .ToArray();

            foreach (var session in _wsSessions.GetSessions(endpointId)) {
                session.SendAsync(buffer);
            }
        }

        #region NetCoreServer Modifications

        /// <summary>
        /// Copy of PrepareSendFrame to make it completely thread safe.
        /// </summary>
        /// <param name="opcode">WebSocket opcode</param>
        /// <param name="mask">WebSocket mask</param>
        /// <param name="buffer">Buffer to send</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="size">Buffer size</param>
        /// <param name="maskBytes"></param>
        /// <param name="status">WebSocket status (default is 0)</param>
        protected List<byte> ThreadSafePrepareSendFrame(byte opcode, bool mask, byte[] buffer, long offset, long size, byte[] maskBytes, int status = 0) {
            var sendBuffer = new List<byte> { // Append WebSocket frame opcode
                opcode
            };

            // Append WebSocket frame size
            if (size <= 125)
                sendBuffer.Add((byte)(((int)size & 0xFF) | (mask ? 0x80 : 0)));
            else if (size <= 65535) {
                sendBuffer.Add((byte)(126 | (mask ? 0x80 : 0)));
                sendBuffer.Add((byte)((size >> 8) & 0xFF));
                sendBuffer.Add((byte)(size & 0xFF));
            } else {
                sendBuffer.Add((byte)(127 | (mask ? 0x80 : 0)));
                for (int i = 7; i >= 0; i--)
                    sendBuffer.Add((byte)((size >> (8 * i)) & 0xFF));
            }

            if (mask) {
                // Append WebSocket frame mask
                sendBuffer.Add(maskBytes[0]);
                sendBuffer.Add(maskBytes[1]);
                sendBuffer.Add(maskBytes[2]);
                sendBuffer.Add(maskBytes[3]);
            }

            // Resize WebSocket frame buffer
            var bufferOffset = sendBuffer.Count;
            sendBuffer.AddRange(new byte[size]);

            // Mask WebSocket frame content
            for (var i = 0; i < size; i++)
                sendBuffer[bufferOffset + i] = (byte)(buffer[offset + i] ^ maskBytes[i % 4]);

            return sendBuffer;
        }

        #endregion

        #endregion

        #region EndpointCollection

        /// <summary>Find endpoint definition for the given HttpMethod and url.</summary>
        /// <param name="method"></param>
        /// <param name="absoluteUrl"></param>
        /// <param name="ignoreTag">Ignore endpoints with the given tag</param>
        public Endpoint? FindEndpoint(HttpMethod? method, string absoluteUrl, object ignoreTag = null) {
            _logger.Log($"FindEndpoint for {method} and {absoluteUrl}.");
            return !method.HasValue ? null : _endpointCollection.FindEndpoint(method.Value, absoluteUrl, ignoreTag);
        }

        #endregion
    }
}