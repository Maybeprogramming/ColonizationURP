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

            struct v2g
            {
                uint instanceID : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 color : TEXCOORD2;
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

            void EmitQuad(inout TriangleStream<g2f> stream, float3 a, float3 b, float3 c, float3 d, float2 uvA, float2 uvB, float2 uvC, float2 uvD, float4 color)
            {
                g2f o;
                o.color = color;

                o.pos = TransformWorldToHClip(a);
                o.worldPos = a;
                o.uv = uvA;
                stream.Append(o);

                o.pos = TransformWorldToHClip(c);
                o.worldPos = c;
                o.uv = uvC;
                stream.Append(o);

                o.pos = TransformWorldToHClip(b);
                o.worldPos = b;
                o.uv = uvB;
                stream.Append(o);

                o.pos = TransformWorldToHClip(b);
                o.worldPos = b;
                o.uv = uvB;
                stream.Append(o);

                o.pos = TransformWorldToHClip(c);
                o.worldPos = c;
                o.uv = uvC;
                stream.Append(o);

                o.pos = TransformWorldToHClip(d);
                o.worldPos = d;
                o.uv = uvD;
                stream.Append(o);

                stream.RestartStrip();
            }

            [maxvertexcount(72)]
            void geom(point v2g input[1], inout TriangleStream<g2f> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _StemHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float stemWidth = _StemWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float headSize = _HeadSize * (0.8 + GetRandom(seed + 3) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * 6.28) * _WindStrength;

                float3 viewDir = _WorldSpaceCameraPos - root;
                viewDir.y = 0;
                viewDir = normalize(viewDir);
                float3 stemRight = cross(float3(0, 1, 0), viewDir);

                float3 windOffset = stemRight * wind * height;
                float3 stemTop = root + float3(0, height, 0) + windOffset;

                float3 stemAxes[2];
                stemAxes[0] = stemRight;
                stemAxes[1] = viewDir;

                for (int s = 0; s < 2; s++)
                {
                    float3 axis = stemAxes[s];
                    float3 bL = root + axis * -stemWidth;
                    float3 bR = root + axis * stemWidth;
                    float3 tL = stemTop + axis * -stemWidth * 0.3;
                    float3 tR = stemTop + axis * stemWidth * 0.3;

                    g2f o;
                    o.color = 0;

                    o.pos = TransformWorldToHClip(bL);
                    o.worldPos = bL;
                    o.uv = float2(0, 0);
                    o.color = lerp(_StemColorBottom, _StemColorTop, 0);
                    stream.Append(o);

                    o.pos = TransformWorldToHClip(tL);
                    o.worldPos = tL;
                    o.uv = float2(0.5, 1);
                    o.color = lerp(_StemColorBottom, _StemColorTop, 1);
                    stream.Append(o);

                    o.pos = TransformWorldToHClip(bR);
                    o.worldPos = bR;
                    o.uv = float2(1, 0);
                    o.color = lerp(_StemColorBottom, _StemColorTop, 0);
                    stream.Append(o);

                    o.pos = TransformWorldToHClip(bR);
                    o.worldPos = bR;
                    o.uv = float2(1, 0);
                    o.color = lerp(_StemColorBottom, _StemColorTop, 0);
                    stream.Append(o);

                    o.pos = TransformWorldToHClip(tL);
                    o.worldPos = tL;
                    o.uv = float2(0.5, 1);
                    o.color = lerp(_StemColorBottom, _StemColorTop, 1);
                    stream.Append(o);

                    o.pos = TransformWorldToHClip(tR);
                    o.worldPos = tR;
                    o.uv = float2(0.5, 1);
                    o.color = lerp(_StemColorBottom, _StemColorTop, 1);
                    stream.Append(o);

                    stream.RestartStrip();
                }

                float typeRand = GetRandom(seed + 5);
                int ft = (int)(typeRand * 3);

                if (ft == 0)
                {
                    int daisyPetals = 8;
                    float daisyLen = headSize * 0.5;

                    for (int p = 0; p < daisyPetals; p++)
                    {
                        float angle = (float)p / daisyPetals * 6.28319;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + daisyLen) + float3(0, headSize * 0.12, 0);

                        float baseHW = headSize * 0.035;
                        float tipHW = headSize * 0.065;

                        float3 bL = base + widthDir * -baseHW;
                        float3 bR = base + widthDir * baseHW;
                        float3 tL = tip + widthDir * -tipHW;
                        float3 tR = tip + widthDir * tipHW;

                        g2f o;
                        o.color = 0;

                        o.pos = TransformWorldToHClip(bL);
                        o.worldPos = bL;
                        o.uv = float2(0, 0);
                        o.color = _DaisyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tL);
                        o.worldPos = tL;
                        o.uv = float2(0, 1);
                        o.color = _DaisyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(bR);
                        o.worldPos = bR;
                        o.uv = float2(1, 0);
                        o.color = _DaisyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(bR);
                        o.worldPos = bR;
                        o.uv = float2(1, 0);
                        o.color = _DaisyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tL);
                        o.worldPos = tL;
                        o.uv = float2(0, 1);
                        o.color = _DaisyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tR);
                        o.worldPos = tR;
                        o.uv = float2(1, 1);
                        o.color = _DaisyColor;
                        stream.Append(o);

                        stream.RestartStrip();
                    }

                    float cSize = headSize * 0.08;
                    float3 cUp = stemTop + float3(0, cSize * 0.3, 0);
                    float3 cView = _WorldSpaceCameraPos - cUp;
                    cView.y = 0;
                    cView = normalize(cView);
                    float3 cRight = normalize(cross(float3(0, 1, 0), cView));

                    for (int cq = 0; cq < 2; cq++)
                    {
                        float3 ca = (cq == 0) ? cRight : cView;
                        float3 cBL = cUp + ca * -cSize + float3(0, -cSize * 0.5, 0);
                        float3 cBR = cUp + ca * cSize + float3(0, -cSize * 0.5, 0);
                        float3 cTL = cUp + ca * -cSize + float3(0, cSize * 0.5, 0);
                        float3 cTR = cUp + ca * cSize + float3(0, cSize * 0.5, 0);

                        g2f o;
                        o.color = 0;

                        o.pos = TransformWorldToHClip(cBL);
                        o.worldPos = cBL;
                        o.uv = float2(0, 0);
                        o.color = _DaisyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cTL);
                        o.worldPos = cTL;
                        o.uv = float2(0, 1);
                        o.color = _DaisyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cBR);
                        o.worldPos = cBR;
                        o.uv = float2(1, 0);
                        o.color = _DaisyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cBR);
                        o.worldPos = cBR;
                        o.uv = float2(1, 0);
                        o.color = _DaisyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cTL);
                        o.worldPos = cTL;
                        o.uv = float2(0, 1);
                        o.color = _DaisyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cTR);
                        o.worldPos = cTR;
                        o.uv = float2(1, 1);
                        o.color = _DaisyCenter;
                        stream.Append(o);

                        stream.RestartStrip();
                    }
                }
                else if (ft == 1)
                {
                    int tulipPetals = 5;
                    float tHeight = headSize * 0.4;

                    for (int p = 0; p < tulipPetals; p++)
                    {
                        float angle = (float)p / tulipPetals * 6.28319;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.02;
                        float3 tip = stemTop + outward * headSize * 0.35 + float3(0, tHeight, 0);
                        tip += outward * headSize * 0.1;

                        float baseHW = headSize * 0.18;
                        float tipHW = headSize * 0.35;

                        float3 bL = base + widthDir * -baseHW;
                        float3 bR = base + widthDir * baseHW;
                        float3 tL = tip + widthDir * -tipHW;
                        float3 tR = tip + widthDir * tipHW;

                        g2f o;
                        o.color = 0;

                        o.pos = TransformWorldToHClip(bL);
                        o.worldPos = bL;
                        o.uv = float2(0, 0);
                        o.color = _TulipColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tL);
                        o.worldPos = tL;
                        o.uv = float2(0, 1);
                        o.color = _TulipColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(bR);
                        o.worldPos = bR;
                        o.uv = float2(1, 0);
                        o.color = _TulipColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(bR);
                        o.worldPos = bR;
                        o.uv = float2(1, 0);
                        o.color = _TulipColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tL);
                        o.worldPos = tL;
                        o.uv = float2(0, 1);
                        o.color = _TulipColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tR);
                        o.worldPos = tR;
                        o.uv = float2(1, 1);
                        o.color = _TulipColor;
                        stream.Append(o);

                        stream.RestartStrip();
                    }
                }
                else
                {
                    int poppyPetals = 4;
                    float pLen = headSize * 0.35;

                    for (int p = 0; p < poppyPetals; p++)
                    {
                        float angle = (float)p / poppyPetals * 6.28319;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + pLen) + float3(0, headSize * 0.18, 0);
                        tip += float3(0, -headSize * 0.04, 0);

                        float baseHW = headSize * 0.18;
                        float tipHW = headSize * 0.5;

                        float3 bL = base + widthDir * -baseHW;
                        float3 bR = base + widthDir * baseHW;
                        float3 tL = tip + widthDir * -tipHW;
                        float3 tR = tip + widthDir * tipHW;

                        g2f o;
                        o.color = 0;

                        o.pos = TransformWorldToHClip(bL);
                        o.worldPos = bL;
                        o.uv = float2(0, 0);
                        o.color = _PoppyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tL);
                        o.worldPos = tL;
                        o.uv = float2(0, 1);
                        o.color = _PoppyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(bR);
                        o.worldPos = bR;
                        o.uv = float2(1, 0);
                        o.color = _PoppyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(bR);
                        o.worldPos = bR;
                        o.uv = float2(1, 0);
                        o.color = _PoppyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tL);
                        o.worldPos = tL;
                        o.uv = float2(0, 1);
                        o.color = _PoppyColor;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(tR);
                        o.worldPos = tR;
                        o.uv = float2(1, 1);
                        o.color = _PoppyColor;
                        stream.Append(o);

                        stream.RestartStrip();
                    }

                    float cSize = headSize * 0.09;
                    float3 cUp = stemTop + float3(0, cSize * 0.25, 0);
                    float3 cView = _WorldSpaceCameraPos - cUp;
                    cView.y = 0;
                    cView = normalize(cView);
                    float3 cRight = normalize(cross(float3(0, 1, 0), cView));

                    for (int cq = 0; cq < 2; cq++)
                    {
                        float3 ca = (cq == 0) ? cRight : cView;
                        float3 cBL = cUp + ca * -cSize + float3(0, -cSize * 0.5, 0);
                        float3 cBR = cUp + ca * cSize + float3(0, -cSize * 0.5, 0);
                        float3 cTL = cUp + ca * -cSize + float3(0, cSize * 0.5, 0);
                        float3 cTR = cUp + ca * cSize + float3(0, cSize * 0.5, 0);

                        g2f o;
                        o.color = 0;

                        o.pos = TransformWorldToHClip(cBL);
                        o.worldPos = cBL;
                        o.uv = float2(0, 0);
                        o.color = _PoppyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cTL);
                        o.worldPos = cTL;
                        o.uv = float2(0, 1);
                        o.color = _PoppyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cBR);
                        o.worldPos = cBR;
                        o.uv = float2(1, 0);
                        o.color = _PoppyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cBR);
                        o.worldPos = cBR;
                        o.uv = float2(1, 0);
                        o.color = _PoppyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cTL);
                        o.worldPos = cTL;
                        o.uv = float2(0, 1);
                        o.color = _PoppyCenter;
                        stream.Append(o);

                        o.pos = TransformWorldToHClip(cTR);
                        o.worldPos = cTR;
                        o.uv = float2(1, 1);
                        o.color = _PoppyCenter;
                        stream.Append(o);

                        stream.RestartStrip();
                    }
                }
            }

            half4 frag(g2f i) : SV_Target
            {
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(float3(0, 1, 0), mainLight.direction) * 0.5 + 0.5);
                float3 lit = i.color.rgb * mainLight.color * (mainLight.shadowAttenuation * 0.6 + 0.4) * NdotL + i.color.rgb * 0.3;
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            StructuredBuffer<float4> _Positions;

            float _StemHeight;
            float _StemWidth;
            float _HeadSize;
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
                v2g o;
                o.instanceID = instanceID;
                return o;
            }

            float GetRandom(float seed)
            {
                return frac(sin(seed * 123.456) * 789.123);
            }

            [maxvertexcount(72)]
            void geom(point v2g input[1], inout TriangleStream<g2f> stream)
            {
                uint id = input[0].instanceID;
                float4 data = _Positions[id];
                float3 root = data.xyz;
                float seed = data.w;

                float height = _StemHeight * (1 - _Jitter + GetRandom(seed + 1) * _Jitter);
                float stemWidth = _StemWidth * (0.8 + GetRandom(seed + 2) * 0.4);
                float headSize = _HeadSize * (0.8 + GetRandom(seed + 3) * 0.4);
                float wind = sin(_Time.y * _WindSpeed + seed * 6.28) * _WindStrength;

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

                for (int s = 0; s < 2; s++)
                {
                    float3 axis = stemAxes[s];
                    float3 bL = root + axis * -stemWidth;
                    float3 bR = root + axis * stemWidth;
                    float3 tL = stemTop + axis * -stemWidth * 0.3;
                    float3 tR = stemTop + axis * stemWidth * 0.3;

                    float3 verts[6] = { bL, tL, bR, bR, tL, tR };
                    for (int i = 0; i < 6; i++)
                    {
                        g2f o;
                        float3 biased = ApplyShadowBias(verts[i], normalWS, lightDir);
                        o.pos = TransformWorldToHClip(biased);
                        stream.Append(o);
                    }
                    stream.RestartStrip();
                }

                float typeRand = GetRandom(seed + 5);
                int ft = (int)(typeRand * 3);

                if (ft == 0)
                {
                    int daisyPetals = 8;
                    float daisyLen = headSize * 0.5;

                    for (int p = 0; p < daisyPetals; p++)
                    {
                        float angle = (float)p / daisyPetals * 6.28319;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + daisyLen) + float3(0, headSize * 0.12, 0);

                        float baseHW = headSize * 0.035;
                        float tipHW = headSize * 0.065;

                        float3 bL = base + widthDir * -baseHW;
                        float3 bR = base + widthDir * baseHW;
                        float3 tL = tip + widthDir * -tipHW;
                        float3 tR = tip + widthDir * tipHW;

                        float3 verts[6] = { bL, tL, bR, bR, tL, tR };
                        for (int i = 0; i < 6; i++)
                        {
                            g2f o;
                            float3 biased = ApplyShadowBias(verts[i], normalWS, lightDir);
                            o.pos = TransformWorldToHClip(biased);
                            stream.Append(o);
                        }
                        stream.RestartStrip();
                    }

                    float cSize = headSize * 0.08;
                    float3 cUp = stemTop + float3(0, cSize * 0.3, 0);
                    float3 cCamDir = _WorldSpaceCameraPos - cUp;
                    cCamDir.y = 0;
                    cCamDir = normalize(cCamDir);
                    float3 cRightDir = normalize(cross(float3(0, 1, 0), cCamDir));

                    for (int cq = 0; cq < 2; cq++)
                    {
                        float3 ca = (cq == 0) ? cRightDir : cCamDir;
                        float3 cBL = cUp + ca * -cSize + float3(0, -cSize * 0.5, 0);
                        float3 cBR = cUp + ca * cSize + float3(0, -cSize * 0.5, 0);
                        float3 cTL = cUp + ca * -cSize + float3(0, cSize * 0.5, 0);
                        float3 cTR = cUp + ca * cSize + float3(0, cSize * 0.5, 0);

                        float3 verts[6] = { cBL, cTL, cBR, cBR, cTL, cTR };
                        for (int i = 0; i < 6; i++)
                        {
                            g2f o;
                            float3 biased = ApplyShadowBias(verts[i], normalWS, lightDir);
                            o.pos = TransformWorldToHClip(biased);
                            stream.Append(o);
                        }
                        stream.RestartStrip();
                    }
                }
                else if (ft == 1)
                {
                    int tulipPetals = 5;
                    float tHeight = headSize * 0.4;

                    for (int p = 0; p < tulipPetals; p++)
                    {
                        float angle = (float)p / tulipPetals * 6.28319;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.02;
                        float3 tip = stemTop + outward * headSize * 0.35 + float3(0, tHeight, 0);
                        tip += outward * headSize * 0.1;

                        float baseHW = headSize * 0.18;
                        float tipHW = headSize * 0.35;

                        float3 bL = base + widthDir * -baseHW;
                        float3 bR = base + widthDir * baseHW;
                        float3 tL = tip + widthDir * -tipHW;
                        float3 tR = tip + widthDir * tipHW;

                        float3 verts[6] = { bL, tL, bR, bR, tL, tR };
                        for (int i = 0; i < 6; i++)
                        {
                            g2f o;
                            float3 biased = ApplyShadowBias(verts[i], normalWS, lightDir);
                            o.pos = TransformWorldToHClip(biased);
                            stream.Append(o);
                        }
                        stream.RestartStrip();
                    }
                }
                else
                {
                    int poppyPetals = 4;
                    float pLen = headSize * 0.35;

                    for (int p = 0; p < poppyPetals; p++)
                    {
                        float angle = (float)p / poppyPetals * 6.28319;
                        float3 outward = float3(cos(angle), 0, sin(angle));
                        float3 widthDir = normalize(cross(outward, float3(0, 1, 0)));

                        float3 base = stemTop + outward * headSize * 0.04;
                        float3 tip = stemTop + outward * (headSize * 0.04 + pLen) + float3(0, headSize * 0.18, 0);
                        tip += float3(0, -headSize * 0.04, 0);

                        float baseHW = headSize * 0.18;
                        float tipHW = headSize * 0.5;

                        float3 bL = base + widthDir * -baseHW;
                        float3 bR = base + widthDir * baseHW;
                        float3 tL = tip + widthDir * -tipHW;
                        float3 tR = tip + widthDir * tipHW;

                        float3 verts[6] = { bL, tL, bR, bR, tL, tR };
                        for (int i = 0; i < 6; i++)
                        {
                            g2f o;
                            float3 biased = ApplyShadowBias(verts[i], normalWS, lightDir);
                            o.pos = TransformWorldToHClip(biased);
                            stream.Append(o);
                        }
                        stream.RestartStrip();
                    }

                    float cSize = headSize * 0.09;
                    float3 cUp = stemTop + float3(0, cSize * 0.25, 0);
                    float3 cCamDir = _WorldSpaceCameraPos - cUp;
                    cCamDir.y = 0;
                    cCamDir = normalize(cCamDir);
                    float3 cRightDir = normalize(cross(float3(0, 1, 0), cCamDir));

                    for (int cq = 0; cq < 2; cq++)
                    {
                        float3 ca = (cq == 0) ? cRightDir : cCamDir;
                        float3 cBL = cUp + ca * -cSize + float3(0, -cSize * 0.5, 0);
                        float3 cBR = cUp + ca * cSize + float3(0, -cSize * 0.5, 0);
                        float3 cTL = cUp + ca * -cSize + float3(0, cSize * 0.5, 0);
                        float3 cTR = cUp + ca * cSize + float3(0, cSize * 0.5, 0);

                        float3 verts[6] = { cBL, cTL, cBR, cBR, cTL, cTR };
                        for (int i = 0; i < 6; i++)
                        {
                            g2f o;
                            float3 biased = ApplyShadowBias(verts[i], normalWS, lightDir);
                            o.pos = TransformWorldToHClip(biased);
                            stream.Append(o);
                        }
                        stream.RestartStrip();
                    }
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
