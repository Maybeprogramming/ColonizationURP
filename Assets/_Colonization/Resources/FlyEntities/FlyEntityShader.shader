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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                float3 localPos = v.vertex.xyz * _Scale;

                float flapAngle = sin(_Time.y * _FlapSpeed) * _FlapAngle;

                float wingFlap = 0;
                float u = v.uv.x;

                if (u < 1.0 / 3.0)
                    wingFlap = flapAngle;
                else if (u > 2.0 / 3.0)
                    wingFlap = -flapAngle;

                float s = sin(wingFlap);
                float c = cos(wingFlap);

                float3 wingPos;
                wingPos.x = localPos.x * c - localPos.y * s;
                wingPos.y = localPos.x * s + localPos.y * c;
                wingPos.z = localPos.z;

                v2f o;
                o.pos = TransformObjectToHClip(wingPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                color.a *= _Alpha;
                clip(color.a - 0.01);
                return color;
            }
            ENDHLSL
        }
    }
}
