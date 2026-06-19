Shader "VoD/PSX_PostProcess"
{
    Properties
    {
        _ColorDepth       ("Color Depth (32=PSX)",  Float)      = 32
        _ScanlineStrength ("Scanline Strength",     Range(0,1)) = 0.10
        _DitherStrength   ("Dither Strength",       Range(0,1)) = 0.55
        // Chia màn hình xuống độ phân giải thấp: 4 = ~480x270, 6 = ~320x180 (PSX feel)
        _PixelDivisor     ("Pixel Divisor (res÷N)", Float)      = 4
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "PSX"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _ColorDepth;
            float _ScanlineStrength;
            float _DitherStrength;
            float _PixelDivisor;

            // Bayer 4×4 ordered dither
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

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Pixelation: snap UV xuống lưới thấp hơn
                float2 lowResSize = floor(_ScreenParams.xy / max(_PixelDivisor, 1.0));
                float2 pixelUV    = floor(input.texcoord * lowResSize) / lowResSize;

                // Point-sample tại UV đã snap → blocky pixel look
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, pixelUV);

                float2 screenPos = input.texcoord * _ScreenParams.xy;

                // Bayer dither → quantize (PSX 15-bit palette feel)
                float depth  = max(_ColorDepth, 1.0);
                float dither = (BayerDither(screenPos) - 0.5) * _DitherStrength / depth;
                col.rgb = saturate(col.rgb + dither);
                col.rgb = floor(col.rgb * depth + 0.5) / depth;

                // Scanlines nhẹ — mỗi hàng pixel thứ 2
                float scan = step(0.5, frac(screenPos.y * 0.5));
                col.rgb *= lerp(1.0 - _ScanlineStrength, 1.0, scan);

                return col;
            }
            ENDHLSL
        }
    }
}
