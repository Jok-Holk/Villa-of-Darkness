using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// Gắn chuỗi ĐÚNG THỨ TỰ Jok chỉnh lại: [thoại tò mò "thứ gì đây?"] -> [lên dây + nghe băng] -> [soi 360 độ
// hộp âm nhạc] -> [nhặt] -> [lưu checkpoint cảnh 3 tại 1 spawn point CỐ ĐỊNH, KHÔNG phải vị trí Player lúc
// đó -- tránh lưu ngay chỗ nguy hiểm nếu đang bị ma đuổi]. Không hand-viết YAML DialogueAsset (đã có bug
// thật từ trước với DialogueAsset_Ch1_Intro) -- tạo qua CreateInstance + CreateAsset trong code.
public static class VoD_SetupMusicBoxSequence
{
    private const string DialogueAssetPath = "Assets/_Project/Data/Dialogue/Chapter1/DialogueAsset_HopAmNhac_ToMo.asset";

    [MenuItem("VoD/Villa/Fix - Gắn Chuỗi Hộp Âm Nhạc (thoại + Examine + checkpoint)")]
    public static void Setup()
    {
        var musicBox = Object.FindFirstObjectByType<MusicBoxInteractable>(FindObjectsInactive.Include);
        if (musicBox == null) { Debug.LogError("[VoD][MusicBox] Không tìm thấy MusicBoxInteractable trong scene."); return; }

        var musicBoxSO = new SerializedObject(musicBox);
        var pickupProp = musicBoxSO.FindProperty("_pickupItemAfterListening");
        var pickup = pickupProp?.objectReferenceValue as PickupItem;
        if (pickup == null) { Debug.LogError("[VoD][MusicBox] MusicBoxInteractable chưa gán _pickupItemAfterListening."); return; }

        // Thoại tò mò -- gắn vào MusicBoxInteractable._curiousThought, chạy TRƯỚC khi lên dây/nghe băng.
        DialogueAsset thought = GetOrCreateThoughtDialogue();
        var curiousProp = musicBoxSO.FindProperty("_curiousThought");
        curiousProp.objectReferenceValue = thought;
        musicBoxSO.ApplyModifiedProperties();

        GameObject pickupGO = pickup.gameObject;

        var examineItem = pickupGO.GetComponent<ExamineItem>();
        bool examineWasMissing = examineItem == null;
        if (examineItem == null) examineItem = Undo.AddComponent<ExamineItem>(pickupGO);

        // Sau khi nghe băng xong -> thẳng vào Examine, KHÔNG có thoại lần 2 -- thoại tò mò đã chạy ở đầu rồi.
        var pickupSO = new SerializedObject(pickup);
        pickupSO.FindProperty("_requireExamineFirst").boolValue = true;
        pickupSO.FindProperty("_examineItem").objectReferenceValue = examineItem;
        pickupSO.ApplyModifiedProperties();

        var examineSO = new SerializedObject(examineItem);
        var linkedProp = examineSO.FindProperty("_linkedPickupItem");
        if (linkedProp != null) linkedProp.objectReferenceValue = pickup;
        examineSO.ApplyModifiedProperties();

        // Lưu checkpoint cảnh 3 ngay khi nhặt xong -- KHÔNG tự gán Spawn Point (không biết vị trí nào an
        // toàn) -- Jok tự kéo 1 Transform đặt sẵn trong scene vào Inspector sau khi tool chạy xong.
        var checkpointSaver = pickupGO.GetComponent<SaveCheckpointOnEvent>();
        bool checkpointSaverWasMissing = checkpointSaver == null;
        if (checkpointSaver == null) checkpointSaver = Undo.AddComponent<SaveCheckpointOnEvent>(pickupGO);

        if (!HasPersistentListener(pickup.OnPickedUp, checkpointSaver, "Save"))
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(pickup.OnPickedUp, checkpointSaver.Save);

        EditorUtility.SetDirty(musicBox);
        EditorUtility.SetDirty(pickup);
        EditorUtility.SetDirty(examineItem);
        EditorUtility.SetDirty(checkpointSaver);

        Debug.Log($"[VoD][MusicBox] XONG -- {(examineWasMissing ? "đã thêm mới" : "đã có sẵn")} ExamineItem trên '{pickupGO.name}'. " +
                  $"Thứ tự: thoại tò mò -> nghe băng -> Examine -> nhặt -> lưu checkpoint cảnh 3.");

        if (checkpointSaverWasMissing)
            Debug.LogWarning($"[VoD][MusicBox] BẮT BUỘC: vào '{pickupGO.name}' > SaveCheckpointOnEvent, kéo 1 Transform đặt ở vị trí AN TOÀN (không phải chỗ dễ bị ma tóm) vào field 'Spawn Point' -- chưa gán thì checkpoint cảnh 3 sẽ không lưu được gì cả.");
    }

    private static DialogueAsset GetOrCreateThoughtDialogue()
    {
        var existing = AssetDatabase.LoadAssetAtPath<DialogueAsset>(DialogueAssetPath);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<DialogueAsset>();
        asset.lines = new List<DialogueLine>
        {
            new DialogueLine
            {
                speakerName = "Khoa",
                text = "Một hộp nhạc bằng đồng, đặt ngay ngắn giữa lớp bụi -- như thể mới có ai đó lau qua nó chưa lâu.",
                hasVoice = false,
            },
            new DialogueLine
            {
                speakerName = "Khoa",
                text = "Bên trong này là gì nhỉ? Thử lên dây xem sao.",
                hasVoice = false,
            },
        };

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DialogueAssetPath));
        AssetDatabase.CreateAsset(asset, DialogueAssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[VoD][MusicBox] Đã tạo DialogueAsset mới tại '{DialogueAssetPath}' -- Jok tự chỉnh lại lời thoại nếu muốn khác.");
        return asset;
    }

    private static bool HasPersistentListener(UnityEngine.Events.UnityEvent evt, Object target, string methodName)
    {
        for (int i = 0; i < evt.GetPersistentEventCount(); i++)
        {
            if (evt.GetPersistentTarget(i) == target && evt.GetPersistentMethodName(i) == methodName)
                return true;
        }
        return false;
    }
}
