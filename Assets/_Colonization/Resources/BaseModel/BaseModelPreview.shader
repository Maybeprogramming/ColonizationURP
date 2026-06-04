Shader "Custom/BaseModelPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.5, 0.8, 1, 1)
        _SecondaryColor ("Secondary Color", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 0.5
        _IridescenceSpeed ("Iridescence Speed", Range(0, 5)) = 1.0
        _IridescenceIntensity ("Iridescence Intensity", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.6
            #define TWO_PI 6.28318530718

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _Color;
            float4 _SecondaryColor;
            float _Alpha;
            float _IridescenceSpeed;
            float _IridescenceIntensity;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct VertexToFragment
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            VertexToFragment vert(Attributes input)
            {
                VertexToFragment output;
                output.position = TransformObjectToHClip(input.vertex.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normal);
                output.viewDirWS = normalize(_WorldSpaceCameraPos - positionWS);
                return output;
            }

            half4 frag(VertexToFragment input) : SV_Target
            {
                half4 textureColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float normalDotView = saturate(dot(normalWS, viewDirWS));

                float iridescencePhase = normalDotView + _Time.y * _IridescenceSpeed;

                float gradientFactor = sin(iridescencePhase * TWO_PI) * 0.5 + 0.5;
                float3 gradientColor = lerp(_Color.rgb, _SecondaryColor.rgb, gradientFactor);

                float3 finalColor = lerp(textureColor.rgb * _Color.rgb, gradientColor, _IridescenceIntensity);
                float finalAlpha = textureColor.a * _Color.a * _Alpha;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma target 4.6

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct VertexToFragment
            {
                float4 position : SV_POSITION;
            };

            VertexToFragment vertShadow(Attributes input)
            {
                VertexToFragment output;
                float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normal);
                float4 shadowPosition = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                output.position = shadowPosition;
                return output;
            }

            half4 fragShadow(VertexToFragment input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
