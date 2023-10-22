using RestServer;
using UnityEngine;

namespace de.bearo.restserver.Samples.BinaryResponse {
    
    /// <summary>
    /// Simple example to let the user download binary data (in this case a PNG image).
    /// </summary>
    public class BRBinaryResponse : MonoBehaviour {
        public RestServer.RestServer server;

        // Texture must have the "Advanced Settings -> Read/Write" checkbox set, no compression allowed. These are Unity/EncodeToPNG restrictions!
        public Texture2D texture;

        private byte[] binaryTexture;
        
        void Start() {
            // Convert texture to PNG
            binaryTexture = texture.EncodeToPNG();
            
            // Register endpoint
            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/image", GetImage);
        }

        void GetImage(RestRequest request) {
            // Respond with the image data directly. We are a bit "scary" here, as we do not threadlock while accessing 
            // unity data on the mono behaviour. In this case it's ok as the variable binaryTexture is only set once in
            // the Start() method and never changed again. If you want to be on the safe side, you can use the TreadingHelper here.
            request.CreateResponse()
                .Header(HttpHeader.CONTENT_TYPE, MimeType.IMAGE_PNG)
                .Body(binaryTexture)
                .SendAsync();
        }
    }
}