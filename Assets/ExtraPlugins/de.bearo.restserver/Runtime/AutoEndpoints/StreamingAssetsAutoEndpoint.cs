using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using RestServer.Helper;
using UnityEngine;
using UnityEngine.Networking;

namespace RestServer.AutoEndpoints {
    [AddComponentMenu("Rest Server/StreamingAssets Auto Endpoint - WebServer")]
    [HelpURL("https://markus-seidl.de/unity-restserver/doc/60_autoendpoints/#streamingassets-auto-endpoint")]
    public class StreamingAssetsAutoEndpoint : AbstractAutoEndpoint {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        private readonly ConcurrentQueue<SAAFileRequest> _proxyRequestStack = new ConcurrentQueue<SAAFileRequest>();

        [Tooltip("Sub folder of the streaming assets folder to serve. All files found in this folder will be served. Must start with '/'.")]
        public string streamingAssetSubFolder = "/";

        [Tooltip("Map paths from incoming requests that end with a slash to an index.htm(l), if this file exists.")]
        public bool mapIndexHtml = true;

        [Tooltip("File paths to append to '/' requests. If the file does not exist, the next file will be tried. Only used if mapIndexHtml is true.")]
        public List<string> TryFiles = new List<string>() {
            "index.html",
            "index.htm",
        };

        [Tooltip("Maximum time to wait until the StreamingAsset is loaded from the APK (Android only).")]
        public int backgroundRequestTimeout = 5000;

        /// <summary>
        /// Tag used to identify the endpoint for de-registration/redirects.
        /// </summary>
        private readonly object _endpointTag = new object();

        #region De/Register

        public override void Register() {
            _logger.logEnabled = restServer.DebugLog;

            var assetRootPath = PathHelper.ConcatPath(Application.streamingAssetsPath, streamingAssetSubFolder);
            var endpointRegex = new Regex($"{PathHelper.RemoveEndingSlash(endpointPath)}/.*");
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, endpointRegex, request => {
                var filePath = PathHelper.ConcatPath(assetRootPath, request.RequestUri.AbsolutePath);

                if (filePath.StartsWith("jar")) {
                    ResolveRequestForJar(request, filePath, _endpointTag);
                } else {
                    ResolveRequestForFiles(request, filePath, _endpointTag);
                }
            }, _endpointTag);
        }


        public override void Deregister() {
            restServer.EndpointCollection.DeleteByTag(_endpointTag);
        }

        #endregion

        #region Android

        /// <summary>
        /// Handle incoming request if platform is Android.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filePath"></param>
        /// <param name="ignoreTag"></param>
        protected virtual void ResolveRequestForJar(RestRequest request, string filePath, object ignoreTag) {
            // Android only, need to load the file from the APK
            SAAFileResponse fileResponse;

            if (mapIndexHtml && request.RequestUri.AbsolutePath.EndsWith("/")) {
                foreach (var tryFile in TryFiles) {
                    var tryFilePath = PathHelper.ConcatPath(filePath, tryFile);
                    fileResponse = HandoffToMainThread(tryFilePath);
                    if (fileResponse.IsSuccess) {
                        filePath = tryFilePath;
                        break;
                    }
                }
            }

            _logger.Log($"Trying to serving file {filePath} from StreamingAssets folder");

            fileResponse = HandoffToMainThread(filePath);
            if (!fileResponse.IsSuccess) {
                _logger.Log($"{filePath} not found in file system, redirecting internally to other endpoints or 404.");
                // There is no asset with the same path, redirect internally to see if there is another endpoint defined for this path 
                request.ScheduleInternalRedirect(request.RequestUri.AbsolutePath, ignoreTag);
                return;
            }

            var mimeType = MimeType.APPLICATION_OCTET_STREAM; // fallback to binary
            if (MimeTypeDict.MIME_TYPE_MAPPINGS.TryGetValue(Path.GetExtension(filePath), out var foundMimeType)) {
                mimeType = foundMimeType;
            }

            request.CreateResponse()
                .Body(fileResponse.Data, mimeType)
                .SendAsync();
        }

