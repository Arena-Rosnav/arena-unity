using System;
using System.Collections.Generic;
using RestServer;
using RestServer.MultipartFormData;
using UnityEngine;

namespace de.bearo.restserver.Samples.SimpleFormData {
    
    /// <summary>
    /// This sample shows how to handle multipart/form-data requests. It allows the user to upload an image and displays it on a plane.
    /// </summary>
    public class SimpleFormData : MonoBehaviour {
        public RestServer.RestServer RestServer;

        public Texture2D UploadedTexture;
        public Material Material;
        
        void Start() {
            // Register the upload endpoint
            RestServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/upload", request => {
                List<MultiFormDataElement> uploadData;
                try {
                    uploadData = new SimpleMFDParser().Parse(request);
                }
                catch (SystemException e) {
                    Debug.LogError(e);
                    request.CreateResponse()
                        .StatusError()
                        .Body("No form data found.")
                        .SendAsync();
                    return;
                }

                // Stop if there is no data in the body
                if (uploadData.Count == 0) {
                    request.CreateResponse()
                        .StatusError()
                        .Body("No file specified.")
                        .SendAsync();
                    return;
                }

                // Handle the decoding of the image on the unity main thread, because it uses unity api
                var textureLoaded = ThreadingHelper.Instance.ExecuteSync(() => {
                    var first = uploadData[0];
                    var firstImageBytes = first.Data;

                    var temp = new Texture2D(1024, 1024);

                    // the size of the texture will be replaced by image size
                    var ret = temp.LoadImage(firstImageBytes);
                    if (ret) {
                        UploadedTexture = temp;
                        Material.mainTexture = temp;
                    }

                    return ret;
                });

                if (textureLoaded) {
                    request.CreateResponse()
                        .Body("OK")
                        .SendAsync();
                }
                else {
                    request.CreateResponse()
                        .InternalServerError("Couldn't parse image data")
                        .SendAsync();
                }
            });
        }
    }
}