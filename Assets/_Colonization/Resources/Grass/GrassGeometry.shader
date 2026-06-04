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
            Tags { "LightMode" = "UniversalForward" }

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
                v2g output;
                output.instanceID = instanceID;
                return output;
            }

            float GetRandom(float seed)
            {
                return frac(sin(seed * 123.456) * 789.123);
            }

            [maxvertexcount(12)]
            void geom(point v2g input[1], inout TriangleStream<g2f> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _BladeHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float width = _BladeWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * 6.28) * _WindStrength;

                float3 viewDir = _WorldSpaceCameraPos - root;
                viewDir.y = 0;
                viewDir = normalize(viewDir);
                float3 bladeRight = cross(float3(0, 1, 0), viewDir);

                float3 windOffset = bladeRight * wind * height;

                float3 top = root + float3(0, height, 0) + windOffset;

                float3 rightVectors[2];
                rightVectors[0] = bladeRight;
                rightVectors[1] = viewDir;

                float2 uvCoordinates[12];
                uvCoordinates[0] = float2(0, 0);
                uvCoordinates[1] = float2(1, 0);
                uvCoordinates[2] = float2(0.5, 1);
                uvCoordinates[3] = float2(0.5, 1);
                uvCoordinates[4] = float2(1, 0);
                uvCoordinates[5] = float2(0.5, 1);
                uvCoordinates[6] = float2(0, 0);
                uvCoordinates[7] = float2(1, 0);
                uvCoordinates[8] = float2(0.5, 1);
                uvCoordinates[9] = float2(0.5, 1);
                uvCoordinates[10] = float2(1, 0);
                uvCoordinates[11] = float2(0.5, 1);

                for (int quad = 0; quad < 2; quad++)
                {
                    float3 right = rightVectors[quad];

                    float3 vertices[6];
                    vertices[0] = root + right * -width;
                    vertices[1] = root + right * width;
                    vertices[2] = top;
                    vertices[3] = top;
                    vertices[4] = root + right * width;
                    vertices[5] = top + right * width * 0.2;

                    for (int i = 0; i < 6; i++)
                    {
                        g2f output;
                        output.pos = TransformWorldToHClip(vertices[i]);
                        output.worldPos = vertices[i];
                        output.uv = uvCoordinates[quad * 6 + i];
                        stream.Append(output);
                    }

                    stream.RestartStrip();
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

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment fragShadow

            #pragma require geometry
            #pragma target 4.6

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            StructuredBuffer<float4> _Positions;

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
            };

            v2g vert(uint instanceID : SV_InstanceID)
            {
                v2g output;
                output.instanceID = instanceID;
                return output;
            }

            float GetRandom(float seed)
            {
                return frac(sin(seed * 123.456) * 789.123);
            }

            [maxvertexcount(12)]
            void geom(point v2g input[1], inout TriangleStream<g2f> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _BladeHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float width = _BladeWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * 6.28) * _WindStrength;

                float3 viewDir = _WorldSpaceCameraPos - root;
                viewDir.y = 0;
                viewDir = normalize(viewDir);
                float3 bladeRight = cross(float3(0, 1, 0), viewDir);

                float3 windOffset = bladeRight * wind * height;

                float3 top = root + float3(0, height, 0) + windOffset;

                float3 rightVectors[2];
                rightVectors[0] = bladeRight;
                rightVectors[1] = viewDir;

                float3 normalWS = float3(0, 1, 0);
                float3 lightDir = _MainLightPosition.xyz;

                for (int quad = 0; quad < 2; quad++)
                {
                    float3 right = rightVectors[quad];

                    float3 vertices[6];
                    vertices[0] = root + right * -width;
                    vertices[1] = root + right * width;
                    vertices[2] = top;
                    vertices[3] = top;
                    vertices[4] = root + right * width;
                    vertices[5] = top + right * width * 0.2;

                    for (int i = 0; i < 6; i++)
                    {
                        g2f output;
                        float3 biased = ApplyShadowBias(vertices[i], normalWS, lightDir);
                        output.pos = TransformWorldToHClip(biased);
                        stream.Append(output);
                    }

                    stream.RestartStrip();
                }
            }

            half4 fragShadow(g2f i) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
