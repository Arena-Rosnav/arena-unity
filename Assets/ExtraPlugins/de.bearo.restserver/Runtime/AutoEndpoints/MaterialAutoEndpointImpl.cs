#if UNITY_2021_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace RestServer.AutoEndpoints {
    public static class MaterialAutoEndpointImpl {
        public static void RequestHandlerGet(Material material, string endpointPath, MAEPropertiesInfo properties, RestRequest request) {
            var dto = ThreadingHelper.Instance.ExecuteSync(() => ToDTO(material, properties), $"MaterialAutoEndpoint-{endpointPath}");
            request.CreateResponse().BodyJson(dto).SendAsync();
        }

        public static void RequestHandlerPost(Material material, string endpointPath, MAEPropertiesInfo properties, RestRequest request) {
            var dto = ThreadingHelper.Instance.ExecuteSync(() => {
                                                               var newDTO = request.JsonBody<MAEPropertiesDTO>();
                                                               FromDTO(material, newDTO, properties);
                                                               return ToDTO(material, properties);
                                                           },
                                                           $"MaterialAutoEndpoint-{endpointPath}"
                                                          );
            request.CreateResponse().BodyJson(dto).SendAsync();
        }

        public static void RequestHandlerPatch(Material material, string endpointPath, MAEPropertiesInfo properties, RestRequest request) {
            var dto = ThreadingHelper.Instance.ExecuteSync(() => {
                                                               var patchDTO = ToDTO(material, properties);
                                                               JsonUtility.FromJsonOverwrite(request.Body, patchDTO);
                                                               FromDTO(material, patchDTO, properties);
                                                               return ToDTO(material, properties);
                                                           },
                                                           $"MaterialAutoEndpoint-{endpointPath}"
                                                          );
            request.CreateResponse().BodyJson(dto).SendAsync();
        }

        public static void Register(RestServer restServer, Material material, MAEPropertiesInfo properties, string endpointPath) {
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, endpointPath, request => RequestHandlerGet(material, endpointPath, properties, request));
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST, endpointPath, request => RequestHandlerPost(material, endpointPath, properties, request));
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.PATCH, endpointPath, request => RequestHandlerPatch(material, endpointPath, properties, request));
        }

        public static void Deregister(RestServer restServer, string endpointPath) {
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.GET, endpointPath);
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.POST, endpointPath);
            restServer.EndpointCollection.RemoveEndpoint(HttpMethod.PATCH, endpointPath);
        }

        public static MAEPropertiesInfo GeneratePropertiesInfo(Material material, string[] exposedPropertyNames) {
            var used = new HashSet<string>();
            var ret = new MAEPropertiesInfo();

            foreach (var p in exposedPropertyNames) {
                if (material.HasColor(p)) {
                    ret.AddColor(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    used.Add(p);
                } else if (material.HasFloat(p)) {
                    ret.AddFloat(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    used.Add(p);
                } else if (material.HasInt(p)) {
                    ret.AddInt(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    used.Add(p);
                } else if (material.HasMatrix(p)) {
                    ret.AddMatrix(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    used.Add(p);
                } else if (material.HasVector(p) && !material.HasColor(p)) {
                    ret.AddVector(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    used.Add(p);
                } else if (material.HasTexture(p)) {
                    ret.AddTextureOffset(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    ret.AddTextureScale(new MAEPropertyInfo(p, Shader.PropertyToID(p)));
                    used.Add(p);
                }
            }

            foreach (var p in exposedPropertyNames) {
                if (!used.Contains(p)) {
                    Debug.LogWarning($"Unused property '{p}' for material {material.name}. Please check if the name exists and if you prefixed it with a underscore.");
                }
            }

            return ret;
        }

        #region from - toDTO

        public static MAEPropertiesDTO ToDTO(Material m, MAEPropertiesInfo properties) {
            var ret = new MAEPropertiesDTO();

            foreach (var element in properties.colors) {
                var value = m.GetColor(element.id);
                ret.colors.Add(new MAEPropertiesColorDTO(element.name, value));
            }

            foreach (var element in properties.floats) {
                var value = m.GetFloat(element.id);
                ret.floats.Add(new MAEPropertiesFloatDTO(element.name, value));
            }

            foreach (var element in properties.ints) {
                var value = m.GetInt(element.id);
                ret.ints.Add(new MAEPropertiesInt(element.name, value));
            }

            foreach (var element in properties.matrixs) {
                var value = m.GetMatrix(element.id);
                ret.matrixs.Add(new MAEPropertiesMatrix(element.name, value));
            }

            foreach (var element in properties.vectors) {
                var value = m.GetVector(element.id);
                ret.vectors.Add(new MAEPropertiesVector(element.name, value));
            }

            foreach (var element in properties.textureOffsets) {
                var value = m.GetTextureOffset(element.id);
                ret.textureOffsets.Add(new MAEPropertiesTextureOffset(element.name, value));
            }

            foreach (var element in properties.textureScales) {
                var value = m.GetTextureScale(element.id);
                ret.textureScales.Add(new MAEPropertiesTextureScale(element.name, value));
            }

            return ret;
        }

        public static void FromDTO(Material m, MAEPropertiesDTO dto, MAEPropertiesInfo properties) {
            foreach (var element in dto.colors) {
                if (properties.idxColors.TryGetValue(element.name, out var property)) {
                    m.SetColor(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }

            foreach (var element in dto.floats) {
                if (properties.idxFloats.TryGetValue(element.name, out var property)) {
                    m.SetFloat(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }

            foreach (var element in dto.ints) {
                if (properties.idxInts.TryGetValue(element.name, out var property)) {
                    m.SetInt(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }

            foreach (var element in dto.matrixs) {
                if (properties.idxMatrixs.TryGetValue(element.name, out var property)) {
                    m.SetMatrix(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }

            foreach (var element in dto.vectors) {
                if (properties.idxVectors.TryGetValue(element.name, out var property)) {
                    m.SetVector(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }

            foreach (var element in dto.textureOffsets) {
                if (properties.idxTextureOffsets.TryGetValue(element.name, out var property)) {
                    m.SetTextureOffset(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }

            foreach (var element in dto.textureScales) {
                if (properties.idxTextureScales.TryGetValue(element.name, out var property)) {
                    m.SetTextureScale(property.id, element.value);
                } else {
                    Debug.LogWarning($"Unknown property {element.name}");
                }
            }
        }

        #endregion
    }


    #region Properties Info

    public class MAEPropertiesInfo {
        public List<MAEPropertyInfo> colors = new List<MAEPropertyInfo>();
        public List<MAEPropertyInfo> floats = new List<MAEPropertyInfo>();
        public List<MAEPropertyInfo> ints = new List<MAEPropertyInfo>();
        public List<MAEPropertyInfo> matrixs = new List<MAEPropertyInfo>();
        public List<MAEPropertyInfo> vectors = new List<MAEPropertyInfo>();
        public List<MAEPropertyInfo> textureOffsets = new List<MAEPropertyInfo>();
        public List<MAEPropertyInfo> textureScales = new List<MAEPropertyInfo>();

        public Dictionary<string, MAEPropertyInfo> idxColors = new Dictionary<string, MAEPropertyInfo>();
        public Dictionary<string, MAEPropertyInfo> idxFloats = new Dictionary<string, MAEPropertyInfo>();
        public Dictionary<string, MAEPropertyInfo> idxInts = new Dictionary<string, MAEPropertyInfo>();
        public Dictionary<string, MAEPropertyInfo> idxMatrixs = new Dictionary<string, MAEPropertyInfo>();
        public Dictionary<string, MAEPropertyInfo> idxVectors = new Dictionary<string, MAEPropertyInfo>();
        public Dictionary<string, MAEPropertyInfo> idxTextureOffsets = new Dictionary<string, MAEPropertyInfo>();
        public Dictionary<string, MAEPropertyInfo> idxTextureScales = new Dictionary<string, MAEPropertyInfo>();

        public void AddColor(MAEPropertyInfo p) {
            colors.Add(p);
            idxColors.Add(p.name, p);
        }

        public void AddFloat(MAEPropertyInfo p) {
            floats.Add(p);
            idxFloats.Add(p.name, p);
        }

        public void AddInt(MAEPropertyInfo p) {
            ints.Add(p);
            idxInts.Add(p.name, p);
        }

        public void AddMatrix(MAEPropertyInfo p) {
            matrixs.Add(p);
            idxMatrixs.Add(p.name, p);
        }

        public void AddVector(MAEPropertyInfo p) {
            vectors.Add(p);
            idxVectors.Add(p.name, p);
        }

        public void AddTextureOffset(MAEPropertyInfo p) {
            textureOffsets.Add(p);
            idxTextureOffsets.Add(p.name, p);
        }

        public void AddTextureScale(MAEPropertyInfo p) {
            textureScales.Add(p);
            idxTextureScales.Add(p.name, p);
        }
    }

    public struct MAEPropertyInfo {
        public readonly string name;
        public readonly int id;

        public MAEPropertyInfo(string name, int id) {
            this.name = name;
            this.id = id;
        }
    }

    #endregion

    #region DTO

    [Serializable]
    public class MAEPropertiesDTO {
        public List<MAEPropertiesColorDTO> colors = new List<MAEPropertiesColorDTO>();
        public List<MAEPropertiesFloatDTO> floats = new List<MAEPropertiesFloatDTO>();
        public List<MAEPropertiesInt> ints = new List<MAEPropertiesInt>();
        public List<MAEPropertiesMatrix> matrixs = new List<MAEPropertiesMatrix>();
        public List<MAEPropertiesVector> vectors = new List<MAEPropertiesVector>();
        public List<MAEPropertiesTextureOffset> textureOffsets = new List<MAEPropertiesTextureOffset>();
        public List<MAEPropertiesTextureScale> textureScales = new List<MAEPropertiesTextureScale>();
    }

    [Serializable]
    public class MAEPropertiesBase<T> {
        public string name;
        public T value;

        public MAEPropertiesBase() { }

        public MAEPropertiesBase(string name, T value) {
            this.name = name;
            this.value = value;
        }
    }

    [Serializable]
    public class MAEPropertiesColorDTO : MAEPropertiesBase<Color> {
        public MAEPropertiesColorDTO() { }
        public MAEPropertiesColorDTO(string name, Color value) : base(name, value) { }
    }

    [Serializable]
    public class MAEPropertiesFloatDTO : MAEPropertiesBase<float> {
        public MAEPropertiesFloatDTO() { }
        public MAEPropertiesFloatDTO(string name, float value) : base(name, value) { }
    }

    [Serializable]
    public class MAEPropertiesInt : MAEPropertiesBase<int> {
        public MAEPropertiesInt() { }
        public MAEPropertiesInt(string name, int value) : base(name, value) { }
    }

    [Serializable]
    public class MAEPropertiesMatrix : MAEPropertiesBase<Matrix4x4> {
        public MAEPropertiesMatrix() { }
        public MAEPropertiesMatrix(string name, Matrix4x4 value) : base(name, value) { }
    }

    [Serializable]
    public class MAEPropertiesVector : MAEPropertiesBase<Vector4> {
        public MAEPropertiesVector() { }
        public MAEPropertiesVector(string name, Vector4 value) : base(name, value) { }
    }

    [Serializable]
    public class MAEPropertiesTextureOffset : MAEPropertiesBase<Vector2> {
        public MAEPropertiesTextureOffset() { }
        public MAEPropertiesTextureOffset(string name, Vector2 value) : base(name, value) { }
    }

    [Serializable]
    public class MAEPropertiesTextureScale : MAEPropertiesBase<Vector2> {
        public MAEPropertiesTextureScale() { }
        public MAEPropertiesTextureScale(string name, Vector2 value) : base(name, value) { }
    }

    #endregion
}
#endif
