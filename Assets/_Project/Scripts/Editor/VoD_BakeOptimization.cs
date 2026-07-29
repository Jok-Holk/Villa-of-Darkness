using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class VoD_BakeOptimization
{
    // Tắt Cast Shadows cho các prop nhỏ (dưới ngưỡng kích thước) -- đây là chi
    // phí render thật sự (Shadow casters: 1111 trong Stats) mà Occlusion
    // Culling không giúp được cho góc nhìn ngoài trời. Object nhỏ (gai cổng,
    // chi tiết hàng rào, đá trang trí...) đổ bóng gần như không ai để ý,
    // nhưng vẫn tốn 1 draw call/object trong shadow pass.
    [MenuItem("VoD/Optimize/Disable Shadows On Small Props")]
    public static void DisableShadowsOnSmallProps()
    {
        const float MaxDimensionThreshold = 0.6f; // object có cạnh lớn nhất < 0.6m mới bị tắt shadow

        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int disabled = 0, skipped = 0;

        foreach (var r in renderers)
        {
            if (r.gameObject.name.StartsWith("Key_") || r.gameObject.name.StartsWith("KeyDeco_") || r.gameObject.name.StartsWith("Cutter_"))
                continue;
            if (r.GetComponentInParent<Canvas>() != null)
                continue;
            if (r.shadowCastingMode == ShadowCastingMode.Off)
                continue; // đã tắt sẵn rồi, khỏi tính vào số liệu

            // AN TOÀN: không đụng object có script tương tác/di chuyển -- lý do giống hệt MarkStaticGeometry.
            if (r.GetComponent<Animator>() != null ||
                r.GetComponent<Rigidbody>() != null ||
                r.GetComponentInParent<UnityEngine.AI.NavMeshAgent>() != null ||
                r.GetComponent<DoorController>() != null || r.GetComponentInParent<DoorController>() != null ||
                r.GetComponent<PickupItem>() != null || r.GetComponentInParent<PickupItem>() != null ||
                r.GetComponent<ExamineItem>() != null || r.GetComponentInParent<ExamineItem>() != null ||
                r.GetComponentInParent<PianoInteractable>() != null ||
                r.GetComponentInParent<GhostAI>() != null)
            {
                skipped++;
                continue;
            }

            var size = r.bounds.size;
            float maxDim = Mathf.Max(size.x, size.y, size.z);
            if (maxDim < MaxDimensionThreshold)
            {
                Undo.RecordObject(r, "VoD Disable Shadow On Small Prop");
                r.shadowCastingMode = ShadowCastingMode.Off;
                EditorUtility.SetDirty(r);
                disabled++;
            }
        }

        Debug.Log($"[VoD] Đã tắt Cast Shadows cho {disabled} prop nhỏ (cạnh lớn nhất < {MaxDimensionThreshold}m), bỏ qua {skipped} object có script tương tác. Không đụng tường/sàn/vật lớn -- vẫn giữ bóng đổ cho mấy thứ quan trọng nhìn thấy rõ. Nhớ Ctrl+S.");
    }

    // Giảm Shadow Distance của URP Asset -- villa dùng fog/horror lighting nên
    // tầm nhìn xa vốn đã bị fog che mờ, đổ bóng cho vật ở xa hơn tầm fog nhìn
    // thấy được là lãng phí render thuần tuý. Đây là 1 thay đổi toàn cục, tác
    // động mạnh hơn hẳn việc tắt shadow từng object lẻ.
    [MenuItem("VoD/Optimize/Reduce URP Shadow Distance To 40m")]
    public static void ReduceShadowDistance()
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("[VoD] Không tìm thấy Universal Render Pipeline Asset đang active trong Graphics Settings.");
            return;
        }

        float old = urpAsset.shadowDistance;
        Undo.RecordObject(urpAsset, "VoD Reduce Shadow Distance");
        urpAsset.shadowDistance = 40f;
        EditorUtility.SetDirty(urpAsset);

        Debug.Log($"[VoD] Đã giảm Shadow Distance từ {old}m xuống 40m trên '{urpAsset.name}'. Nếu fog/tầm nhìn trong game xa hơn 40m thì báo mình chỉnh lại số cho khớp. Nhớ Ctrl+S (Project Settings sẽ tự lưu khi save Asset).");
    }
    [MenuItem("VoD/Optimize/Bake Occlusion Culling")]
    public static void BakeOcclusionCulling()
    {
        Debug.Log("[VoD] Bắt đầu bake Occlusion Culling (có thể mất vài phút với scene lớn)...");
        bool ok = StaticOcclusionCulling.Compute();
        if (ok)
            Debug.Log("[VoD] Bake Occlusion Culling xong.");
        else
            Debug.LogWarning("[VoD] Occlusion Culling bake thất bại hoặc bị huỷ — kiểm tra Console để biết lý do (thường do chưa có object nào đánh dấu Occluder/Occludee Static, hoặc scene quá nhỏ để cần culling).");
    }

    [MenuItem("VoD/Optimize/Mark All Static Geometry (Occlusion)")]
    public static void MarkStaticGeometry()
    {
        // Đánh dấu Navigation Static + Occluder/Occludee Static cho toàn bộ MeshRenderer trong scene
        // (bỏ qua phím đàn KeyDeco_/Key_ và Cutter_ vì quá nhỏ/không cần culling, và object thuộc UI Canvas).
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int count = 0;

        int skipped = 0;
        foreach (var r in renderers)
        {
            if (r.gameObject.name.StartsWith("Key_") || r.gameObject.name.StartsWith("KeyDeco_") || r.gameObject.name.StartsWith("Cutter_"))
                continue;
            if (r.GetComponentInParent<Canvas>() != null)
                continue;

            // AN TOÀN: bỏ qua bất kỳ object nào có script khiến nó di chuyển/biến đổi lúc chạy
            // (đánh dấu Static sai sẽ làm object đó "đứng hình" — cửa không mở được, item không rơi/ẩn đúng, ma không đi được...).
            if (r.GetComponent<Animator>() != null ||
                r.GetComponent<Rigidbody>() != null ||
                r.GetComponentInParent<UnityEngine.AI.NavMeshAgent>() != null ||
                r.GetComponent<DoorController>() != null || r.GetComponentInParent<DoorController>() != null ||
                r.GetComponent<PickupItem>() != null || r.GetComponentInParent<PickupItem>() != null ||
                r.GetComponent<ExamineItem>() != null || r.GetComponentInParent<ExamineItem>() != null ||
                r.GetComponentInParent<PianoInteractable>() != null ||   // toàn bộ cụm piano — có phím nhún động
                r.GetComponentInParent<GhostAI>() != null)
            {
                skipped++;
                continue;
            }

            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
            flags |= StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, flags);
            count++;
        }

        Debug.Log($"[VoD] Đã đánh dấu Static cho {count} object (Occluder + Occludee + Batching), bỏ qua {skipped} object có script tương tác/di chuyển để không làm hỏng gameplay. " +
                  "Giờ chạy 'VoD → Optimize → Bake Occlusion Culling'. NavMesh bake làm thủ công qua Window > AI > Navigation.");
    }
}
