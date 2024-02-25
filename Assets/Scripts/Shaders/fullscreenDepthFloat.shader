Shader "FullScreen/DepthFloat"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 vulkan metal

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float4 frag(Varyings varyings) : SV_Target
    {
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        float linearDepthKM = LinearEyeDepth(depth, _ZBufferParams);
        return float4(linearDepthKM, linearDepthKM, linearDepthKM, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline"}
        Pass
        {
            Tags{"LightMode" = "SRPDefaultUnlit"}
            Name "DepthOnly"

            ZWrite Off
            ZTest Always
            Cull Back

            HLSLPROGRAM
                #pragma fragment frag
            ENDHLSL
        }
    }
    Fallback Off
}
