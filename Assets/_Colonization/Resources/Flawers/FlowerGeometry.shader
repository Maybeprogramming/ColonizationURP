Shader "Custom/FlowerGeometry"
{
    Properties
    {
        [Space]
        [Header(Stem)]
        _StemHeight ("Stem Height", Range(0.3, 3)) = 1.2
        _StemWidth ("Stem Width", Range(0.01, 0.15)) = 0.04
        _StemColorBottom ("Stem Bottom Color", Color) = (0.2, 0.45, 0.08)
        _StemColorTop ("Stem Top Color", Color) = (0.35, 0.65, 0.12)
        [Space]
        [Header(Daisy)]
        _DaisyColor ("Daisy Petal Color", Color) = (1, 1, 1)
        _DaisyCenter ("Daisy Center Color", Color) = (1, 0.9, 0.1)
        [Space]
        [Header(Tulip)]
        _TulipColor ("Tulip Color", Color) = (1, 0.2, 0.2)
        [Space]
        [Header(Poppy)]
        _PoppyColor ("Poppy Color", Color) = (1, 0.1, 0.1)
        _PoppyCenter ("Poppy Center Color", Color) = (0.1, 0.05, 0.05)
        [Space]
        [Header(Global)]
        _HeadSize ("Head Size", Range(0.1, 1.5)) = 0.5
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
            #define TWO_PI 6.28318530718

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            StructuredBuffer<float4> _Positions;

            float4 _StemColorBottom;
            float4 _StemColorTop;
            float4 _DaisyColor;
            float4 _DaisyCenter;
            float4 _TulipColor;
            float4 _PoppyColor;
            float4 _PoppyCenter;
            float _StemHeight;
            float _StemWidth;
            float _HeadSize;
            float _WindSpeed;
            float _WindStrength;
            float _Jitter;

            struct VertexToGeometry
            {
                uint instanceID : TEXCOORD0;
            };

            struct GeometryToFragment
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 color : TEXCOORD2;
            };

            VertexToGeometry vert(uint instanceID : SV_InstanceID)
            {
                VertexToGeometry output;
                output.instanceID = instanceID;
                return output;
            }

            float GetRandom(float seed)
            {
                return frac(sin(seed * 123.456) * 789.123);
            }

            void EmitQuad(inout TriangleStream<GeometryToFragment> stream, float3 a, float3 b, float3 c, float3 d, float2 uvA, float2 uvB, float2 uvC, float2 uvD, float4 color)
            {
                GeometryToFragment output;
                output.color = color;

                output.pos = TransformWorldToHClip(a);
                output.worldPos = a;
                output.uv = uvA;
                stream.Append(output);

                output.pos = TransformWorldToHClip(c);
                output.worldPos = c;
                output.uv = uvC;
                stream.Append(output);

                output.pos = TransformWorldToHClip(b);
                output.worldPos = b;
                output.uv = uvB;
                stream.Append(output);

                output.pos = TransformWorldToHClip(b);
                output.worldPos = b;
                output.uv = uvB;
                stream.Append(output);

                output.pos = TransformWorldToHClip(c);
                output.worldPos = c;
                output.uv = uvC;
                stream.Append(output);

                output.pos = TransformWorldToHClip(d);
                output.worldPos = d;
                output.uv = uvD;
                stream.Append(output);

                stream.RestartStrip();
            }

            [maxvertexcount(72)]
            void geom(point VertexToGeometry input[1], inout TriangleStream<GeometryToFragment> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _StemHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float stemWidth = _StemWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float headSize = _HeadSize * (0.8 + GetRandom(seed + 3) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * TWO_PI) * _WindStrength;

                float3 viewDir = _WorldSpaceCameraPos - root;
                viewDir.y = 0;
                viewDir = normalize(viewDir);
                float3 stemRight = cross(float3(0, 1, 0), viewDir);

                float3 windOffset = stemRight * wind * height;
                float3 stemTop = root + float3(0, height, 0) + windOffset;

                float3 stemAxes[2];
                stemAxes[0] = stemRight;
                stemAxes[1] = viewDir;

                for (int stemIndex = 0; stemIndex < 2; stemIndex++)
                {
                    float3 axis = stemAxes[stemIndex];
                    float3 bottomLeft = root + axis * -stemWidth;
                    float3 bottomRight = root + axis * stemWidth;
                    float3 topLeft = stemTop + axis * -stemWidth * 0.3;
                    float3 topRight = stemTop + axis * stemWidth * 0.3;

                    GeometryToFragment output;
                    output.color = 0;

                    output.pos = TransformWorldToHClip(bottomLeft);
                    output.worldPos = bottomLeft;
                    output.uv = float2(0, 0);
                    output.color = lerp(_StemColorBottom, _StemColorTop, 0);
                    stream.Append(output);

                    output.pos = TransformWorldToHClip(topLeft);
                    output.worldPos = topLeft;
                    output.uv = float2(0.5, 1);
                    output.color = lerp(_StemColorBottom, _StemColorTop, 1);
                    stream.Append(output);

                    output.pos = TransformWorldToHClip(bottomRight);
                    output.worldPos = bottomRight;
                    output.uv = float2(1, 0);
                    output.color = lerp(_StemColorBottom, _StemColorTop, 0);
                    stream.Append(output);

                    output.pos = TransformWorldToHClip(bottomRight);
                    output.worldPos = bottomRight;
                    output.uv = float2(1, 0);
                    output.color = lerp(_StemColorBottom, _StemColorTop, 0);
                    stream.Append(output);

                    output.pos = TransformWorldToHClip(topLeft);
                    output.worldPos = topLeft;
                    output.uv = float2(0.5, 1);
                    output.color = lerp(_StemColorBottom, _StemColorTop, 1);
                    stream.Append(output);

                    output.pos = TransformWorldToHClip(topRight);
                    output.worldPos = topRight;
                    output.uv = float2(0.5, 1);
                    output.color = lerp(_StemColorBottom, _StemColorTop, 1);
                    stream.Append(output);

                    stream.RestartStrip();
                }

                float randomType = GetRandom(seed + 5);
                int flowerType = (int)(randomType * 3);

                if (flowerType == 0)
                {
                    int daisyPetals = 8;
                    float daisyPetalLength = headSize * 0.5;

                    for (int petalIndex = 0; petalIndex < daisyPetals; petalIndex++)
                    {
                        float angle = (float)petalIndex / daisyPetals * TWO_PI;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + daisyPetalLength) + float3(0, headSize * 0.12, 0);

                        float baseHalfWidth = headSize * 0.035;
                        float tipHalfWidth = headSize * 0.065;

                        float3 bottomLeft = base + widthDir * -baseHalfWidth;
                        float3 bottomRight = base + widthDir * baseHalfWidth;
                        float3 topLeft = tip + widthDir * -tipHalfWidth;
                        float3 topRight = tip + widthDir * tipHalfWidth;

                        GeometryToFragment output;
                        output.color = 0;

                        output.pos = TransformWorldToHClip(bottomLeft);
                        output.worldPos = bottomLeft;
                        output.uv = float2(0, 0);
                        output.color = _DaisyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topLeft);
                        output.worldPos = topLeft;
                        output.uv = float2(0, 1);
                        output.color = _DaisyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(bottomRight);
                        output.worldPos = bottomRight;
                        output.uv = float2(1, 0);
                        output.color = _DaisyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(bottomRight);
                        output.worldPos = bottomRight;
                        output.uv = float2(1, 0);
                        output.color = _DaisyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topLeft);
                        output.worldPos = topLeft;
                        output.uv = float2(0, 1);
                        output.color = _DaisyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topRight);
                        output.worldPos = topRight;
                        output.uv = float2(1, 1);
                        output.color = _DaisyColor;
                        stream.Append(output);

                        stream.RestartStrip();
                    }

                    float centerSize = headSize * 0.08;
                    float3 centerUp = stemTop + float3(0, centerSize * 0.3, 0);
                    float3 centerViewDir = _WorldSpaceCameraPos - centerUp;
                    centerViewDir.y = 0;
                    centerViewDir = normalize(centerViewDir);
                    float3 centerRight = normalize(cross(float3(0, 1, 0), centerViewDir));

                    for (int centerQuadIndex = 0; centerQuadIndex < 2; centerQuadIndex++)
                    {
                        float3 centerAxis = (centerQuadIndex == 0) ? centerRight : centerViewDir;
                        float3 centerBottomLeft = centerUp + centerAxis * -centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerBottomRight = centerUp + centerAxis * centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerTopLeft = centerUp + centerAxis * -centerSize + float3(0, centerSize * 0.5, 0);
                        float3 centerTopRight = centerUp + centerAxis * centerSize + float3(0, centerSize * 0.5, 0);

                        GeometryToFragment output;
                        output.color = 0;

                        output.pos = TransformWorldToHClip(centerBottomLeft);
                        output.worldPos = centerBottomLeft;
                        output.uv = float2(0, 0);
                        output.color = _DaisyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerTopLeft);
                        output.worldPos = centerTopLeft;
                        output.uv = float2(0, 1);
                        output.color = _DaisyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerBottomRight);
                        output.worldPos = centerBottomRight;
                        output.uv = float2(1, 0);
                        output.color = _DaisyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerBottomRight);
                        output.worldPos = centerBottomRight;
                        output.uv = float2(1, 0);
                        output.color = _DaisyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerTopLeft);
                        output.worldPos = centerTopLeft;
                        output.uv = float2(0, 1);
                        output.color = _DaisyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerTopRight);
                        output.worldPos = centerTopRight;
                        output.uv = float2(1, 1);
                        output.color = _DaisyCenter;
                        stream.Append(output);

                        stream.RestartStrip();
                    }
                }
                else if (flowerType == 1)
                {
                    int tulipPetals = 5;
                    float tulipHeight = headSize * 0.4;

                    for (int petalIndex = 0; petalIndex < tulipPetals; petalIndex++)
                    {
                        float angle = (float)petalIndex / tulipPetals * TWO_PI;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.02;
                        float3 tip = stemTop + outward * headSize * 0.35 + float3(0, tulipHeight, 0);
                        tip += outward * headSize * 0.1;

                        float baseHalfWidth = headSize * 0.18;
                        float tipHalfWidth = headSize * 0.35;

                        float3 bottomLeft = base + widthDir * -baseHalfWidth;
                        float3 bottomRight = base + widthDir * baseHalfWidth;
                        float3 topLeft = tip + widthDir * -tipHalfWidth;
                        float3 topRight = tip + widthDir * tipHalfWidth;

                        GeometryToFragment output;
                        output.color = 0;

                        output.pos = TransformWorldToHClip(bottomLeft);
                        output.worldPos = bottomLeft;
                        output.uv = float2(0, 0);
                        output.color = _TulipColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topLeft);
                        output.worldPos = topLeft;
                        output.uv = float2(0, 1);
                        output.color = _TulipColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(bottomRight);
                        output.worldPos = bottomRight;
                        output.uv = float2(1, 0);
                        output.color = _TulipColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(bottomRight);
                        output.worldPos = bottomRight;
                        output.uv = float2(1, 0);
                        output.color = _TulipColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topLeft);
                        output.worldPos = topLeft;
                        output.uv = float2(0, 1);
                        output.color = _TulipColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topRight);
                        output.worldPos = topRight;
                        output.uv = float2(1, 1);
                        output.color = _TulipColor;
                        stream.Append(output);

                        stream.RestartStrip();
                    }
                }
                else
                {
                    int poppyPetals = 4;
                    float poppyPetalLength = headSize * 0.35;

                    for (int petalIndex = 0; petalIndex < poppyPetals; petalIndex++)
                    {
                        float angle = (float)petalIndex / poppyPetals * TWO_PI;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + poppyPetalLength) + float3(0, headSize * 0.18, 0);
                        tip += float3(0, -headSize * 0.04, 0);

                        float baseHalfWidth = headSize * 0.18;
                        float tipHalfWidth = headSize * 0.5;

                        float3 bottomLeft = base + widthDir * -baseHalfWidth;
                        float3 bottomRight = base + widthDir * baseHalfWidth;
                        float3 topLeft = tip + widthDir * -tipHalfWidth;
                        float3 topRight = tip + widthDir * tipHalfWidth;

                        GeometryToFragment output;
                        output.color = 0;

                        output.pos = TransformWorldToHClip(bottomLeft);
                        output.worldPos = bottomLeft;
                        output.uv = float2(0, 0);
                        output.color = _PoppyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topLeft);
                        output.worldPos = topLeft;
                        output.uv = float2(0, 1);
                        output.color = _PoppyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(bottomRight);
                        output.worldPos = bottomRight;
                        output.uv = float2(1, 0);
                        output.color = _PoppyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(bottomRight);
                        output.worldPos = bottomRight;
                        output.uv = float2(1, 0);
                        output.color = _PoppyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topLeft);
                        output.worldPos = topLeft;
                        output.uv = float2(0, 1);
                        output.color = _PoppyColor;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(topRight);
                        output.worldPos = topRight;
                        output.uv = float2(1, 1);
                        output.color = _PoppyColor;
                        stream.Append(output);

                        stream.RestartStrip();
                    }

                    float centerSize = headSize * 0.09;
                    float3 centerUp = stemTop + float3(0, centerSize * 0.25, 0);
                    float3 centerViewDir = _WorldSpaceCameraPos - centerUp;
                    centerViewDir.y = 0;
                    centerViewDir = normalize(centerViewDir);
                    float3 centerRight = normalize(cross(float3(0, 1, 0), centerViewDir));

                    for (int centerQuadIndex = 0; centerQuadIndex < 2; centerQuadIndex++)
                    {
                        float3 centerAxis = (centerQuadIndex == 0) ? centerRight : centerViewDir;
                        float3 centerBottomLeft = centerUp + centerAxis * -centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerBottomRight = centerUp + centerAxis * centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerTopLeft = centerUp + centerAxis * -centerSize + float3(0, centerSize * 0.5, 0);
                        float3 centerTopRight = centerUp + centerAxis * centerSize + float3(0, centerSize * 0.5, 0);

                        GeometryToFragment output;
                        output.color = 0;

                        output.pos = TransformWorldToHClip(centerBottomLeft);
                        output.worldPos = centerBottomLeft;
                        output.uv = float2(0, 0);
                        output.color = _PoppyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerTopLeft);
                        output.worldPos = centerTopLeft;
                        output.uv = float2(0, 1);
                        output.color = _PoppyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerBottomRight);
                        output.worldPos = centerBottomRight;
                        output.uv = float2(1, 0);
                        output.color = _PoppyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerBottomRight);
                        output.worldPos = centerBottomRight;
                        output.uv = float2(1, 0);
                        output.color = _PoppyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerTopLeft);
                        output.worldPos = centerTopLeft;
                        output.uv = float2(0, 1);
                        output.color = _PoppyCenter;
                        stream.Append(output);

                        output.pos = TransformWorldToHClip(centerTopRight);
                        output.worldPos = centerTopRight;
                        output.uv = float2(1, 1);
                        output.color = _PoppyCenter;
                        stream.Append(output);

                        stream.RestartStrip();
                    }
                }
            }

            half4 frag(GeometryToFragment input) : SV_Target
            {
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(float3(0, 1, 0), mainLight.direction) * 0.5 + 0.5);
                float3 lit = input.color.rgb * mainLight.color * (mainLight.shadowAttenuation * 0.6 + 0.4) * NdotL + input.color.rgb * 0.3;
                return half4(lit, 1);
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
            #define TWO_PI 6.28318530718

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            StructuredBuffer<float4> _Positions;

            float _StemHeight;
            float _StemWidth;
            float _HeadSize;
            float _WindSpeed;
            float _WindStrength;
            float _Jitter;

            struct VertexToGeometry
            {
                uint instanceID : TEXCOORD0;
            };

            struct GeometryToFragment
            {
                float4 pos : SV_POSITION;
            };

            VertexToGeometry vert(uint instanceID : SV_InstanceID)
            {
                VertexToGeometry output;
                output.instanceID = instanceID;
                return output;
            }

            float GetRandom(float seed)
            {
                return frac(sin(seed * 123.456) * 789.123);
            }

            [maxvertexcount(72)]
            void geom(point VertexToGeometry input[1], inout TriangleStream<GeometryToFragment> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _StemHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float stemWidth = _StemWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float headSize = _HeadSize * (0.8 + GetRandom(seed + 3) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * TWO_PI) * _WindStrength;

                float3 viewDir = _WorldSpaceCameraPos - root;
                viewDir.y = 0;
                viewDir = normalize(viewDir);
                float3 stemRight = cross(float3(0, 1, 0), viewDir);

                float3 windOffset = stemRight * wind * height;
                float3 stemTop = root + float3(0, height, 0) + windOffset;

                float3 normalWS = float3(0, 1, 0);
                float3 lightDir = _MainLightPosition.xyz;

                float3 stemAxes[2];
                stemAxes[0] = stemRight;
                stemAxes[1] = viewDir;

                for (int stemIndex = 0; stemIndex < 2; stemIndex++)
                {
                    float3 axis = stemAxes[stemIndex];
                    float3 bottomLeft = root + axis * -stemWidth;
                    float3 bottomRight = root + axis * stemWidth;
                    float3 topLeft = stemTop + axis * -stemWidth * 0.3;
                    float3 topRight = stemTop + axis * stemWidth * 0.3;

                    float3 vertexPositions[6] = { bottomLeft, topLeft, bottomRight, bottomRight, topLeft, topRight };
                    for (int i = 0; i < 6; i++)
                    {
                        GeometryToFragment output;
                        float3 biased = ApplyShadowBias(vertexPositions[i], normalWS, lightDir);
                        output.pos = TransformWorldToHClip(biased);
                        stream.Append(output);
                    }
                    stream.RestartStrip();
                }

                float randomType = GetRandom(seed + 5);
                int flowerType = (int)(randomType * 3);

                if (flowerType == 0)
                {
                    int daisyPetals = 8;
                    float daisyPetalLength = headSize * 0.5;

                    for (int petalIndex = 0; petalIndex < daisyPetals; petalIndex++)
                    {
                        float angle = (float)petalIndex / daisyPetals * TWO_PI;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + daisyPetalLength) + float3(0, headSize * 0.12, 0);

                        float baseHalfWidth = headSize * 0.035;
                        float tipHalfWidth = headSize * 0.065;

                        float3 bottomLeft = base + widthDir * -baseHalfWidth;
                        float3 bottomRight = base + widthDir * baseHalfWidth;
                        float3 topLeft = tip + widthDir * -tipHalfWidth;
                        float3 topRight = tip + widthDir * tipHalfWidth;

                        float3 vertexPositions[6] = { bottomLeft, topLeft, bottomRight, bottomRight, topLeft, topRight };
                        for (int i = 0; i < 6; i++)
                        {
                            GeometryToFragment output;
                            float3 biased = ApplyShadowBias(vertexPositions[i], normalWS, lightDir);
                            output.pos = TransformWorldToHClip(biased);
                            stream.Append(output);
                        }
                        stream.RestartStrip();
                    }

                    float centerSize = headSize * 0.08;
                    float3 centerUp = stemTop + float3(0, centerSize * 0.3, 0);
                    float3 centerCamDir = _WorldSpaceCameraPos - centerUp;
                    centerCamDir.y = 0;
                    centerCamDir = normalize(centerCamDir);
                    float3 centerRightDir = normalize(cross(float3(0, 1, 0), centerCamDir));

                    for (int centerQuadIndex = 0; centerQuadIndex < 2; centerQuadIndex++)
                    {
                        float3 centerAxis = (centerQuadIndex == 0) ? centerRightDir : centerCamDir;
                        float3 centerBottomLeft = centerUp + centerAxis * -centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerBottomRight = centerUp + centerAxis * centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerTopLeft = centerUp + centerAxis * -centerSize + float3(0, centerSize * 0.5, 0);
                        float3 centerTopRight = centerUp + centerAxis * centerSize + float3(0, centerSize * 0.5, 0);

                        float3 vertexPositions[6] = { centerBottomLeft, centerTopLeft, centerBottomRight, centerBottomRight, centerTopLeft, centerTopRight };
                        for (int i = 0; i < 6; i++)
                        {
                            GeometryToFragment output;
                            float3 biased = ApplyShadowBias(vertexPositions[i], normalWS, lightDir);
                            output.pos = TransformWorldToHClip(biased);
                            stream.Append(output);
                        }
                        stream.RestartStrip();
                    }
                }
                else if (flowerType == 1)
                {
                    int tulipPetals = 5;
                    float tulipHeight = headSize * 0.4;

                    for (int petalIndex = 0; petalIndex < tulipPetals; petalIndex++)
                    {
                        float angle = (float)petalIndex / tulipPetals * TWO_PI;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.02;
                        float3 tip = stemTop + outward * headSize * 0.35 + float3(0, tulipHeight, 0);
                        tip += outward * headSize * 0.1;

                        float baseHalfWidth = headSize * 0.18;
                        float tipHalfWidth = headSize * 0.35;

                        float3 bottomLeft = base + widthDir * -baseHalfWidth;
                        float3 bottomRight = base + widthDir * baseHalfWidth;
                        float3 topLeft = tip + widthDir * -tipHalfWidth;
                        float3 topRight = tip + widthDir * tipHalfWidth;

                        float3 vertexPositions[6] = { bottomLeft, topLeft, bottomRight, bottomRight, topLeft, topRight };
                        for (int i = 0; i < 6; i++)
                        {
                            GeometryToFragment output;
                            float3 biased = ApplyShadowBias(vertexPositions[i], normalWS, lightDir);
                            output.pos = TransformWorldToHClip(biased);
                            stream.Append(output);
                        }
                        stream.RestartStrip();
                    }
                }
                else
                {
                    int poppyPetals = 4;
                    float poppyPetalLength = headSize * 0.35;

                    for (int petalIndex = 0; petalIndex < poppyPetals; petalIndex++)
                    {
                        float angle = (float)petalIndex / poppyPetals * TWO_PI;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + poppyPetalLength) + float3(0, headSize * 0.18, 0);
                        tip += float3(0, -headSize * 0.04, 0);

                        float baseHalfWidth = headSize * 0.18;
                        float tipHalfWidth = headSize * 0.5;

                        float3 bottomLeft = base + widthDir * -baseHalfWidth;
                        float3 bottomRight = base + widthDir * baseHalfWidth;
                        float3 topLeft = tip + widthDir * -tipHalfWidth;
                        float3 topRight = tip + widthDir * tipHalfWidth;

                        float3 vertexPositions[6] = { bottomLeft, topLeft, bottomRight, bottomRight, topLeft, topRight };
                        for (int i = 0; i < 6; i++)
                        {
                            GeometryToFragment output;
                            float3 biased = ApplyShadowBias(vertexPositions[i], normalWS, lightDir);
                            output.pos = TransformWorldToHClip(biased);
                            stream.Append(output);
                        }
                        stream.RestartStrip();
                    }

                    float centerSize = headSize * 0.09;
                    float3 centerUp = stemTop + float3(0, centerSize * 0.25, 0);
                    float3 centerCamDir = _WorldSpaceCameraPos - centerUp;
                    centerCamDir.y = 0;
                    centerCamDir = normalize(centerCamDir);
                    float3 centerRightDir = normalize(cross(float3(0, 1, 0), centerCamDir));

                    for (int centerQuadIndex = 0; centerQuadIndex < 2; centerQuadIndex++)
                    {
                        float3 centerAxis = (centerQuadIndex == 0) ? centerRightDir : centerCamDir;
                        float3 centerBottomLeft = centerUp + centerAxis * -centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerBottomRight = centerUp + centerAxis * centerSize + float3(0, -centerSize * 0.5, 0);
                        float3 centerTopLeft = centerUp + centerAxis * -centerSize + float3(0, centerSize * 0.5, 0);
                        float3 centerTopRight = centerUp + centerAxis * centerSize + float3(0, centerSize * 0.5, 0);

                        float3 vertexPositions[6] = { centerBottomLeft, centerTopLeft, centerBottomRight, centerBottomRight, centerTopLeft, centerTopRight };
                        for (int i = 0; i < 6; i++)
                        {
                            GeometryToFragment output;
                            float3 biased = ApplyShadowBias(vertexPositions[i], normalWS, lightDir);
                            output.pos = TransformWorldToHClip(biased);
                            stream.Append(output);
                        }
                        stream.RestartStrip();
                    }
                }
            }

            half4 fragShadow(GeometryToFragment input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
