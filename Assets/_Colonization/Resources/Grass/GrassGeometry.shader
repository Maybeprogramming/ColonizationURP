Shader "Custom/GrassGeometry"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.25, 0.5, 0.1)
        _TopColor ("Top Color", Color) = (0.4, 0.7, 0.15)
        _BladeHeight ("Blade Height", Range(0.1, 2)) = 0.6
        _BladeWidth ("Blade Width", Range(0.01, 0.2)) = 0.04
        _WindSpeed ("Wind Speed", Range(0, 5)) = 1.0
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.3
        _Jitter ("Height Jitter", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #pragma require geometry
            #pragma target 4.6

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            StructuredBuffer<float4> _Positions;

            float4 _BaseColor;
            float4 _TopColor;
            float _BladeHeight;
            float _BladeWidth;
            float _WindSpeed;
            float _WindStrength;
            float _Jitter;

            struct v2g
            {
                uint instanceID : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            v2g vert(uint instanceID : SV_InstanceID)
            {
                v2g o;
                o.instanceID = instanceID;
                return o;
            }

            float GetRandom(float seed)
            {
                return frac(sin(seed * 123.456) * 789.123);
            }

            [maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _BladeHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float width = _BladeWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * 6.28) * _WindStrength;
                float3 windOffset = float3(wind * height, 0, 0);

                float3 top = root + float3(0, height, 0) + windOffset;

                float3 vertices[6];
                vertices[0] = root + float3(-width, 0, 0);
                vertices[1] = root + float3(width, 0, 0);
                vertices[2] = top;
                vertices[3] = top;
                vertices[4] = root + float3(width, 0, 0);
                vertices[5] = top + float3(width * 0.2, 0, 0);

                float2 uvs[6];
                uvs[0] = float2(0, 0);
                uvs[1] = float2(1, 0);
                uvs[2] = float2(0, 1);
                uvs[3] = float2(0, 1);
                uvs[4] = float2(1, 0);
                uvs[5] = float2(1, 1);

                for (int i = 0; i < 6; i++)
                {
                    g2f o;
                    o.pos = TransformWorldToHClip(vertices[i]);
                    o.worldPos = vertices[i];
                    o.uv = uvs[i];
                    stream.Append(o);
                }
            }

            half4 frag(g2f i) : SV_Target
            {
                float3 color = lerp(_BaseColor.rgb, _TopColor.rgb, i.uv.y);

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(float3(0, 1, 0), mainLight.direction) * 0.5 + 0.5);
                color *= mainLight.color * (mainLight.shadowAttenuation * 0.6 + 0.4) * NdotL + 0.3;

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}