using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using RestServer.Helper;
using UnityEngine;

namespace RestServer {
    /// <summary>
    /// Class which helps registering the contents of a supplied zip file to the StaticContentHandler.
    ///
    /// This class should only be used to create / fill a StaticContentHandler+RestServer wih all necessary information and then discarded. One time use only.
    /// </summary>
    /// <remarks>
    /// Be aware of unity binary file restrictions https://docs.unity3d.com/Manual/class-TextAsset.html .
    /// Use ".bytes" as file extension for zip files.
    /// </remarks>
    public class StaticContentBuilder {
        private readonly Logger _logger = new Logger(Debug.unityLogger.logHandler);

        /// <summary>Content handler class which will receive the requests.</summary>
        public StaticContentHandler ContentHandler;

        /// <summary>
        /// If a file with that name exists, it will be served as index file of the directory containing it.
        ///
        /// A request of http://localhost:8080/blah/ will deliver the file /blah/index.html if "index.html" is listed as TryIndexFile.
        /// </summary>
        public readonly List<string> TryIndexFiles = new List<string>();

        /// <summary>
        /// This maps extensions to Http Content Types. The key is the file extension (without the .) and the value is the Http Content Type to use.
        /// </summary>
        public readonly Dictionary<string, string> FileExtensionContentTypes = new Dictionary<string, string>();

        /// <summary>
        /// This maps content types to the fact, if the content should be treated as binary and handled as such. The key is the Http Content Type and the value
        /// is true if binary and false if handled as text.
        /// </summary>
        public readonly Dictionary<string, bool> ContentTypeBinary = new Dictionary<string, bool>();

        /// <summary>
        /// Filename patterns that are ignored when registering content from zip files. The pattern has to match against the filename for the file to be ignored.
        /// Make sure to write your regex like "^(?:foo|bar)$" to ensure the pattern matches the whole string.
        /// </summary>
        public readonly List<Regex> ZipIgnorePatterns = new List<Regex>();

        /// <summary>
        /// Reference to the rest server which the content should be registered with.
        /// </summary>
        public RestServer RestServer;

        /// <summary>Path under which the assets are registered (root-path + sub-path = path).</summary>
        public string RootPath;

        /// <summary>Map index.htm(l) assets to /</summary>
        public bool MapIndexHtml;

        /// <summary>
        /// Assets that should be registered.
        /// </summary>
        public readonly List<StaticContentBuilderAsset> Assets = new List<StaticContentBuilderAsset>();

        /// <summary>Returns if the build has completed. Useful to check if the coroutine has finished.</summary>
        public bool IsBuildDone { get; protected set; }

        /// <summary>Default ctor.</summary>
        public StaticContentBuilder() {
            TryIndexFiles.Add("index.html");
            TryIndexFiles.Add("index.htm");

            ZipIgnorePatterns.Add(new Regex(@"^(.*~)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(~\$.*)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(\..*)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(~.*\.tmp)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(.*\.~*)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(Thumbs.db)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(System Volume Information)$"));
            ZipIgnorePatterns.Add(new Regex(@"^(\..*)$"));


            // default extension
            RegisterDefaultContentTypes(FileExtensionContentTypes, ContentTypeBinary);
        }

        /// <summary>
        /// The rest server to register the content with. 
        /// </summary>
        public virtual StaticContentBuilder WithRestServer(RestServer restServer) {
            RestServer = restServer;
            return this;
        }

        /// <summary>
        /// The content handler to use. The content handler is called everytime a request is made against a path of a file.
        /// </summary>
        public virtual StaticContentBuilder WithContentHandler(StaticContentHandler contentHandler) {
            ContentHandler = contentHandler;
            return this;
        }

        /// <summary>Path under which the assets are registered (root-path + sub-path = path).</summary>
        public virtual StaticContentBuilder WithRootPath(string rootPath) {
            RootPath = rootPath;
            return this;
        }

        /// <summary>Map index.htm(l) assets to /</summary>
        public virtual StaticContentBuilder WithMapIndexHtml(bool mapIndexHtml) {
            MapIndexHtml = mapIndexHtml;
            return this;
        }

