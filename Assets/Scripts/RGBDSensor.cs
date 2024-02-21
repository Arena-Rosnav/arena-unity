using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using System.Collections.Generic;

public enum DepthOutputFormat
{
    UINT16,
    FLOAT32,
    POINTCLOUD
}

public class RGBDSensor : MonoBehaviour
{
    private Camera depthCamera;
    private Camera colorCamera;
    private double lastTime = 0;

    private RenderTexture depthTexture;
    private RenderTexture colorTexture;
    public int Width = 1280;
    public int Height = 720;

    public bool UseDepthCamera = true;
    public bool UseColorCamera = true;
    public DepthOutputFormat DepthFormat = DepthOutputFormat.FLOAT32;

    private Texture2D depthTex;
    private Rect textureRect;

    private uint seq = 0;

    private CustomPassVolume customPassVolume;


    //this is necessary so that the shader gets copied into the build
    [SerializeField]
    public Shader DepthShader;

    public string frameId = "rgbd_camera_link";
    public string topicName = "rgbd";
    public string topicNamespace = "";

    public double publishRateHz = 30;
    private RosTopicState imagePublisher;
    private RosTopicState depthPublisher;

    private byte[] bytePixelBuffer;
    private float[] floatPixelBuffer;
    private bool shouldCaptureDepth = true;
    private bool shouldCaptureColor = false;

    public float nearClip = 0.1f;
    public float farClip = 1000f;
    public float fov = 70f;
    string PublishTopic => topicNamespace + "/" + topicName;

    public void ConfigureRGBDSensor(Dictionary<string, object> configDict, string robotName, string jointName)
    {
        if (configDict.ContainsKey("topic"))
            topicName = (string)configDict["topic"];

        if (configDict.ContainsKey("width"))
            int.TryParse((string)configDict["width"], out Width);
        if (configDict.ContainsKey("height"))
            int.TryParse((string)configDict["height"], out Height);
        if (configDict.ContainsKey("target_fps"))
            double.TryParse((string)configDict["target_fps"], out publishRateHz);
        if (configDict.ContainsKey("use_rgb"))
            bool.TryParse((string)configDict["use_rgb"], out UseColorCamera);
        if (configDict.ContainsKey("use_depth"))
            bool.TryParse((string)configDict["use_depth"], out UseDepthCamera);
        if (configDict.ContainsKey("depth_output_format"))
        {
            string format = (string)configDict["depth_output_format"];
            switch (format)
            {
                case "uint16":
                    DepthFormat = DepthOutputFormat.UINT16;
                    break;
                case "float32":
                    DepthFormat = DepthOutputFormat.FLOAT32;
                    break;
                case "pointcloud":
                    DepthFormat = DepthOutputFormat.POINTCLOUD;
                    break;
                default:
                    Debug.LogError($"Unknown depth output format: {format}");
                    break;
            }
        }

        if (configDict.ContainsKey("near_clip"))
            float.TryParse((string)configDict["near_clip"], out nearClip);
        if (configDict.ContainsKey("far_clip"))
            float.TryParse((string)configDict["far_clip"], out farClip);
        if (configDict.ContainsKey("fov"))
            float.TryParse((string)configDict["fov"], out fov);

    }

    public void ConfigureDefaultRGBDSensor(string robotName, string jointName)
    {
    }

    void Start()
    {
        if (!UseDepthCamera && !UseColorCamera)
        {
            Debug.LogError("At least one camera must be enabled");
            return;
        }
        if (UseDepthCamera)
            SetUpDepthCamera();
        if (UseColorCamera)
            SetUpColorCamera();

        textureRect = new Rect(0, 0, Width, Height);
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;

    }

    private void SetUpColorCamera()
    {
        GameObject colorCameraObject = new GameObject("ColorCamera");
        colorCameraObject.transform.parent = transform;
        colorCameraObject.transform.localPosition = Vector3.zero;
        colorCameraObject.transform.localRotation = Quaternion.identity;
        colorCamera = colorCameraObject.AddComponent<Camera>();
        colorCamera.nearClipPlane = nearClip;
        colorCamera.farClipPlane = farClip;
        colorCamera.fieldOfView = fov;
        colorTexture = new RenderTexture(Width, Height, 32, RenderTextureFormat.ARGBFloat);
        colorCamera.targetTexture = colorTexture;

        imagePublisher = FindObjectOfType<ROSConnection>().RegisterPublisher<RosMessageTypes.Sensor.ImageMsg>($"{PublishTopic}/image", 2);

        HDAdditionalCameraData hdData = colorCameraObject.AddComponent<HDAdditionalCameraData>();
        hdData.enabled = true;
        hdData.flipYMode = HDAdditionalCameraData.FlipYMode.ForceFlipY;
    }

