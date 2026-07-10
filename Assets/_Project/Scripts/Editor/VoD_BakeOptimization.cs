using UnityEngine;
using UnityEditor;

public static class VoD_BakeOptimization
{
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
