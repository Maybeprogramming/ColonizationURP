Shader "Custom/Water_v2"
{
    Properties
    {
        [Header(Waves)]
        _WaveStrength ("Wave Strength", Range(0, 3)) = 0.6
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveTiling ("Wave Tiling", Float) = 3.0
        _WaveRoughness ("Wave Roughness", Range(0, 1)) = 0.4

        [Header(Color)]
        _ColorShallow ("Shallow Color", Color) = (0.05, 0.45, 0.35, 1)
        _ColorDeep ("Deep Color", Color) = (0.0, 0.08, 0.15, 1)
        _DepthMaxDistance ("Depth Max Distance", Float) = 8.0
        _WaterOpacity ("Water Opacity", Range(0, 1)) = 0.7
        _ShoreSmoothness ("Shore Smoothness", Range(0, 1)) = 0.3

        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 0.9)
        _FoamDistance ("Foam Distance", Float) = 2.0
        _FoamIntensity ("Foam Intensity", Range(0, 2)) = 1.0
        _FoamNoiseScale ("Foam Noise Scale", Float) = 4.0

        [Header(Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0, 0.1)) = 0.02

        [Header(Specular)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
        _SpecularIntensity ("Specular Intensity", Range(0, 5)) = 1.5

        [Header(Fresnel)]
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 2.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-10" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            float _WaveStrength;
            float _WaveSpeed;
            float _WaveTiling;
            float _WaveRoughness;
            float4 _ColorShallow;
            float4 _ColorDeep;
            float _DepthMaxDistance;
            float _WaterOpacity;
            float _ShoreSmoothness;
            float4 _FoamColor;
            float _FoamDistance;
            float _FoamIntensity;
            float _FoamNoiseScale;
            float _RefractionStrength;
            float _Smoothness;
            float _SpecularIntensity;
            float _FresnelPower;
            float _FresnelIntensity;

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            void CalcWave(float2 dir, float freq, float amp, float speed, float steep,
                float2 uv, float time, inout float3 slope, inout float height)
            {
                float theta = dot(dir, uv) * freq + time * speed;
                float s = sin(theta);
                float c = cos(theta);
                float qa = amp * steep;

                slope.x += dir.x * freq * amp * c;
                slope.z += dir.y * freq * amp * c;
                slope.y += qa * freq * s;
                height += amp * s;
            }

            void SampleWaves(float2 uv, float time, out float3 normal, out float height)
            {
                float3 slope = 0;
                height = 0;

                float t = _WaveTiling;
                float a = 1.0;
                float spd = _WaveSpeed;
                float str = _WaveStrength;
                float rough = lerp(0.15, 0.5, _WaveRoughness);

                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 1.5708 + 0.3;
                    float2 dir = float2(cos(angle), sin(angle));
                    float freq = t * (0.5 + i * 0.25);
                    float amp2 = a / max(0.01, 1.0 + freq * 0.3);
                    float spd2 = spd * (0.7 + i * 0.15);

                    CalcWave(dir, freq, amp2, spd2, rough, uv, time, slope, height);

                    t *= 1.4;
                    a *= 0.55;
                }

                t = _WaveTiling * 2.5;
                a = 0.3;
                [unroll]
                for (int j = 0; j < 4; j++)
                {
                    float angle = j * 0.7854 + 1.2;
                    float2 dir = float2(cos(angle), sin(angle));
                    float freq = t * (0.8 + j * 0.3);
                    float amp2 = a / max(0.01, 1.0 + freq * 0.2);
                    float spd2 = spd * (1.2 + j * 0.25);

                    CalcWave(dir, freq, amp2, spd2, rough * 0.6, uv, time, slope, height);

                    t *= 1.3;
                    a *= 0.5;
                }

                normal = normalize(float3(-slope.x, 1.0, -slope.z));
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                float rawDepth = SampleSceneDepth(screenUV);
                float sceneLinear = LinearEyeDepth(rawDepth, _ZBufferParams);
                float waterLinear = LinearEyeDepth(i.pos.z, _ZBufferParams);
                float depthDiff = max(sceneLinear - waterLinear, 0);

                float3 normalWS;
                float waveHeight;
                SampleWaves(i.uv * _WaveTiling, _Time.y, normalWS, waveHeight);

                float3 viewDir = normalize(GetCameraPositionWS() - i.worldPos);
                float NdotV = saturate(dot(normalWS, viewDir));
                float fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelIntensity;

                float2 refraction = normalWS.xz * depthDiff * _RefractionStrength;
                float2 refractedUV = screenUV + refraction;

                float3 refractedColor = SampleSceneColor(refractedUV);

                float depthFactor = saturate(depthDiff / _DepthMaxDistance);
                float3 waterColor = lerp(refractedColor * _ColorShallow.rgb, _ColorDeep.rgb, depthFactor * _WaterOpacity);

                Light mainLight = GetMainLight();
                float3 halfVec = normalize(mainLight.direction + viewDir);
                float NdotH = saturate(dot(normalWS, halfVec));
                float specular = pow(NdotH, _Smoothness * 128.0) * _SpecularIntensity;
                waterColor += specular * mainLight.color;
                waterColor += fresnel;

                float2 foamUV = i.uv * _FoamNoiseScale;
                float foamNoise = 0.5 + 0.5 * sin(foamUV.x * 10.0 + foamUV.y * 8.0 + _Time.y * 0.5);
                float foamLine = 1.0 - smoothstep(0, _FoamDistance, depthDiff);
                float foam = foamLine * foamNoise * _FoamIntensity;
                waterColor = lerp(waterColor, _FoamColor.rgb, foam * _FoamColor.a);

                float shoreBlend = 1.0 - smoothstep(0, _ShoreSmoothness, depthDiff * 2.0);
                float alpha = lerp(0.85, 1.0, shoreBlend);

                return half4(waterColor, alpha);
            }
            ENDHLSL
        }
    }
}