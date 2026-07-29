using UnityEditor;
using UnityEngine;

// Jok báo -- "GhostAI trên ghost cube không phải, lộn object rồi. Chưa hề có cái của Thuận spawn ra bao
// giờ": kiểm tra lại TOÀN BỘ trạng thái ghost thật trong scene trước khi sửa gì -- không đoán mù nữa.
public static class VoD_ScanGhostSetup
{
    [MenuItem("VoD/Villa/Scan - Toàn Bộ Setup Ghost (GhostAI + Thuận + Glimpse)")]
    public static void Scan()
    {
        Debug.Log("[VoD][ScanGhostSetup] === 1) Mọi GhostAI trong scene ===");
        var ghosts = Resources.FindObjectsOfTypeAll<GhostAI>();
        int ghostCount = 0;
        foreach (var g in ghosts)
        {
            if (!g.gameObject.scene.IsValid()) continue;
            ghostCount++;
            var anim = g.GetComponent<Animator>();
            string ctrlName = anim != null && anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "(không có/không gán)";
            var smr = g.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var mr = g.GetComponentsInChildren<MeshRenderer>(true);
            Debug.Log($"  '{GetPath(g.transform)}' -- active={g.gameObject.activeInHierarchy} -- Animator Controller='{ctrlName}' " +
                      $"-- SkinnedMeshRenderer con={smr.Length} -- MeshRenderer con={mr.Length}");
            foreach (var r in smr) Debug.Log($"    SkinnedMesh: '{GetPath(r.transform)}' mesh='{(r.sharedMesh != null ? r.sharedMesh.name : "NULL")}'");
            foreach (var r in mr)
            {
                var mf = r.GetComponent<MeshFilter>();
                Debug.Log($"    MeshRenderer: '{GetPath(r.transform)}' mesh='{(mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "NULL")}'");
            }
        }
        if (ghostCount == 0) Debug.LogWarning("  KHÔNG có GhostAI nào trong scene đang mở.");

        Debug.Log("[VoD][ScanGhostSetup] === 2) 'Ghost_Monster_Thuan' (đã tạo lúc trước cho DiaryReactionCutsceneTrigger) ===");
        var thuanGO = FindByNameIncludingInactive("Ghost_Monster_Thuan");
        if (thuanGO == null)
        {
            Debug.LogWarning("  KHÔNG tồn tại trong scene -- tool VoD_FixGhostGlideModel trước đó có thể chưa từng chạy thành công, hoặc đã bị xoá.");
        }
        else
        {
            var anim = thuanGO.GetComponentInChildren<Animator>(true);
            string ctrlName = anim != null && anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "(không có/không gán)";
            Debug.Log($"  '{GetPath(thuanGO.transform)}' -- active={thuanGO.activeInHierarchy} -- Animator Controller='{ctrlName}'");
        }

        Debug.Log("[VoD][ScanGhostSetup] === 3) DiaryReactionCutsceneTrigger._ghost đang trỏ đâu ===");
        var triggers = Resources.FindObjectsOfTypeAll<DiaryReactionCutsceneTrigger>();
        foreach (var trigger in triggers)
        {
            if (!trigger.gameObject.scene.IsValid()) continue;
            var so = new SerializedObject(trigger);
            var ghostProp = so.FindProperty("_ghost");
            var prefabProp = so.FindProperty("_ghostPrefab");
            var ghostT = ghostProp != null ? ghostProp.objectReferenceValue as Transform : null;
            var prefab = prefabProp != null ? prefabProp.objectReferenceValue as GameObject : null;
            Debug.Log($"  '{GetPath(trigger.transform)}'._ghost -> '{(ghostT != null ? GetPath(ghostT) : "TRỐNG (null)")}' " +
                      $"-- _ghostPrefab='{(prefab != null ? prefab.name : "TRỐNG")}'");

            // SỬA (Jok yêu cầu "tự add prefab bằng code"): Awake() chỉ TỰ SPAWN khi _ghost ĐANG TRỐNG -- nếu
            // _ghost đã có sẵn 1 reference CŨ (từ tool trước đây), auto-spawn sẽ bị bỏ qua hoàn toàn.
            if (ghostT != null && prefab != null)
                Debug.LogWarning("    _ghost ĐANG CÓ giá trị sẵn -- auto-spawn từ _ghostPrefab sẽ KHÔNG chạy " +
                                  "(bị bỏ qua). Nếu muốn dùng auto-spawn mới, xoá reference _ghost hiện tại (kéo về None) trong Inspector.");
        }

        Debug.Log("[VoD][ScanGhostSetup] === 4) 'GhostCube' (nghi placeholder cũ) ===");
        var cubeGO = FindByNameIncludingInactive("GhostCube");
        if (cubeGO == null)
        {
            Debug.Log("  Không có object tên 'GhostCube' trong scene.");
        }
        else
        {
            var ghostAIOnCube = cubeGO.GetComponent<GhostAI>();
            Debug.Log($"  '{GetPath(cubeGO.transform)}' -- active={cubeGO.activeInHierarchy} -- có GhostAI={(ghostAIOnCube != null)}");
        }

        Debug.Log("[VoD][ScanGhostSetup] === 5) Prefab 'Thuan.fbx' có tồn tại đúng đường dẫn không ===");
        var thuanAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Animations/Monster/Animation/Thuan.fbx");
        Debug.Log(thuanAsset != null
            ? $"  OK -- load được, tên asset='{thuanAsset.name}'"
            : "  LỖI -- không load được model tại đường dẫn dự kiến.");

        Debug.Log("[VoD][ScanGhostSetup] XONG.");
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    private static GameObject FindByNameIncludingInactive(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in all)
        {
            if (t.hideFlags != HideFlags.None) continue;
            if (t.name != name) continue;
            if (!t.gameObject.scene.IsValid()) continue;
            return t.gameObject;
        }
        return null;
    }
}
