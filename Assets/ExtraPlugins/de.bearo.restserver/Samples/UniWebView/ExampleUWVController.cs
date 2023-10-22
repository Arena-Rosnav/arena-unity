// un-comment the line below if you have the uniwebview package imported to the project
// #define UNIWEBVIEW

using System.Collections;
using System.Net.Sockets;
using de.bearo.restserver.Samples.StaticContentExample;
using de.bearo.restserver.Samples.WebSocketExample;
using RestServer.AutoEndpoints;
using RestServer.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace de.bearo.restserver.Samples.UniWebView {
    public class ExampleUWVController : MonoBehaviour {
        [Header("Server")]
        public RestServer.RestServer restServer;

        [Header("Example Behaviours")]
        public ReverseProxyExample.ReverseProxyExample reverseProxyExample;
        public StaticContentAutoEndpoint staticContentExample;
        public StaticContentExampleHelper staticContentExampleHelper;
        public WSMover wsMover;
        
        [Header("UI")]
        [Header("UI - Reverse Proxy Example")]
        public Button buttonReverseProxyExample;

        public Text textReverseProxyExample;
        
        [Header("UI - Static Content Example")]
        public Button buttonStaticContentExample;

        public Text textStaticContentExample;

        public Text textExtractingContent;
        
        [Header("UI - No UniWebView")]
        public Text textNoUniWebView;

        public int LastFreePort { get; private set; }

        void Start() {
#if UNIWEBVIEW
            textReverseProxyExample.text = "Show reverse proxy example";
            textStaticContentExample.text = "Show static content example";
            textNoUniWebView.enabled = false;
#else
            textReverseProxyExample.text = "Switch to reverse proxy example";
            textStaticContentExample.text = "Switch to static content example";
            textNoUniWebView.enabled = true;
#endif
            
            // Some OS are much more strict with sockets and TCP connections. Additionally the UniWebView doesn't close connections when the web view is closed
            // therefore we need to set some additional socket options to make it work.
            restServer.AdditionalSocketConfigurationServer = socket => {
                // Platform defaults can be different, therefore we set them here explicitly 

                // Allow immediate reuse of the socket after closing it
                 socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Shouldn't be necessary, as this is mostly the default. But we set it here explicitly to be sure.
                // Close socket immediately after closing it, do not wait for clients to finish.
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
            };

            // For this example we allow anyone that can reach the server to connect to the websocket also
            restServer.SpecialHandlers.AllowWsUpgradeRequest = DefaultRequestHandlerImpl.AllowWsUpgradeAlways;

            // Since we are doing manual configurations (see above), we have to restart/start the server manually (autostart is disabled on the server)
            restServer.StartServer();
            LastFreePort = restServer.EffectivePort; // save the port for later use, so we can keep it on restarts
            restServer.port = LastFreePort; // if the rest server is restarted, we want to keep the same port
        }


        private bool waitForContentGeneration = false;

        void Update() {
            InitializeExamples();

            // Wait until the static content has been generated before showing the web view
            if (waitForContentGeneration && staticContentExample.IsBuildDone) {
                ShowWebBrowser();

                EnableButtons();
                textExtractingContent.enabled = false;
                waitForContentGeneration = false;
            }
        }


        #region Port and Start/Stop Handling

        public string Url => "http://localhost:" + restServer.EffectivePort;

        private void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                return;
            }

            // We have focus again (on mobile we are in the foreground again)
            // Now multiple things can happen:
            //   - The port is still unused, we can re-use it.
            //   - The port is used by another app, we have to find a new one
            //   - The port is still unused, but the socket has been silently closed by the OS (we need to re-open it):
            //          Also note that on iOS the port can be silently closed by the OS when the app is in the background for too long, but not if it's in the
            //          background shortly. Neither Unity or Mono/IL2CPP do know this and every status on the underlying socket indicates it's still open.
            // From here on we have these options:
            //   - Hope all will go well (do nothing)
            //   - We can try to send a request to the server and see if it's still alive. This is the most reliable way but also the most expensive one.
            //   - We can just try to start the server again and see if it works. This is the fastest way, but can result in a port change if there are still
            //     active communications with clients (= port can't be re-used). Almost all web browsers do not close the connection immediately,
            //     or not at all. This can result in a port change and the UniWebView will need to reload the page.

            restServer.StopServer();
            restServer.port = LastFreePort; // ensure we try to re-use the port
            restServer.StartServer();
            if (restServer.IsStarted) {
                Debug.Log("Server restarted.");
                return; // we are done, the port is still free
            }

            restServer.StopServer();
            restServer.port = 0; // ensure we try to find a new port
            restServer.StartServer();

            if (restServer.IsStarted) {
                Debug.Log("Server restarted, with new port");

                // Save port for future re-use (this is needed as the debug view allows start/stop of the server everytime while the app is focused) 
                LastFreePort = restServer.EffectivePort;
                restServer.port = LastFreePort;

                // Not implemented in this example: Reload webview if open
            }
            else {
                // System error:
                //   - restart the application
                //   - show an error message
                //   - try to restart the server after some time and show an error message if it still doesn't work
                Debug.Log("Couldn't restart server.");
            }
        }

        #endregion

        #region Button Actions

        private int _firstUpdate = 0;

        private void InitializeExamples() {
            // Only for this example: When behaviours are enabled for the first time, the Start() method is called. This is something we don't expect in the code
            // therefore we enable all behaviours once and disable them again immediately.
            if (_firstUpdate == 0) {
                _firstUpdate = 1;

                reverseProxyExample.enabled = true;
                staticContentExample.enabled = true;
                staticContentExampleHelper.enabled = true;
                wsMover.enabled = true;
            }
            else if (_firstUpdate == 1) {
                DisableAll();
                EnableButtons();
                _firstUpdate = 2; // disable
                
                Debug.Log("--- Initialized ---");
                Debug.Log("Initialized: You can probably ignore all warnings before this message.");
            }
        }

        public void DisableAll() {
            buttonReverseProxyExample.enabled = false;
            buttonStaticContentExample.enabled = false;

            reverseProxyExample.enabled = false;
            staticContentExample.enabled = false;
            staticContentExampleHelper.enabled = false;
            wsMover.enabled = false;

            restServer.EndpointCollection.Clear();

            textExtractingContent.enabled = false;
        }

        public void EnableButtons() {
            buttonReverseProxyExample.enabled = true;
            buttonStaticContentExample.enabled = true;
        }

        public void ShowReverseProxyExample() {
            DisableAll();
            reverseProxyExample.enabled = true;
            reverseProxyExample.Start();

            CreateWebBrowser();
            ShowWebBrowser();

            EnableButtons();
        }

        public void ShowStaticPageExample() {
            DisableAll();
            staticContentExample.enabled = true;
            staticContentExampleHelper.enabled = true;
            wsMover.enabled = true;

            staticContentExampleHelper.Start();
            wsMover.Start();

            CreateWebBrowser();

            textExtractingContent.enabled = true;
            waitForContentGeneration = true;
        }

        #endregion

        #region UNIWEBVIEW

#if UNIWEBVIEW
        private global::UniWebView _webView;
#endif

        void CreateWebBrowser() {
#if UNIWEBVIEW
            var webViewGameObject = new GameObject("UniWebView");
            _webView = webViewGameObject.AddComponent<global::UniWebView>();

            _webView.Frame = new Rect(0, 0, Screen.width * 0.5f, Screen.height);
#endif
        }

        public void ShowWebBrowser() {
#if UNIWEBVIEW
            StartCoroutine(ShowWebBrowserCoroutine());
#endif
        }

        public IEnumerator ShowWebBrowserCoroutine() {
#if UNIWEBVIEW
            global::UniWebView.SetWebContentsDebuggingEnabled(true);

            _webView.CleanCache();

            // The web view is not immediately ready to be used and sometimes still contains the contents of the previous example.
            yield return new WaitForSeconds(0.1f);

            _webView.Load(Url);
            _webView.EmbeddedToolbar.Show();
            _webView.Show();

            // The web view is not immediately ready to be used and sometimes still contains the contents of the previous example.
            yield return new WaitForSeconds(0.1f);

            _webView.Reload();
#endif
            yield return null;
        }

        #endregion
    }
}