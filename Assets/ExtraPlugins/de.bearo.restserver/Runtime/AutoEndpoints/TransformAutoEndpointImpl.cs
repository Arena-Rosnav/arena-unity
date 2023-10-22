using UnityEngine;

namespace RestServer.AutoEndpoints {
    /// <summary>
    /// Internal methods.
    /// </summary>
    public static class TransformAutoEndpointImpl {
        public static void RequestHandlerPost(GameObject target, string endpointPath, RestRequest request) {
            var shouldPos = request.JsonBody<TransformAutoEndpointPositionDTO>();

            TransformAutoEndpointPositionDTO InRenderThread() {
                var t = target.transform;
                shouldPos.UpdateTransform(t);

                return TransformAutoEndpointPositionDTO.FromTransform(target.transform);
            }

            var isPos = ThreadingHelper.Instance.ExecuteSync(InRenderThread, $"TransformAutoEndpoint-{endpointPath}");
            request.CreateResponse()
                .BodyJson(isPos)
                .SendAsync();
        }

        public static void RequestHandlerPatch(GameObject target, string endpointPath, RestRequest request) {
            var shouldPos = ThreadingHelper.Instance.ExecuteSync(() => TransformAutoEndpointPositionDTO.FromTransform(target.transform));
            JsonUtility.FromJsonOverwrite(request.Body, shouldPos);

            TransformAutoEndpointPositionDTO InRenderThread() {
                var t = target.transform;
                shouldPos.UpdateTransform(t);

                return TransformAutoEndpointPositionDTO.FromTransform(target.transform);
            }

            var isPos = ThreadingHelper.Instance.ExecuteSync(InRenderThread, $"TransformAutoEndpoint-{endpointPath}");
            request.CreateResponse()
                .BodyJson(isPos)
                .SendAsync();
        }

        public static void RequestHandlerGet(GameObject target, string endpointPath, RestRequest request) {
            var isPos = ThreadingHelper.Instance.ExecuteSync(() => TransformAutoEndpointPositionDTO.FromTransform(target.transform), $"TransformAutoEndpoint-{endpointPath}");
            request.CreateResponse()
                .BodyJson(isPos)
                .SendAsync();
        }

        public static void Register(RestServer restServer, GameObject target, string endpointPath) {
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST,
                                                           endpointPath,
                                                           request => RequestHandlerPost(target, endpointPath, request)
                                                          );
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET,
                                                           endpointPath,
                                                           request => RequestHandlerGet(target, endpointPath, request)
                                                          );
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.PATCH,
                                                           endpointPath,
                                                           request => RequestHandlerPatch(target, endpointPath, request)
                                                          );
        }

        public static void Deregister(RestServer restServer, string endpointPath) {
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.POST, endpointPath);
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.GET, endpointPath);
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.PATCH, endpointPath);
        }
    }


    public class TransformAutoEndpointPositionDTO {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public TransformAutoEndpointPositionDTO(Vector3 position, Vector3 rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public static TransformAutoEndpointPositionDTO FromTransform(Transform t) {
            return new TransformAutoEndpointPositionDTO(t.position, t.rotation.eulerAngles, t.localScale);
        }

        public void UpdateTransform(Transform t) {
            t.position = position;
            t.rotation = Quaternion.Euler(rotation);
            t.localScale = scale;
        }
    }
}