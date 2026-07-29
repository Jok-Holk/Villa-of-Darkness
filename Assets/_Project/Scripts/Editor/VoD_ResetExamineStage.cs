using UnityEditor;
using UnityEngine;

// Sửa code trong ExamineStageUI.cs/HudMetersUI.cs/TutorialHintUI.cs KHÔNG tự áp vào object đã có sẵn
// trong scene -- OnEnable()/BuildStage()/BuildUI() đều có dòng chặn "đã có rồi thì bỏ qua" để tránh dựng
// chồng, nhưng đồng nghĩa code sửa sau không tự cập nhật vào object cũ. Xoá + dựng lại sạch cả 3 để áp
// đúng code mới nhất mỗi lần cần -- dùng lại được nhiều lần, không phải tool 1 lần rồi xoá.
public static class VoD_ResetExamineStage
{
    [MenuItem("VoD/Villa/Fix - Dựng Lại Examine+HUD+Tutorial Hint (áp code mới nhất)")]
    public static void Reset()
    {
        ResetOne<ExamineStageUI>("ExamineStageUI");
        ResetOne<HudMetersUI>("HudMetersUI");
        ResetOne<TutorialHintUI>("TutorialHintUI");
        Debug.Log("[VoD][ResetUI] XONG -- cả 3 đã dựng lại từ code hiện tại. Nhớ Ctrl+S lưu scene.");
    }

    private static void ResetOne<T>(string name) where T : Component
    {
        GameObject old = FindByNameIncludingInactive(name);
        if (old != null)
        {
            Object.DestroyImmediate(old);
            Debug.Log($"[VoD][ResetUI] Đã xoá '{name}' cũ.");
        }

        var go = new GameObject(name);
        go.AddComponent<T>();
        Debug.Log($"[VoD][ResetUI] Đã dựng lại '{name}' mới.");
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