        /// <summary>
        /// Register a zip file that is extracted and then all the content is registered under the specified subpath.
        /// </summary>
        public virtual StaticContentBuilder WithAssetZip(TextAsset asset, string subPath) {
            return WithAsset(asset, true, false, subPath, null);
        }

        /// <summary>
        /// Register the text asset at the subpath with the specified mime type.
        /// </summary>
        public virtual StaticContentBuilder WithAssetText(TextAsset asset, string subPath, string contentType = MimeType.TEXT_HTML) {
            return WithAsset(asset, true, false, subPath, contentType);
        }

        /// <summary>
        /// Register the binary asset (must have the '.bytes' suffix in Unity) at the subpath with the specified mime type.
        /// </summary>
        public virtual StaticContentBuilder WithAssetBinary(TextAsset asset, string subPath, string contentType = MimeType.APPLICATION_OCTET_STREAM) {
            return WithAsset(asset, true, false, subPath, contentType);
        }

        /// <summary>
        /// Disable logging when registering content. 
        /// </summary>
        public virtual StaticContentBuilder WithDisabledLogging() {
            _logger.logEnabled = false;
            return this;
        }

        /// <summary>
        /// Register the content. 
        /// </summary>
        /// <param name="asset">Reference to the unity asset to register.</param>
        /// <param name="isBinary">Use unity's binary methods to extract the data from the asset. Has to be used when the original file is binary.</param>
        /// <param name="isZip">Assume the asset is a zip file. Extract the zip file and register each file individually.</param>
        /// <param name="subPath">Locate the asset at this path</param>
        /// <param name="contentType">The content type header to send to the browser when delivering this asset</param>
        public virtual StaticContentBuilder WithAsset(TextAsset asset, bool isBinary, bool isZip, string subPath, string contentType) {
            if (asset == null) {
                throw new ArgumentNullException(nameof(asset));
            }

            if (string.IsNullOrEmpty(subPath)) {
                throw new ArgumentNullException(nameof(subPath));
            }

            if (string.IsNullOrEmpty(contentType) && !isZip) {
                throw new ArgumentNullException(nameof(contentType));
            }

            Assets.Add(new StaticContentBuilderAsset(asset, isBinary, isZip, subPath, contentType));
            return this;
        }

        /// <summary>
        /// Register all content (build all endpoints) synchronously. This means that a zip file, if registered, will be extracted inside a single frame.
        /// Note that this method can not be called from a thread, as the method on the TextAsset are not multi-thread-able. Use BuildAsync with Coroutines instead.
        /// </summary>
        public virtual void BuildSync() {
            Validate();

            foreach (var asset in Assets) {
                var itemPath = AdjustPathForIndex(PathHelper.ConcatPath(RootPath, asset.SubPath));

                if (asset.IsZip) {
                    foreach (var unused in HandleZip(asset)) { }
                } else if (asset.IsBinary) {
                    RegisterBinaryAsset(asset, itemPath);
                } else {
                    RegisterTextAsset(asset, itemPath);
                }
            }

            IsBuildDone = true;
        }

        protected virtual void RegisterTextAsset(StaticContentBuilderAsset asset, string itemPath) {
            var contentType = string.IsNullOrEmpty(asset.ContentType) ? MimeType.TEXT_HTML : asset.ContentType;
            var content = new StaticContentEntry(asset.SubPath, null, asset.Asset.text, contentType, ContentHandler);
            RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, itemPath, ContentHandler.EndpointCallback, content);
        }