        protected virtual SAAFileResponse HandoffToMainThread(string uri) {
            var pr = new SAAFileRequest(uri);

            _proxyRequestStack.Enqueue(pr);
            // Wait for the request to finish on the main thread, or timeout if it takes too long (timeout is also used, when the main thread request fails)
            // The timeout can also occur, if the proxied resource is responding too slow or too large to download in backgroundRequestTimeout.
            pr.WaitHandle.WaitOne(backgroundRequestTimeout);

            return pr.FileResponse;
        }

        /// <summary>
        /// Android: load the file from the StreamingAssets folder in the background
        /// </summary>
        public void Update() {
            if (_proxyRequestStack.IsEmpty) {
                return;
            }

            while (_proxyRequestStack.TryDequeue(out var request)) {
                StartCoroutine(DoRequest(request));
            }
        }

        /// <summary>
        /// Load the file from the StreamingAssets folder in the background, needed for Android
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <returns></returns>
        IEnumerator DoRequest(SAAFileRequest fileRequest) {
            _logger.Log("Extracting file from APK: " + fileRequest.RequestUri);

            using var uwr = UnityWebRequest.Get(fileRequest.RequestUri);
            yield return uwr.SendWebRequest();

            _logger.Log("Extracting file form APK finished: " + fileRequest.RequestUri + " with: " + uwr.result);

            fileRequest.FileResponse = new SAAFileResponse(uwr);
            fileRequest.WaitHandle.Set();
        }

        #endregion

        #region Other Platforms

        /// <summary>
        /// Handle incoming request for all other platforms.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filePath"></param>
        /// <param name="ignoreTag"></param>
        protected virtual void ResolveRequestForFiles(RestRequest request, string filePath, object ignoreTag) {
            if (mapIndexHtml && request.RequestUri.AbsolutePath.EndsWith("/")) {
                foreach (var tryFile in TryFiles) {
                    var tryFilePath = PathHelper.ConcatPath(filePath, tryFile);
                    if (File.Exists(tryFilePath)) {
                        filePath = tryFilePath;
                        break;
                    }
                }
            }

            _logger.Log($"Trying to serving file {filePath} from StreamingAssets folder");

            if (!File.Exists(filePath)) {
                _logger.Log($"{filePath} not found in file system, redirecting internally to other endpoints or 404.");
                // There is no asset with the same path, redirect internally to see if there is another endpoint defined for this path 
                request.ScheduleInternalRedirect(request.RequestUri.AbsolutePath, ignoreTag);
                return;
            }

            var mimeType = MimeType.APPLICATION_OCTET_STREAM; // fallback to binary
            if (MimeTypeDict.MIME_TYPE_MAPPINGS.TryGetValue(Path.GetExtension(filePath), out var foundMimeType)) {
                mimeType = foundMimeType;
            }

            var fileBytes = File.ReadAllBytes(filePath);
            request.CreateResponse()
                .Body(fileBytes, mimeType)
                .SendAsync();
        }

        #endregion

        #region Helper Classes

        public class SAAFileRequest {
            public readonly string RequestUri;
            public readonly AutoResetEvent WaitHandle;
            public SAAFileResponse FileResponse;

            public SAAFileRequest(string requestUri) {
                RequestUri = requestUri;
                WaitHandle = new AutoResetEvent(false);
            }
        }

        public class SAAFileResponse {
            public readonly long ResponseCode;
            public readonly byte[] Data;

            public bool IsSuccess => ResponseCode >= 200 && ResponseCode < 300;

            public SAAFileResponse(UnityWebRequest uwr) {
                ResponseCode = uwr.responseCode;
                Data = uwr.downloadHandler.data;
            }
        }

        #endregion
    }
}