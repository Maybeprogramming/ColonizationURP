Shader "Custom/FlyEntity"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _FlapSpeed ("Flap Speed", Range(1, 20)) = 8
        _FlapAngle ("Flap Angle", Range(0, 1.5)) = 0.8
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
                float3 localPos = v.vertex.xyz;

                float wingPhase = sin(_Time.y * _FlapSpeed) * _FlapAngle;
                float side = sign(localPos.x);
                float flapAmount = abs(localPos.x);
                float angle = wingPhase * side * flapAmount;

                float cosA = cos(angle);
                float sinA = sin(angle);

                float3 wingPos;
                wingPos.x = localPos.x * cosA;
                wingPos.y = localPos.y;
                wingPos.z = localPos.x * sinA;

                float4 centerWorld = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
                float3 forward = normalize(_WorldSpaceCameraPos - centerWorld.xyz);
                float3 right = normalize(cross(float3(0, 1, 0), forward));
                float3 up = cross(forward, right);

                float3 worldPos = centerWorld.xyz + right * wingPos.x + up * wingPos.y + forward * wingPos.z;

                v2f o;
                o.pos = TransformWorldToHClip(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
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
