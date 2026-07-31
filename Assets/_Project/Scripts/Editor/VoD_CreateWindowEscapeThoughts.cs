using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Jok mô tả (2026-08-01): thiếu suy nghĩ dẫn dắt "phải ra qua cửa sổ lúc vào -> cửa sổ hết dùng được ->
// phải ra sân sau". Đây là SUY NGHĨ (chữ nổi, KHÔNG thu giọng theo đúng quy ước story bible) nên viết nội
// dung được luôn, không cần chờ voice thật như MK-HIDE/MK-DEATH. Tool tạo 2 DialogueAsset + 1 object phát
// suy nghĩ 1 (wire vào OnReveal của 4 HideDoor) + 1 object phát suy nghĩ 2 (đặt cạnh WindowEntryTrigger).
// TOOL DÙNG 1 LẦN RỒI XOÁ.
public static class VoD_CreateWindowEscapeThoughts
{
    private const string AssetFolder = "Assets/_Project/Data/Dialogue/Chapter1/";

    [MenuItem("VoD/Villa/ONE-TIME - Tạo Suy Nghĩ Thoát Qua Cửa Sổ")]
    public static void Run()
    {
        if (!AssetDatabase.IsValidFolder(AssetFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Dialogue"))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Dialogue");
            AssetDatabase.CreateFolder("Assets/_Project/Data/Dialogue", "Chapter1");
        }

        var thought1 = CreateThoughtAsset("DialogueAsset_Ch1_Thought_LeaveViaWindow",
            "Phải ra khỏi đây thôi. Đúng rồi — cửa sổ phòng ăn, chỗ mình trèo vào lúc nãy!");

        var thought2 = CreateThoughtAsset("DialogueAsset_Ch1_Thought_WindowBlocked",
            "Cửa sổ này... không ra được nữa rồi. Phải tìm đường khác thôi. Ra sân sau thử xem.");

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // 1) Object phát suy nghĩ 1 -- Jok/nhóm tự wire Play() vào OnReveal của TỪNG HideDoor (4 cái) trong Inspector.
        GameObject playerCall = GameObject.Find("ThoughtOnHideExit");
        if (playerCall == null)
        {
            playerCall = new GameObject("ThoughtOnHideExit");
            Undo.RegisterCreatedObjectUndo(playerCall, "Create thought caller");
        }
        var caller = playerCall.GetComponent<PlayDialogueOnCall>();
        if (caller == null) caller = playerCall.AddComponent<PlayDialogueOnCall>();
        var soCaller = new SerializedObject(caller);
        soCaller.FindProperty("_dialogue").objectReferenceValue = thought1;
        soCaller.ApplyModifiedProperties();

        // 2) Object phát suy nghĩ 2 -- đặt cạnh WindowEntryTrigger nếu tìm thấy, không thì đặt tạm ở gốc toạ độ.
        var windowTrigger = Object.FindFirstObjectByType<WindowEntryTrigger>(FindObjectsInactive.Include);
        Vector3 pos = windowTrigger != null ? windowTrigger.transform.position : Vector3.zero;

        GameObject thoughtGO = GameObject.Find("Trigger_WindowBlockedThought");
        if (thoughtGO == null)
        {
            thoughtGO = new GameObject("Trigger_WindowBlockedThought");
            Undo.RegisterCreatedObjectUndo(thoughtGO, "Create thought trigger");
            var col = thoughtGO.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2f, 2f, 2f);
        }
        thoughtGO.transform.position = pos;
        var thoughtTrigger = thoughtGO.GetComponent<ThoughtTrigger>();
        if (thoughtTrigger == null) thoughtTrigger = thoughtGO.AddComponent<ThoughtTrigger>();
        var soThought = new SerializedObject(thoughtTrigger);
        soThought.FindProperty("_thought").objectReferenceValue = thought2;
        soThought.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);

        string windowNote = windowTrigger != null
            ? $"Đã đặt đúng vị trí '{windowTrigger.gameObject.name}'."
            : "KHÔNG tìm thấy WindowEntryTrigger trong scene -- đang đứng tạm ở gốc toạ độ (0,0,0), Jok tự kéo tới đúng chỗ cửa sổ.";

        Debug.Log("[VoD] XONG. 2 DialogueAsset đã tạo sẵn nội dung (không cần voice, chữ nổi màn hình):\n" +
                   "- 'ThoughtOnHideExit' (object mới) -- Jok tự wire Play() vào OnReveal của cả 4 HideDoor trong Inspector.\n" +
                   $"- 'Trigger_WindowBlockedThought' -- {windowNote}");
    }

    private static DialogueAsset CreateThoughtAsset(string name, string text)
    {
        string path = AssetFolder + name + ".asset";
        var asset = AssetDatabase.LoadAssetAtPath<DialogueAsset>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueAsset>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.lines.Clear();
        asset.lines.Add(new DialogueLine { speakerName = "Khoa (suy nghĩ)", text = text, hasVoice = false, voiceClip = null });
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return asset;
    }
}
