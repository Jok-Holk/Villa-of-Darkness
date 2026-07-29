Shader "VoD/MirrorBrightReflection"
{
    // Gương hiện "1 thực tại khác" sáng hơn hẳn thế giới thật -- theo yêu cầu Jok: thay vì cố chiếu sáng
    // đúng vật lý (chỉ sáng khi đèn pin thật soi vào), gương này CỐ Ý hiện căn phòng sáng rõ độc lập với
    // ánh sáng gameplay thật -- vừa dễ thấy nhân vật trong gương, vừa tạo cảm giác "bất thường" (gương cho
    // thấy 1 phiên bản sáng đèn của căn phòng lẽ ra đang tối) -- đúng chất horror.
    //
    // KHÔNG dùng cách chiếu sáng thật (thêm Light/Rendering Layer) vì URP không hỗ trợ "ánh sáng chỉ ảnh
    // hưởng 1 camera cụ thể" mà không đụng tới TOÀN BỘ vật thể trong scene. Thay vào đó XỬ LÝ NGAY TRÊN ẢNH
    // ĐÃ CHỤP (RenderTexture) bằng phép "nhân sáng + cộng thêm sàn sáng tối thiểu" -- hoạt động đúng ngay cả
    // khi vùng nguồn là ĐEN TUYỆT ĐỐI (phép nhân thường KHÔNG làm được điều này vì 0 × bất kỳ số nào vẫn = 0).
    //
    // THAY 2026-07-27: Bỏ hẳn cách bo 2 góc trên bằng công thức hình tròn đoán bán kính -- khung kính thật
    // là 1 hình VÒM CONG LIÊN TỤC, không phải chữ nhật + 2 góc bo nhỏ, nên công thức cũ dù chỉnh số bao
    // nhiêu cũng chỉ xấp xỉ, không khớp thật. Giờ dùng 1 TEXTURE MASK bake thẳng từ hình dạng THẬT của mặt
    // kính trong mesh gốc (VoD_BuildMirrorGlassMask.cs) -- trắng = hiện phản chiếu, đen = cắt bỏ (lộ khung
    // gốc thật phía sau) -- khớp chính xác 100% vì lấy từ đúng hình học gốc, không đoán số.
    Properties
    {
        _MainTex        ("Render Texture (ảnh phản chiếu thật)", 2D) = "white" {}
        _GlassMask      ("Mask hình kính thật (trắng=hiện, đen=cắt) -- bake tự động, không chỉnh tay", 2D) = "white" {}
        _Brightness     ("Hệ số nhân sáng", Float) = 2.2
        _AmbientLift    ("Sàn sáng tối thiểu (cộng thêm, cả vùng đen tuyệt đối cũng sáng lên)", Color) = (0.32, 0.3, 0.36, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" }
        // Cull Off -- Quad phản chiếu có thể quay mặt bất kỳ hướng nào tuỳ trục local của model gương gốc,
        // không cần đoán đúng hướng, luôn hiện được từ phía phòng bất kể lệch trục.
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_GlassMask);
            SAMPLER(sampler_GlassMask);
            float4 _MainTex_ST;
            float  _Brightness;
            float4 _AmbientLift;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                // Mask bake sẵn từ hình dạng THẬT của mặt kính (xem VoD_BuildMirrorGlassMask.cs) -- kênh R
                // > 0.5 = thuộc mặt kính (hiện phản chiếu), ngược lại cắt bỏ (lộ khung/kính gốc thật phía sau).
                half maskR = SAMPLE_TEXTURE2D(_GlassMask, sampler_GlassMask, IN.uv).r;
                clip(maskR - 0.5);

                half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 result = src.rgb * _Brightness + _AmbientLift.rgb;
                return half4(saturate(result), 1.0);
            }
            ENDHLSL
        }
    }
}
