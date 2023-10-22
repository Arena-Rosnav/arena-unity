using RestServer;
using UnityEngine;

namespace de.bearo.restserver.Samples.MethodAutoEndpointExample {
    
    /// <summary>
    /// Simple endpoint implementation, hides or shows a game object and displays a simple form to do this for the user.
    /// </summary>
    public class MAExampleImpl : MonoBehaviour {
        public GameObject go;

        public void EndpointPath_Get(RestRequest r) {
            Debug.Log("GET Called");
            
            // Respond with the html form to toggle the game object
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

        public void EndpointPath_Post(RestRequest r) {
            Debug.Log("POST Called");
            
            // Toggle game object
            go.SetActive(!go.activeSelf);
            
            // Respond with the html form to toggle the game object
            r.CreateResponse()
                .Body(CreateHtmlResponse())
                .Header(HttpHeader.CONTENT_TYPE, MimeType.TEXT_HTML)
                .SendAsync();
        }
    }
}