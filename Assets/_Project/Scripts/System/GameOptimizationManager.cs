using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Gắn vào 1 GameObject (vd "OptimizationManager") để bấm chạy các tác vụ tối ưu game ngay trong Inspector —
/// chuột phải vào tiêu đề component "Game Optimization Manager" trong Inspector để thấy menu các lệnh.
/// Các field bên dưới cho phép tự custom trước khi chạy (không cố định cứng như trước).
/// Chỉ hoạt động trong Unity Editor (không chạy trong build, không cần thiết lúc runtime thật).
/// </summary>
public class GameOptimizationManager : MonoBehaviour
{
    [Header("Đánh dấu Static — cờ nào sẽ được bật")]
    public bool markOccluderStatic = true;
    public bool markOccludeeStatic = true;
    public bool markBatchingStatic = true;

    [Header("Bỏ qua object có tên bắt đầu bằng (vd object nhỏ/không cần culling)")]
    public string[] excludeNamePrefixes = { "Key_", "KeyDeco_", "Cutter_" };

    [Header("Occlusion Culling — thông số bake (Window > Rendering > Occlusion Culling)")]
    [Tooltip("Vật thể nhỏ hơn kích thước này (mét) sẽ KHÔNG được coi là occluder (vật che khuất)")]
    public float smallestOccluder = 5f;
    [Tooltip("Lỗ hổng nhỏ hơn kích thước này (mét) sẽ bị bỏ qua, coi như bề mặt kín")]
    public float smallestHole = 0.25f;
    [Tooltip("Ngưỡng % mặt sau bị coi là occluder (0-100)")]
    public float backfaceThreshold = 100f;

#if UNITY_EDITOR
    [ContextMenu("1. Đánh Dấu Static Geometry (theo cấu hình phía trên)")]
    public void MarkStaticGeometry()
    {
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int count = 0, skipped = 0;

        StaticEditorFlags flagsToAdd = 0;
        if (markOccluderStatic) flagsToAdd |= StaticEditorFlags.OccluderStatic;
        if (markOccludeeStatic) flagsToAdd |= StaticEditorFlags.OccludeeStatic;
        if (markBatchingStatic) flagsToAdd |= StaticEditorFlags.BatchingStatic;

        foreach (var r in renderers)
        {
            bool excluded = false;
            foreach (var prefix in excludeNamePrefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && r.gameObject.name.StartsWith(prefix)) { excluded = true; break; }
            }
            if (excluded) continue;
            if (r.GetComponentInParent<Canvas>() != null) continue;

            // AN TOÀN: luôn bỏ qua object có script khiến nó di chuyển/biến đổi lúc chạy, bất kể cấu hình phía trên
            // (đánh Static sai sẽ làm cửa không mở được, item không rơi/ẩn đúng, ma không đi được...).
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

            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
            flags |= flagsToAdd;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, flags);
            count++;
        }
        Debug.Log($"[GameOptimizationManager] Đã đánh dấu Static cho {count} object (cờ: {flagsToAdd}), bỏ qua {skipped} object có script tương tác/di chuyển.");
    }

    [ContextMenu("2. Bake Occlusion Culling (theo thông số phía trên)")]
    public void BakeOcclusionCulling()
    {
        StaticOcclusionCulling.smallestOccluder = smallestOccluder;
        StaticOcclusionCulling.smallestHole = smallestHole;
        StaticOcclusionCulling.backfaceThreshold = backfaceThreshold;

        Debug.Log($"[GameOptimizationManager] Bắt đầu bake Occlusion Culling (smallestOccluder={smallestOccluder}, smallestHole={smallestHole}, backfaceThreshold={backfaceThreshold})...");
        bool ok = StaticOcclusionCulling.Compute();
        if (ok)
            Debug.Log("[GameOptimizationManager] Bake Occlusion Culling xong.");
        else
            Debug.LogWarning("[GameOptimizationManager] Bake thất bại/huỷ — kiểm tra Console.");
    }

    [ContextMenu("3. Xoá Occlusion Culling Data Đã Bake")]
    public void ClearOcclusionCulling()
    {
        StaticOcclusionCulling.Clear();
        Debug.Log("[GameOptimizationManager] Đã xoá dữ liệu Occlusion Culling đã bake.");
    }
#endif
}
