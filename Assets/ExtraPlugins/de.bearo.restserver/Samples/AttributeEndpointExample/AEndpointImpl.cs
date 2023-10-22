using RestServer;
using RestServer.AutoEndpoints;
using UnityEngine;

namespace de.bearo.restserver.Samples.AttributeEndpointExample {
    /// <summary>
    /// Simple example to show how to register endpoints via attributes and the AttributeAutoEndpoint Behaviour.
    ///
    /// Creates a simple html page with a toggle button to toggle the active state of a game object.
    /// </summary>
    public class AEndpointImpl : MonoBehaviour {
        public GameObject go;

        /// <summary>
        /// Send a simple html page with a toggle button to toggle the active state of a game object.
        /// </summary>
        /// <param name="r">Reference to the current rest request</param>
        [Endpoint("/get")]
        public void Endpoint_Get(RestRequest r) {
            Debug.Log("GET Called");

            // send the html form back to the client
            r.CreateResponse()
                .Body(CreateHtmlResponse())
                .Header(HttpHeader.CONTENT_TYPE, MimeType.TEXT_HTML)
                .SendAsync();
        }

        /// <summary>
        /// Toggle the active state of a game object and send a simple html page with a toggle button to toggle the active state of a game object.
        /// </summary>
        /// <param name="r"></param>
        [Endpoint(HttpMethod.POST, "/post")]
        public void Endpoint_Post(RestRequest r) {
            Debug.Log("POST Called");
            go.SetActive(!go.activeSelf);

            // send the html form back to the client
            r.CreateResponse()
                .Body(CreateHtmlResponse())
                .Header(HttpHeader.CONTENT_TYPE, MimeType.TEXT_HTML)
                .SendAsync();
        }

        /// <summary>
        /// Regex example, send the html page for any path starting with (api/v1/)/regex_example/
        /// </summary>
        /// <param name="r"></param>
        [Endpoint(HttpMethod.GET, "/regex_example/.*", isRegex: true)]
        public void Endpoint_Regex(RestRequest r) {
            Debug.Log("Regex Called");

            // send the html form back to the client
            r.CreateResponse()
                .Body(CreateHtmlResponse())
                .Header(HttpHeader.CONTENT_TYPE, MimeType.TEXT_HTML)
                .SendAsync();
        }

        private string CreateHtmlResponse() {
            var lazyHtml = $"<html><body><p>Game Object is active? {go.activeSelf}</p>" +
                           $"<p>Toggle active state: <form action=\"/api/v1/post\" method=\"POST\"><input type=\"submit\">" +
                           $"</form></p></body></html>";
            return lazyHtml;
        }
    }
}