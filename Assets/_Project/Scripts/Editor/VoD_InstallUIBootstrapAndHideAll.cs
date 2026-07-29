using UnityEditor;
using UnityEngine;

// Gắn UIBootstrap.cs vào GameManager (chạy đầu tiên, tự bật lại các UI cần thiết lúc vào game thật) rồi
// TẮT SẠCH các object UI đó trong scene để Hierarchy gọn -- an toàn vì UIBootstrap đã lo phần "tự bật lại
// đúng lúc", tắt ở đây chỉ ảnh hưởng lúc Edit Mode xem Hierarchy, không ảnh hưởng gameplay thật nữa.
public static class VoD_InstallUIBootstrapAndHideAll
{
    private static readonly string[] Roots =
    {
        "ScreenFader_Canvas",
        "ExamineStageUI",
        "HudMetersUI",
        "TutorialHintUI",
        "InteractPrompt",
        "DiaryReaderPanel",
    };

    [MenuItem("VoD/Villa/Fix - Gắn UIBootstrap + Tắt Sạch UI Trong Editor")]
    public static void Run()
    {
        GameObject gameManager = FindByNameIncludingInactive("GameManager");
        if (gameManager == null)
        {
            Debug.LogError("[VoD][Bootstrap] Không tìm thấy 'GameManager' trong scene -- cần có object này để gắn UIBootstrap.");
            return;
        }

        if (gameManager.GetComponent<UIBootstrap>() == null)
        {
            gameManager.AddComponent<UIBootstrap>();
            Debug.Log("[VoD][Bootstrap] Đã gắn UIBootstrap vào GameManager.");
        }
        else
        {
            Debug.Log("[VoD][Bootstrap] GameManager đã có UIBootstrap sẵn -- bỏ qua.");
        }

        int hidden = 0;
        foreach (string name in Roots)
        {
            GameObject go = FindByNameIncludingInactive(name);
            if (go == null) { Debug.LogWarning($"[VoD][Bootstrap] Không tìm thấy '{name}'."); continue; }
            if (go.activeSelf)
            {
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                hidden++;
            }
        }

        Debug.Log($"[VoD][Bootstrap] XONG -- đã tắt {hidden} object UI trong scene (Hierarchy gọn hơn), UIBootstrap sẽ tự bật lại đúng lúc Play thật. Nhớ Ctrl+S lưu scene.");
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
