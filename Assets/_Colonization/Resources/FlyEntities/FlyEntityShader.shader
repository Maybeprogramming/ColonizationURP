Shader "Custom/FlyEntity"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _FlapSpeed ("Flap Speed", Range(1, 20)) = 8
        _FlapAngle ("Flap Angle", Range(0, 1)) = 0.5
        _Alpha ("Global Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;
            float _FlapSpeed;
            float _FlapAngle;
            float _Alpha;
            float _Scale;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexToFragment
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexToFragment vert(Attributes input)
            {
                float3 localPosition = input.vertex.xyz * _Scale;

                float flapAngle = sin(_Time.y * _FlapSpeed) * _FlapAngle;

                float wingFlap = 0;
                float uvX = input.uv.x;

                if (uvX < 1.0 / 3.0)
                    wingFlap = flapAngle;
                else if (uvX > 2.0 / 3.0)
                    wingFlap = -flapAngle;

                float sinFlap = sin(wingFlap);
                float cosFlap = cos(wingFlap);

                float3 wingPosition;
                wingPosition.x = localPosition.x * cosFlap - localPosition.y * sinFlap;
                wingPosition.y = localPosition.x * sinFlap + localPosition.y * cosFlap;
                wingPosition.z = localPosition.z;

                VertexToFragment output;
                output.position = TransformObjectToHClip(wingPosition);
                output.uv = input.uv;
                return output;
            }

            half4 frag(VertexToFragment input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                color.a *= _Alpha;
                clip(color.a - 0.01);
                return color;
            }
            ENDHLSL
        }
    }
}
