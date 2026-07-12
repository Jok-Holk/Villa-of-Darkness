Shader "VoD/FakeVolumetricCone"
{
    Properties
    {
        _Color("Color", Color) = (1, 0.95, 0.8, 0.35)
        _Intensity("Intensity", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
            };

            float4 _Color;
            float  _Intensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Vertex alpha bake sẵn trong mesh: 1 ở đỉnh nón (gần nguồn sáng) -> 0 ở đáy (xa nguồn) —
                // Cull Off + additive blend khiến 2 mặt của nón chồng lên nhau, tự nhiên đậm ở giữa,
                // mỏng dần ở viền — không cần tính toán radial phức tạp trong shader.
                float4 c = _Color * _Intensity;
                c.a *= IN.color.a;
                return c;
            }
            ENDHLSL
        }
    }
}