        protected virtual void RegisterBinaryAsset(StaticContentBuilderAsset asset, string itemPath) {
            var contentType = string.IsNullOrEmpty(asset.ContentType) ? MimeType.APPLICATION_OCTET_STREAM : asset.ContentType;
            var content = new StaticContentEntry(asset.SubPath, asset.Asset.bytes, null, contentType, ContentHandler);
            RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, itemPath, ContentHandler.EndpointCallback, content);
        }

        /// <summary>
        /// Register all content (build all endpoints) inside a coroutine. Has to be called with StartCoroutine or a method that is encapsulating this.
        /// Useful if large zip files need to be processed or the startup should be improved. Note that not all content will be available until this method
        /// has finished.
        /// </summary>
        public virtual IEnumerator BuildCoroutine(int stepsPerYield = 1, YieldInstruction yieldInstruction = null, Action doneCallBack = null) {
            Validate();

            if (yieldInstruction == null) {
                yieldInstruction = CICD.SafeWaitForEndOfFrame();
            }

            var step = 0;
            foreach (var asset in Assets) {
                var itemPath = AdjustPathForIndex(PathHelper.ConcatPath(RootPath, asset.SubPath));

                if (asset.IsZip) {
                    foreach (var unused in HandleZip(asset)) {
                        if (step++ % stepsPerYield == (stepsPerYield - 1)) {
                            yield return yieldInstruction;
                        }
                    }
                } else {
                    if (asset.IsBinary) {
                        RegisterBinaryAsset(asset, itemPath);
                    } else {
                        RegisterTextAsset(asset, itemPath);
                    }

                    if (step++ % stepsPerYield == (stepsPerYield - 1)) {
                        yield return yieldInstruction;
                    }
                }
            }

            IsBuildDone = true;
            if (doneCallBack != null) {
                doneCallBack.Invoke();
            }
        }

        /// <summary>
        /// Clear registered assets.
        /// </summary>
        public virtual void Clear() {
            Assets.Clear();
        }

        /// <summary>
        /// Worker method which extracts the zip file and registers the content.
        /// </summary>
        protected virtual IEnumerable HandleZip(StaticContentBuilderAsset cbAsset) {
            ZipArchive zipArchive;
            try {
                zipArchive = new ZipArchive(new MemoryStream(cbAsset.Asset.bytes));
            }
            catch (Exception e) {
                _logger.LogError(GetType().Name, $"Couldn't extract zip file from asset with name {cbAsset.Asset.name}");
                _logger.LogException(e);
                throw;
            }

            yield return null;

            foreach (var entry in zipArchive.Entries) {
                try {
                    if (entry.FullName.EndsWith("/")) {
                        // This zip implementation doesn't really give hints about what type the entry has. Filter directories by assuming they end with "/"
                        continue;
                    }

                    if (IsIgnored(entry.Name)) {
                        _logger.Log(
                            $"Ignoring {entry.FullName} from asset {cbAsset.Asset.name}, subPath {cbAsset.SubPath} because it matches ZipIgnorePatterns.");
                        continue;
                    }

                    var itemPath = AdjustPathForIndex(PathHelper.EnsureSlashPrefix(
                        PathHelper.ConcatPath(RootPath, PathHelper.ConcatPath(cbAsset.SubPath, entry.FullName))
                    ));

                    var contentType = ContentTypeForFilename(entry.FullName, out var binaryContent);
                    _logger.Log($"Extracting {entry.FullName} from asset {cbAsset.Asset.name}, subPath {cbAsset.SubPath}, " +
                                $"resultingPath {itemPath}, contentType {contentType}");

                    if (binaryContent) {
                        var b = new BinaryReader(entry.Open());
                        var bytes = b.ReadBytes(int.MaxValue);

                        var content = new StaticContentEntry(itemPath, bytes, null, contentType, ContentHandler);
                        RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, itemPath, ContentHandler.EndpointCallback, content);
                    } else {
                        var s = new StreamReader(entry.Open());
                        var txt = s.ReadToEnd();

                        var content = new StaticContentEntry(itemPath, null, txt, contentType, ContentHandler);
                        RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, itemPath, ContentHandler.EndpointCallback, content);
                    }
                }
                catch (Exception e) {
                    _logger.LogError(GetType().Name, $"Error while adding {entry.FullName}");
                    _logger.LogException(e);
                }

                yield return null;
            }
        }

        protected virtual bool IsIgnored(string filename) {
            foreach (var ignorePattern in ZipIgnorePatterns) {
                if (ignorePattern.IsMatch(filename)) {
                    return true;
                }
            }

            return false;
        }

        protected virtual void Validate() {
            if (ContentHandler == null) {
                throw new ArgumentNullException(nameof(ContentHandler));
            }

            if (RestServer == null) {
                throw new ArgumentNullException(nameof(RestServer));
            }
        }

        protected virtual string AdjustPathForIndex(string path) {
            if (!MapIndexHtml) {
                return path;
            }

            foreach (var tryIndexFile in TryIndexFiles) {
                if (path.EndsWith("/" + tryIndexFile)) {
                    return PathHelper.EnsureSlashPrefix(path.Remove(path.Length - tryIndexFile.Length));
                }
            }

            return path;
        }

        protected static void RegisterDefaultContentTypes(Dictionary<string, string> typeMap, Dictionary<string, bool> binaryMap) {
            foreach (var kv in MimeTypeDict.MIME_TYPE_MAPPINGS) {
                typeMap.Add(kv.Key, kv.Value);
                if (!binaryMap.ContainsKey(kv.Value)) {
                    binaryMap.Add(kv.Value, MimeTypeDict.IsBinary(kv.Value));
                }
            }

            // typeMap.Add("js", MimeType.APPLICATION_JAVASCRIPT);
            // typeMap.Add("json", MimeType.APPLICATION_JSON);
            // typeMap.Add("pdf", MimeType.APPLICATION_PDF);
            // typeMap.Add("rtf", MimeType.APPLICATION_RTF);
            // typeMap.Add("xhtml", MimeType.APPLICATION_XHTML);
            // typeMap.Add("gif", MimeType.IMAGE_GIF);
            // typeMap.Add("jpeg", MimeType.IMAGE_JPEG);
            // typeMap.Add("jpg", MimeType.IMAGE_JPEG);
            // typeMap.Add("jpe", MimeType.IMAGE_JPEG);
            // typeMap.Add("png", MimeType.IMAGE_PNG);
            // typeMap.Add("tiff", MimeType.IMAGE_TIFF);
            // typeMap.Add("tif", MimeType.IMAGE_TIFF);
            // typeMap.Add("css", MimeType.TEXT_CSS);
            // typeMap.Add("htm", MimeType.TEXT_HTML);
            // typeMap.Add("html", MimeType.TEXT_HTML);
            // typeMap.Add("csv", MimeType.TEXT_CSV);
            // typeMap.Add("xml", MimeType.TEXT_XML);
            // typeMap.Add("bin", MimeType.APPLICATION_OCTET_STREAM);
            //
            // const bool text = false;
            // const bool binary = true;
            // binaryMap.Add(MimeType.APPLICATION_JAVASCRIPT, text);
            // binaryMap.Add(MimeType.APPLICATION_JSON, text);
            // binaryMap.Add(MimeType.APPLICATION_XHTML, text);
            // binaryMap.Add(MimeType.TEXT_CSS, text);
            // binaryMap.Add(MimeType.TEXT_HTML, text);
            // binaryMap.Add(MimeType.TEXT_CSV, text);
            // binaryMap.Add(MimeType.TEXT_XML, text);
            //
            // binaryMap.Add(MimeType.APPLICATION_PDF, binary);
            // binaryMap.Add(MimeType.APPLICATION_RTF, binary);
            // binaryMap.Add(MimeType.APPLICATION_OCTET_STREAM, text);
            // binaryMap.Add(MimeType.IMAGE_GIF, binary);
            // binaryMap.Add(MimeType.IMAGE_JPEG, binary);
            // binaryMap.Add(MimeType.IMAGE_PNG, binary);
            // binaryMap.Add(MimeType.IMAGE_TIFF, binary);
        }

        protected virtual string ContentTypeForFilename(string fileName, out bool isBinary) {
            var ext = Path.GetExtension(fileName);

            if (ext.StartsWith(".")) {
                ext = ext.Remove(0, 1);
            }

            isBinary = false;
            if (FileExtensionContentTypes.TryGetValue(ext, out var contentType)) {
                ContentTypeBinary.TryGetValue(contentType, out isBinary);
                return contentType;
            }

            return MimeType.TEXT_PLAIN;
        }
    }

    /// <summary>
    /// Static content description for StaticContentBuilder
    /// </summary>
    public struct StaticContentBuilderAsset {
        public readonly TextAsset Asset;
        public readonly bool IsZip;
        public readonly bool IsBinary;
        public readonly string SubPath;
        public readonly string ContentType;

        public StaticContentBuilderAsset(TextAsset asset, bool isBinary, bool isZip, string subPath, string contentType) {
            Asset = asset;
            IsZip = isZip;
            IsBinary = isBinary;
            SubPath = subPath;
            ContentType = contentType;
        }
    }
}