    void SetUpDepthCamera()
    {
        //initialize depth camera
        GameObject depthCameraObject = new GameObject("DepthCamera");
        depthCameraObject.transform.parent = transform;
        depthCameraObject.transform.localPosition = Vector3.zero;
        depthCameraObject.transform.localRotation = Quaternion.identity;
        depthCamera = depthCameraObject.AddComponent<Camera>();
        depthCamera.depthTextureMode = DepthTextureMode.Depth;
        depthCamera.depth = -1;
        depthCamera.nearClipPlane = nearClip;
        depthCamera.farClipPlane = farClip;
        depthCamera.fieldOfView = fov;
        if (DepthFormat == DepthOutputFormat.UINT16)
        {
            depthCamera.farClipPlane = ushort.MaxValue / 1000f;
        }
        depthTexture = new RenderTexture(Width, Height, 32, RenderTextureFormat.ARGBFloat);
        depthCamera.targetTexture = depthTexture;


        depthPublisher = FindObjectOfType<ROSConnection>().RegisterPublisher<RosMessageTypes.Sensor.ImageMsg>($"{PublishTopic}/depth", 2);

        //add custom pass
        customPassVolume = gameObject.AddComponent<CustomPassVolume>();
        customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
        customPassVolume.isGlobal = true;
        customPassVolume.targetCamera = depthCamera;

        string shaderName = DepthFormat == DepthOutputFormat.UINT16 ? "FullScreen/Depth16" : "FullScreen/DepthFloat";
        DepthShader = Shader.Find(shaderName);

        Material depthMaterial = CoreUtils.CreateEngineMaterial(DepthShader);


        FullScreenCustomPass renderPass = CustomPass.CreateFullScreenPass(depthMaterial);
        customPassVolume.customPasses.Add(renderPass);
        customPassVolume.enabled = true;


        var textureFormat = DepthFormat == DepthOutputFormat.UINT16 ? TextureFormat.RGBA32 : TextureFormat.RGBAFloat;
        depthTex = new Texture2D(Width, Height, textureFormat, false);

        floatPixelBuffer = new float[Width * Height];
        bytePixelBuffer = new byte[Width * Height * 4];

        // //reduce the number of draw calls
        #region FrameSettings
        HDAdditionalCameraData hdCameraData = depthCameraObject.AddComponent<HDAdditionalCameraData>();
        hdCameraData.enabled = true;
        hdCameraData.clearDepth = depthCamera.clearFlags != CameraClearFlags.Nothing;
        hdCameraData.customRenderingSettings = true;
        hdCameraData.flipYMode = HDAdditionalCameraData.FlipYMode.ForceFlipY;
        hdCameraData.renderingPathCustomFrameSettings.litShaderMode = LitShaderMode.Deferred;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.OpaqueObjects] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.CustomPass] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.TransparentObjects] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Decals] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.DecalLayers] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.TransparentPrepass] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.TransparentPostpass] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.MotionVectors] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ObjectMotionVectors] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.TransparentsWriteMotionVector] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Refraction] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Distortion] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Postprocess] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.AfterPostprocess] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.LowResTransparent] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ShadowMaps] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ContactShadows] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ProbeVolume] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ScreenSpaceShadows] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Shadowmask] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.SSR] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.SSGI] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.SSAO] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Transmission] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.AtmosphericScattering] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Volumetrics] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReprojectionForVolumetrics] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.LightLayers] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ExposureControl] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReflectionProbe] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.PlanarProbe] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReplaceDiffuseForIndirect] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.SkyReflection] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.DirectSpecularLighting] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.SubsurfaceScattering] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.VolumetricClouds] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.AsyncCompute] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.FPTLForForwardOpaque] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.BigTilePrepass] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.DeferredTile] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ComputeLightEvaluation] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ComputeLightVariants] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ComputeMaterialVariants] = true;
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.OpaqueObjects, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.TransparentObjects, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Decals, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.DecalLayers, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.TransparentPrepass, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.TransparentPostpass, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.MotionVectors, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ObjectMotionVectors, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.TransparentsWriteMotionVector, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Refraction, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Distortion, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Postprocess, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.AfterPostprocess, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LowResTransparent, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ContactShadows, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ProbeVolume, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ScreenSpaceShadows, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Shadowmask, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SSR, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SSGI, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SSAO, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Transmission, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.AtmosphericScattering, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReprojectionForVolumetrics, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LightLayers, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ExposureControl, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReflectionProbe, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.PlanarProbe, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReplaceDiffuseForIndirect, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SkyReflection, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.DirectSpecularLighting, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SubsurfaceScattering, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.VolumetricClouds, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.AsyncCompute, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.FPTLForForwardOpaque, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.BigTilePrepass, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.DeferredTile, true);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ComputeLightEvaluation, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ComputeLightVariants, false);
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ComputeMaterialVariants, false);
        #endregion
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == depthCamera && shouldCaptureDepth)
        {
            shouldCaptureDepth = false;
            CaptureDepthAsync();

        }
        else if (camera == colorCamera && shouldCaptureColor)
        {
            shouldCaptureColor = false;
            CaptureColorAsync();
        }
    }



    void Update()
    {
        var time = Unity.Robotics.Core.Clock.time;
        if (time - lastTime < 1 / publishRateHz)
            return;

        lastTime = time;
        shouldCaptureDepth = true;
        shouldCaptureColor = true;
    }

    private void CaptureColorAsync()
    {
        AsyncGPUReadback.Request(colorTexture, 0, TextureFormat.RGBA32, OnCompleteReadbackColor);
    }

    private void CaptureDepthAsync()
    {
        if (DepthOutputFormat.UINT16 == DepthFormat)
        {
            AsyncGPUReadback.Request(depthTexture, 0, TextureFormat.RGBA32, OnCompleteReadbackDepth);
        }
        else
        {
            AsyncGPUReadback.Request(depthTexture, 0, TextureFormat.RGBAFloat, OnCompleteReadbackDepth);
        }
    }

    private void OnCompleteReadbackColor(AsyncGPUReadbackRequest request)
    {
        if (!request.done || request.hasError)
        {
            Debug.Log("Error in readback");
            return;
        }

        byte[] pixels = request.GetData<byte>().ToArray();
        RosMessageTypes.Std.HeaderMsg header = new RosMessageTypes.Std.HeaderMsg(seq, new TimeStamp(Unity.Robotics.Core.Clock.time), frameId);
        RosMessageTypes.Sensor.ImageMsg msg = new(header, (uint)Height, (uint)Width, "rgba8", 4, (uint)Width * 4, pixels);
        imagePublisher.Publish(msg);
    }


    private void OnCompleteReadbackDepth(AsyncGPUReadbackRequest request)
    {
        if (!request.done || request.hasError)
        {
            Debug.Log("Error in readback");
            return;
        }

        if (DepthFormat == DepthOutputFormat.UINT16)
        {
            Color32[] pixels = request.GetData<Color32>().ToArray();

            Task.Run(() =>
            {
                PublishDepthFrame(pixels);
            });

        }
        else if (DepthFormat == DepthOutputFormat.FLOAT32)
        {
            Color[] pixels = request.GetData<Color>().ToArray();
            double time = Unity.Robotics.Core.Clock.time;
            Task.Run(() =>
            {
                PublishDepthFrame(pixels, time);
            });
        }
        else
        {
            Debug.Log("PointCloud not implemented");
            throw new NotImplementedException();
        }
    }

    private void PublishDepthFrame(Color32[] colors)
    {
        byte[] bytes = new byte[colors.Length * 2];
        Parallel.For(0, Height, (y) =>
        {
            for (int x = 0; x < Width; x++)
            {
                int i = y * Width + x;
                i *= 2;
                int reverseY = Height - y - 1;
                int reverseI = reverseY * Width + x;
                bytes[i] = colors[reverseI].r;
                bytes[i + 1] = colors[reverseI].g;
            }
        });
        RosMessageTypes.Std.HeaderMsg header = new RosMessageTypes.Std.HeaderMsg(seq, new TimeStamp(Unity.Robotics.Core.Clock.time), frameId);
        RosMessageTypes.Sensor.ImageMsg msg = new(header, (uint)Height, (uint)Width, "mono16", 1, (uint)Width * 2, bytes);
        depthPublisher.Publish(msg);
    }

    private void PublishDepthFrame(Color[] colors, double time)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int i = y * Width + x;
                floatPixelBuffer[i] = colors[i].r;
            }
        }

        Buffer.BlockCopy(floatPixelBuffer, 0, bytePixelBuffer, 0, bytePixelBuffer.Length);

        RosMessageTypes.Std.HeaderMsg header = new RosMessageTypes.Std.HeaderMsg(seq, new TimeStamp(time), frameId);
        RosMessageTypes.Sensor.ImageMsg msg = new(header, (uint)Height, (uint)Width, "32FC1", (byte)(BitConverter.IsLittleEndian ? 0 : 1), (uint)Width * 4, bytePixelBuffer);
        depthPublisher.Publish(msg);
    }


}
