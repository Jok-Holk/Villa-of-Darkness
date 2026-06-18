// PSX_RendererFeature — không dùng nữa.
// Dùng Unity built-in "Full Screen Pass Renderer Feature" thay thế.
// Xem hướng dẫn: VoD → PSX Style → 2 – Select URP Renderer Data
//
// Giữ file này để Unity không báo missing script trên Renderer Data cũ.
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

public class PSX_RendererFeature : ScriptableRendererFeature
{
    public override void Create() { }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) { }

    // Render Graph stub — không làm gì, tránh warning "no implementation"
    class PSXPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph rg, ContextContainer fc) { }
    }
}
