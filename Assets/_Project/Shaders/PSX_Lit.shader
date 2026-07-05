// PSX_Lit — shader "PSX thật" áp trực tiếp lên từng vật thể (không phải post-process toàn màn hình).
// Nhận ánh sáng URP chuẩn (main light + đèn pin/đèn phòng là additional light) NÊN không còn xung đột
// với đèn pin như PSX_PostProcess (camera-level) trước đây.
// Hiệu ứng PSX: (1) rung đỉnh vertex trong clip-space (giả affine/thiếu độ chính xác float của PS1),
// (2) lượng tử hoá màu theo Bayer dither NGAY TRONG shader vật thể (không phải sau khi mọi thứ đã lên hình).
Shader "VoD/PSX_Lit"
{
    Properties
    {
        _BaseMap  ("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _ColorDepth      ("Color Depth (số mức màu mỗi kênh)", Range(4,64)) = 32
        _DitherStrength  ("Dither Strength", Range(0,1)) = 0.15
        _VertexSnap      ("Vertex Snap (độ rung PS1, đơn vị grid trong clip-space)", Range(0,32)) = 10
        _Smoothness      ("Smoothness", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord: TEXCOORD3;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            half4  _BaseColor;
            float  _ColorDepth;
            float  _DitherStrength;
            float  _VertexSnap;
            float  _Smoothness;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);

                // Rung đỉnh kiểu PS1: snap toạ độ clip-space (sau chia w) xuống lưới thô —
                // tạo hiệu ứng "vertex jitter" đặc trưng khi camera/vật thể di chuyển.
                float4 clipPos = vpi.positionCS;
                if (_VertexSnap > 0.5)
                {
                    float snapScale = _VertexSnap;
                    clipPos.xyz = floor(clipPos.xyz / clipPos.w * snapScale) / snapScale * clipPos.w;
                }
                OUT.positionCS = clipPos;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = vpi.positionWS;
                OUT.shadowCoord = GetShadowCoord(vpi);
                return OUT;
            }

            float BayerDither(float2 screenPos)
            {
                int2 p = int2(screenPos) & 3;
                float4 r0 = float4( 0, 8, 2,10) / 16.0;
                float4 r1 = float4(12, 4,14, 6) / 16.0;
                float4 r2 = float4( 3,11, 1, 9) / 16.0;
                float4 r3 = float4(15, 7,13, 5) / 16.0;
                float4 rows[4] = { r0, r1, r2, r3 };
                float4 cm = float4(p.x==0, p.x==1, p.x==2, p.x==3);
                return dot(rows[p.y], cm);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 normalWS = normalize(IN.normalWS);

                // Tính sáng ĐƠN GIẢN thủ công (Lambert diffuse) thay vì UniversalFragmentPBR —
                // hàm PBR cấp thấp cần điền rất nhiều field InputData/SurfaceData mới đúng
                // (bakedGI, normalizedScreenSpaceUV, fogCoord...), thiếu 1 field dễ ra kết quả đen/sai.
                // Cách này cộng trực tiếp main light + mọi additional light (đèn pin/đèn phòng),
                // ít khả năng thiếu sót hơn nhiều.
                float3 lighting = float3(0, 0, 0);

                Light mainLight = GetMainLight(IN.shadowCoord);
                lighting += mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation
                            * saturate(dot(normalWS, mainLight.direction));

                // Ánh sáng môi trường tối thiểu để không có mặt nào đen tuyệt đối
                lighting += 0.08;

#if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; lightIndex++)
                {
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    lighting += light.color * light.distanceAttenuation * light.shadowAttenuation
                                * saturate(dot(normalWS, light.direction));
                }
#endif

                half4 color = half4(albedo.rgb * lighting, albedo.a);

                // Lượng tử hoá màu ngay trên vật thể (không phải sau khi lên hình toàn màn hình) —
                // giữ đúng độ sáng thật của đèn pin, chỉ làm "vỡ khối màu" kiểu PSX.
                float2 screenPos = IN.positionCS.xy;
                float depth = max(_ColorDepth, 1.0);
                float dither = (BayerDither(screenPos) - 0.5) * _DitherStrength / depth;
                color.rgb = saturate(color.rgb + dither);
                color.rgb = floor(color.rgb * depth + 0.5) / depth;

                return color;
            }
            ENDHLSL
        }

        // Pass đổ bóng — bắt buộc để vật thể vẫn nhận/đổ bóng bình thường
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };
            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            ShadowVaryings ShadowVert(ShadowAttributes IN)
            {
                ShadowVaryings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
#if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif
                OUT.positionCS = positionCS;
                return OUT;
            }

            half4 ShadowFrag(ShadowVaryings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
