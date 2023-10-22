using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RestServer.Helper;
using RestServer.WebSocket;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace RestServer {
    /// <summary>
    ///     Main Entry point for this package. Use this to start the server and register endpoints with.
    ///     This component is also needed, so that async / sync work from the UnityThreadingHelper is executed.
    /// </summary>
    [AddComponentMenu("Rest Server/Rest Server")]
    [HelpURL("https://markus-seidl.de/unity-restserver/")]
    public class RestServer : MonoBehaviour, ISerializationCallbackReceiver {
        #region Properties

        /// <summary>
        /// Maximum number of ThreadingHelper workloads to be processed per frame. Default = 5. If set to zero the workload from the ThreadingHelper
        /// will not be processed.
        /// </summary>
        [Tooltip(
            "Maximum number of ThreadingHelper workloads to be processed per frame. Default = 5. If set to zero the workload from the ThreadingHelper will not be processed.")]
        public uint maxThreadingHelperWorkCount = 5;

        /// <summary>
        /// The port where the server will listen to. Can't be changed once the server has been started.
        /// If set to 0, the implementation will find a free port randomly on start. The port can be read back via EffectivePort.
        /// </summary>
        [Tooltip("The port to bind to. Note: Ports <1024 usually require root/administration rights to be able to bind to.")]
        public int port = 8080;

        /// <summary>
        /// Network interfaces which the server should bind to. Default = localhost. 
        /// </summary>
        /// <remarks>
        /// If you bind against public interfaces, you also have to take care about securing the server
        /// </remarks>
        public IPAddress ListenAddress {
            get {
                if (_listenAddress != null) {
                    return _listenAddress;
                }

                var ret = _listenAddressUnity.ToIPAddress();
                if (ret != null) {
                    return ret;
                }

                return IPAddress.Loopback;
            }
            set {
                if (value == null) {
                    value = IPAddress.Loopback;
                }

                _listenAddressUnity = value.ToListAddressUnityEnum();
                _listenAddress = value;
            }
        }

        private IPAddress _listenAddress;

        /// <summary>
        /// This allows the modification of the listenAddress via the inspector. It's serialized, whereas listenAddress is not.
        /// </summary>
        [SerializeField]
        private ListenAddressUnity _listenAddressUnity;

        /// <summary>
        /// Network interfaces which the server should bind to. Default = localhost. This is a convenience method for the unity inspector and serialization.
        /// </summary>
        /// <remarks>
        /// If you bind against public interfaces, you also have to take care about securing the server
        /// </remarks>
        public ListenAddressUnity ListenAddressUnity {
            get => _listenAddress != null ? _listenAddress.ToListAddressUnityEnum() : _listenAddressUnity;
            set {
                _listenAddressUnity = value;
                if (_listenAddressUnity != ListenAddressUnity.Unknown) {
                    _listenAddress = _listenAddressUnity.ToIPAddress();
                }
            }
        }

        /// <summary>
        /// After starting the server this property is set to the actual open port. This is always port, except if port = 0. In that case the OS chooses a
        /// random, free port.
        /// </summary>
        public int EffectivePort {
            get {
                if (Server?.Endpoint == null || Server.Endpoint.GetType() != typeof(IPEndPoint)) {
                    return -1;
                }

                return ((IPEndPoint)Server.Endpoint).Port;
            }
        }

        /// <summary>
        ///     Start the server automatically when the MonoBehaviour starts.
        /// </summary>
        [Tooltip("Autostart the server and bind to the configured port.")]
        public bool autostart = true;

        /// <summary>
        /// Disable the rest server if the component / GameObject is disabled?
        /// </summary>
        [Tooltip("Disable the rest server if the component / GameObject is disabled?")]
        public bool respectUnityDisable = true;

        [SerializeField]
        [Tooltip("Activate debug logging (verbose).")]
        private bool _debugLog;

        /// <summary>
        /// Access log is usually done by setting the function in the SpecialHandlers variable. But this can't be persisted when enabling it with the inspector,
        /// so this variable is used instead. It forces the access log to be enabled when the server is started and overrides the SpecialHandlers setting. 
        /// </summary>
        [Tooltip("Forces the access log to be enabled when the server is started and overrides the SpecialHandlers setting.")]
        public bool forceAccessLog;

        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        /// <summary>Endpoint collection which server responds to.</summary>
        public readonly EndpointCollection EndpointCollection = new EndpointCollection();

        /// <summary>Collection of all active endpoint sessions for each endpoint id</summary>
        public readonly WsSessionCollection WsSessionCollection = new WsSessionCollection();


        /// <summary>
        ///     State handling. When the server was running when OnDisable was called by unity, reenable it on OnEnable if autostart is enabled.
        /// </summary>
        private bool _wasRunningOnDisable;

        /// <summary>
        /// Reference to the server instance. If you need to modify this, please be aware of all effects.
        /// Server will be re-created upon a start/stop cycle.
        /// </summary>
        public LowLevelServer Server { get; private set; }

        /// <summary>
        /// Callback that's executed before the socket is bound. Allows additional socket configuration.
        /// Advanced, please handle with care.
        /// </summary>
        public Action<Socket> AdditionalSocketConfigurationServer {
            get => _additionalSocketConfigurationServer;
            set {
                _additionalSocketConfigurationServer = value;
                if (Server != null) {
                    Server.AdditionalSocketConfigurationServer = value;
                }
            }
        }

        private Action<Socket> _additionalSocketConfigurationServer;

        /// <summary>
        /// Callback that's executed before the session is established. Allows additional session socket configuration.
        /// Advanced, please handle with care.
        /// </summary>
        public Action<Socket> AdditionalSocketConfigurationSession {
            get => _additionalSocketConfigurationSession;
            set {
                _additionalSocketConfigurationSession = value;
                if (Server != null) {
                    Server.AdditionalSocketConfigurationSession = value;
                }
            }
        }

        private Action<Socket> _additionalSocketConfigurationSession;


        /// <summary>
        ///     Enable verbose logging inside the library. Can be enabled / disabled during runtime, but it's only applied to newly created requests.
        /// </summary>
        public bool DebugLog {
            get => _debugLog;
            set {
                // propagate to other instances
                if (Server != null) {
                    Server.DebugLog = value;
                }

                if (EndpointCollection != null) {
                    EndpointCollection.DebugLog = value;
                }

                _logger.logEnabled = value;
            }
        }

        public bool IsStarted => Server != null && Server.IsStarted;

        /// <summary>
        ///     Special handlers allow to control server behaviour in case of endpoint exceptions, no endpoint found, etc.
        /// </summary>
        public SpecialHandlers SpecialHandlers {
            get => _specialHandlers;
            set {
                // propagate to other instances
                _specialHandlers = value;
                if (Server != null) {
                    Server.SpecialHandlers = _specialHandlers;
                }
            }
        }

        private SpecialHandlers _specialHandlers = new SpecialHandlers();

        #endregion

        #region General Methods

        public RestServer() {
            EndpointCollection.DebugLog = _debugLog;
        }


        /// <summary>
        ///     Start the server. Called automatically upon MonoBehaviour#start when #autostart is enabled.
        /// </summary>
        public virtual void StartServer() {
            Server?.Stop();

            Server = new LowLevelServer(
                EndpointCollection, WsSessionCollection, ListenAddress, port, _specialHandlers, DebugLog,
                AdditionalSocketConfigurationServer, AdditionalSocketConfigurationSession
            );
            var portStr = port == 0 ? "<random port>" : $"{port}";
            _logger.Log($"Starting server on {ListenAddress}:{portStr} with debug enabled: {DebugLog} on GameObject {gameObject.name}");
            if (port < 1024 && port != 0) {
                _logger.LogWarning("Ports <= 1024 usually require administrator or root rights. If binding fails, try a port >1024.", this);
            }

            try {
                Server.Start();
                _logger.Log($"Server started on {ListenAddress}:{EffectivePort}");
            }
            catch (SocketException se) {
                if (se.Message.StartsWith("Only one usage of each socket address")) {
                    // It seems that throwing an exception after _logger.LogXXX call doesn't show up in the unity console (as it should).
                    Debug.LogWarning("There are probably two RestServer components in the scene. " +
                                     $"While that is possible, both of them can't be bound to the same port. Error on GameObject ${gameObject.name}");
                }

                throw;
            }
        }

        /// <summary>
        ///     Stop the internal server.
        /// </summary>
        public virtual void StopServer() {
            if (Server != null && Server.IsStarted) {
                _logger.Log("Stopping server.");
            }

            Server?.Stop();
        }

        /// <summary>
        ///     Execute workloads assigned to the Threading Helper in the main thread (= unity render thread). Automatically called from MonoBehaviour#Update
        /// </summary>
        /// <param name="workload"></param>
        public virtual void DoUpdate(Workload workload) {
            using (var marker = new ProfilerMarker(workload.ProfileMarkerText).Auto()) {
                _logger.Log($"Executing workload for {workload.ProfileMarkerText}");
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
                var executionTime = Stopwatch.StartNew();
#endif
                // Sync / Async
                if (workload.HandlerAction != null) {
                    try {
                        workload._ReturnValue = workload.HandlerAction.Invoke();
                    }
                    catch (Exception e) {
                        workload._Exception = e;
                    }

                    workload.WaitHandle?.Set(); // Sync workload, signal completion

                    if (workload.WaitHandle == null && workload._Exception != null) {
                        // Async workload, handle error
                        SpecialHandlers.AsynchronousExceptionHandler(workload._Exception);
                    }
                }

                // Coroutine
                if (workload.HandlerCoroutine != null) {
                    try {
                        StartCoroutine(workload.HandlerCoroutine());
                    }
                    catch (Exception e) {
                        // This only catches exception on the first invocation, everything else is handled by StartCoroutine()
                        workload._Exception = e;
                        SpecialHandlers.AsynchronousExceptionHandler(workload._Exception);
                    }
                }
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
                RestServerProfilerCounters.ThreadingHelperMainThreadBlockTime.Value += executionTime.ElapsedMilliseconds;
#endif
            }
        }

        #endregion

        #region WebSocket Methods

        /// <summary>Send message to all clients connected to the given endpointId.</summary>
        /// <param name="endpointId">The endpointId generated when registering the endpoint</param>
        /// <param name="message">The message to send.</param>
        public void WsSend(WSEndpointId endpointId, string message) {
            Server.WsSend(endpointId, message);
        }

        /// <summary>Send data to all clients connected to the given endpointId.</summary>
        /// <param name="endpointId">The endpointId generated when registering the endpoint</param>
        /// <param name="data">The data to send.</param>
        public void WsSend(WSEndpointId endpointId, byte[] data) {
            Server.WsSend(endpointId, data);
        }

        #endregion

        #region Unity Methods

        private void Awake() {
            ThreadingHelper.Instance.MainThreadReference = Thread.CurrentThread;
        }

        private void Start() {
            if (forceAccessLog) {
                SpecialHandlers.AccessLog = DefaultRequestHandlerImpl.DebugLogAccessLog;
            }

            if (autostart) {
                StartServer();
            }
        }

        private void Update() {
            using (var marker = new ProfilerMarker("RestServer.Execute").Auto()) {
                var th = ThreadingHelper.Instance;
                for (var i = 0; i < maxThreadingHelperWorkCount; i++) {
                    if (!th.HasWorkload()) {
                        break;
                    }

                    var workload = th.DequeueWork();
                    DoUpdate(workload);
                }
            }
        }

        public void OnValidate() {
            // Allowed port range is 1–65535, but many systems (for example *nix based) don't allow access to ports <1024 if you don't run as root.
            // 0 is also allowed, for random ports
            port = Math.Min(Math.Max(0, port), 65535);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            DebugLog = DebugLog; // Propagate inspector settings to all classes, when unity loads the scene 
        }

        public void OnDisable() {
            if (respectUnityDisable) {
                _wasRunningOnDisable = Server != null ? Server.IsStarted : false;
                StopServer();
            }
        }

        public void OnEnable() {
            if (respectUnityDisable) {
                if (_wasRunningOnDisable && autostart) {
                    StartServer();
                }
            }
        }

        #endregion
    }

    #region Inspector Helper

    /// <summary>
    /// Unity friendly wrapper of IPAddress.Any, Loopback and IPv6
    /// </summary>
    public enum ListenAddressUnity {
        Loopback,
        Any,
        AnyIPv6,
        Unknown
    }

    public static class ListenAddressUnityExtension {
        public static IPAddress ToIPAddress(this ListenAddressUnity e) {
            switch (e) {
                case ListenAddressUnity.Loopback:
                    return IPAddress.Loopback;
                case ListenAddressUnity.Any:
                    return IPAddress.Any;
                case ListenAddressUnity.AnyIPv6:
                    return IPAddress.IPv6Any;
            }

            return null;
        }

        public static ListenAddressUnity ToListAddressUnityEnum(this IPAddress ip) {
            if (Equals(IPAddress.Any, ip)) {
                return ListenAddressUnity.Any;
            }

            if (Equals(IPAddress.Loopback, ip)) {
                return ListenAddressUnity.Loopback;
            }

            if (Equals(IPAddress.IPv6Any, ip)) {
                return ListenAddressUnity.AnyIPv6;
            }

            return ListenAddressUnity.Unknown;
        }
    }

    #endregion
}