using System;
using UnityEngine;

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Endpoint that can easily serve static content. 
    /// </summary>
    [AddComponentMenu("Rest Server/Static Content Auto Endpoint")]
    [HelpURL("https://markus-seidl.de/unity-restserver/doc/60_autoendpoints/#static-content-auto-endpoint")]
    public class StaticContentAutoEndpoint : MonoBehaviour {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        [Tooltip("Reference to the rest server to use. Tries to determine it if the rest server is on the same GameObject.")]
        public RestServer restServer;

        [Tooltip("Root path where all files are served from. Must start with '/'.")]
        public string rootPath = "/";

        [Tooltip("Map paths from incoming requests that end with a slash to an index.htm(l), if this file exists.")]
        public bool mapIndexHtml = true;

        [Tooltip("Asynchronously initialise all static content by using coroutines. Content will take some time before it's ready.")]
        public bool useCoroutineInit = true;

        [Tooltip("Log more information about the initialization process.")]
        public bool debugLog = true;

        // [Tooltip("Some web frameworks require that 404 is redirecting to index.html, specify how this should be done. Leave empty to disable. " +
        //          "Note: This overwrites the Not Found special handler on the rest server.")]
        // public string map404To;

        [Tooltip("Files or zip files to serve")]
        public StaticContentAEEntry[] files;

        private bool _isRegistered;

        protected readonly StaticContentHandler Handler = new StaticContentHandler();
        
        /// <summary>
        /// Builder that is used to configure the rest server with the configuration of this class
        /// </summary>
        public readonly StaticContentBuilder Builder = new StaticContentBuilder();

        /// <summary>Returns if the build has completed. Useful to check if the coroutine has finished.</summary>
        public bool IsBuildDone { get; protected set; }

        private void Start() {
            if (restServer == null) {
                // Try to find rest server if it's the same component
                restServer = GetComponent<RestServer>();
            }

            if (restServer == null) {
                _logger.LogError("No rest server instance could be found.", this);
                return;
            }

            if (string.IsNullOrEmpty(rootPath)) {
                rootPath = "/";
            }

            if (files == null) {
                return;
            }

            Register();
        }

        public void OnDisable() {
            if (restServer == null) {
                return;
            }

            if (string.IsNullOrEmpty(rootPath)) {
                return;
            }

            Deregister();
        }

        public void OnEnable() {
            Start();
        }

        /// <summary>
        /// Called on start if restServer and endpointPath is set. Register needed endpoints.
        /// </summary>
        public void Register() {
            if (_isRegistered) {
                return;
            }

            Handler.ClearContent(restServer);

            ConfigureStaticContentBuilder();

            var i = 0;
            foreach (var file in files) {
                if (file.asset == null) {
                    Debug.LogError($"No asset provided for path <{file.subPath}>. Skipping asset at index {i}");
                    i += 1;
                    continue;
                }

                if (string.IsNullOrEmpty(file.subPath)) {
                    file.subPath = "/";
                }

                if (string.IsNullOrEmpty(file.contentType) && !file.isZip) {
                    Debug.LogError($"No content type provided for path <{file.subPath}>, skipping asset at index {i}");
                    i += 1;
                    continue;
                }

                Builder.WithAsset(file.asset, file.isBinary, file.isZip, file.subPath, file.contentType);

                i += 1;
            }

            if (useCoroutineInit) {
                StartCoroutine(Builder.BuildCoroutine(doneCallBack: () => IsBuildDone = true));
            }
            else {
                Builder.BuildSync();
                IsBuildDone = true;
            }

            _isRegistered = true;
        }

        /// <summary>
        /// Overwrite to customize the StaticContentBuilder and register custom types, etc.
        /// </summary>
        protected virtual void ConfigureStaticContentBuilder() {
            Builder.Clear();
            
            Builder.WithRestServer(restServer).WithContentHandler(Handler);
            Builder.WithMapIndexHtml(mapIndexHtml).WithRootPath(rootPath);
            if (!debugLog) {
                Builder.WithDisabledLogging();
            }
        }

        /// <summary>
        /// Called OnDisable. Remove all registered endpoints. 
        /// </summary>
        public void Deregister() {
            Handler.ClearContent(restServer);
            StopAllCoroutines();
            _isRegistered = false;
            IsBuildDone = false;
        }

        // /// <summary>
        // /// True if the subpath is at least twice in the files collection; false otherwise.
        // /// </summary>
        // /// <param name="subPath">subpath to check</param>
        // /// <returns>True if the subpath is at least twice in the files collection; false otherwise.</returns>
        // public bool IsDuplicate(string subPath) {
        //     var count = 0;
        //     foreach (var file in files) {
        //         if (file.subPath == subPath) {
        //             count++;
        //         }
        //
        //         if (count >= 2) {
        //             return true;
        //         }
        //     }
        //
        //     return false;
        // }
    }

    [Serializable]
    public class StaticContentAEEntry {
        public string subPath = "/";

        [Tooltip("Asset to serve. If IsZip is true, the zip file is extracted and the contents is served.")]
        public TextAsset asset;

        public bool isZip;

        public bool isBinary;

        [Tooltip("Content Type to use when serving the asset. Not applicable when IsZip is true.")]
        public string contentType;
    }
}