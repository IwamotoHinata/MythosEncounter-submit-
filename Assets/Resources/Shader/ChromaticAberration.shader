Shader "Hidden/Shader/ChromaticAberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0.1
    }
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    float _Intensity;

    struct Attributes
    {
        float3 positionOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionHCS : SV_Position;
        float2 uv : TEXCOORD0;
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        output.positionHCS = TransformWorldToHClip(TransformObjectToWorld(input.positionOS));
        output.uv = input.uv;
        return output;
    }

    float4 Frag(Varyings input) : SV_Target
    {
        float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
        float2 uvBase = input.uv - 0.5;

        // R
        float2 uvR = uvBase * (1.0 - _Intensity * 2.0) + 0.5;
        col.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR).r;
        
        // G
        float2 uvG = uvBase * (1.0 - _Intensity) + 0.5;
        col.g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvG).g;
        
        // B
        float2 uvB = uvBase * (1.0 + _Intensity * 2.0) + 0.5;
        col.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvB).b;

        return col;
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "Forward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
