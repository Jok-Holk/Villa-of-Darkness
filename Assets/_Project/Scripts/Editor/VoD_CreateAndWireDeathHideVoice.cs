using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Jok yêu cầu (2026-08-01): tự tạo DialogueAsset cho 6 câu MK-HIDE (thay vì bắt Jok tự làm tay), tự gắn cả
// 3 câu MK-DEATH vào GhostAI + WellDeathSequence. Text/speakerName để TRỐNG -- KHÔNG tự bịa nội dung câu
// thoại (không nghe được audio để biết chính xác lời), chỉ Jok mới biết đúng câu đã thu là gì. Việc còn lại
// cho Jok CHỈ CÒN LÀ GÕ CHỮ vào 6 dòng có sẵn, không cần biết Unity/ScriptableObject là gì.
// TOOL DÙNG 1 LẦN RỒI XOÁ.
public static class VoD_CreateAndWireDeathHideVoice
{
    private const string VoiceFolder = "Assets/_Project/Audio/Voice/Chapter1/";
    private const string AssetFolder = "Assets/_Project/Data/Dialogue/Chapter1/";
    private const string HideAssetPath = AssetFolder + "DialogueAsset_Ch1_MK_Hide.asset";

    [MenuItem("VoD/Villa/ONE-TIME - Tạo DialogueAsset MK-HIDE + Gắn MK-DEATH")]
    public static void Run()
    {
        if (!AssetDatabase.IsValidFolder(AssetFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Dialogue"))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Dialogue");
            AssetDatabase.CreateFolder("Assets/_Project/Data/Dialogue", "Chapter1");
        }

        // 1) Tạo DialogueAsset MK-HIDE (6 dòng, voiceClip gắn sẵn, text để TRỐNG cho Jok tự gõ)
        var hideAsset = AssetDatabase.LoadAssetAtPath<DialogueAsset>(HideAssetPath);
        bool isNewAsset = hideAsset == null;
        if (isNewAsset)
        {
            hideAsset = ScriptableObject.CreateInstance<DialogueAsset>();
            AssetDatabase.CreateAsset(hideAsset, HideAssetPath);
        }
        hideAsset.lines.Clear();
        for (int i = 1; i <= 6; i++)
        {
            string clipPath = $"{VoiceFolder}VO_Ch1_MK-HIDE-{i:00}.wav";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            hideAsset.lines.Add(new DialogueLine
            {
                speakerName = "Minh Khoa",
                text = "", // Jok tự gõ đúng câu đã thu -- KHÔNG tự bịa
                hasVoice = true,
                voiceClip = clip
            });
            if (clip == null)
                Debug.LogWarning($"[VoD] Không load được '{clipPath}' -- kiểm tra Unity import xong chưa.");
        }
        EditorUtility.SetDirty(hideAsset);
        AssetDatabase.SaveAssets();

        // 2) Tạo (nếu chưa có) 3 object cutscene còn thiếu -- GhostChaseIntroCutscene/ForcedHideCutscene/
        // GhostVanishTrigger -- rồi gắn hideAsset vào ForcedHideCutscene luôn trong bước này.
        var scene3Group = FindOrCreateScene3Group();
        var scene3Manager = Object.FindFirstObjectByType<Chapter1Scene3Manager>(FindObjectsInactive.Include);
        Vector3 anchor = scene3Manager != null ? scene3Manager.transform.position : Vector3.zero;

        var forcedHide = Object.FindFirstObjectByType<ForcedHideCutscene>(FindObjectsInactive.Include);
        if (forcedHide == null)
        {
            var go = CreateTriggerGO("Trigger_ForcedHide", scene3Group, anchor + new Vector3(-4f, 0f, 0f));
            forcedHide = go.AddComponent<ForcedHideCutscene>();
            Debug.Log("[VoD] Đã tạo mới 'Trigger_ForcedHide' -- Jok tự kéo tới đúng vị trí + gán Hide Spot.");
        }
        var soHide = new SerializedObject(forcedHide);
        soHide.FindProperty("_hideDialogue").objectReferenceValue = hideAsset;
        soHide.ApplyModifiedProperties();
        Debug.Log($"[VoD] Đã gắn DialogueAsset MK-HIDE vào ForcedHideCutscene tại '{forcedHide.gameObject.name}'.");

        if (Object.FindFirstObjectByType<GhostChaseIntroCutscene>(FindObjectsInactive.Include) == null)
        {
            var go = CreateTriggerGO("Trigger_GhostChaseIntro", scene3Group, anchor + new Vector3(-6f, 0f, 0f));
            go.AddComponent<GhostChaseIntroCutscene>();
            Debug.Log("[VoD] Đã tạo mới 'Trigger_GhostChaseIntro' -- Jok tự kéo tới đúng vị trí + gán Ghost.");
        }

        if (Object.FindFirstObjectByType<GhostVanishTrigger>(FindObjectsInactive.Include) == null)
        {
            var go = CreateTriggerGO("Trigger_GhostVanish", scene3Group, anchor + new Vector3(4f, 0f, 0f));
            go.AddComponent<GhostVanishTrigger>();
            Debug.Log("[VoD] Đã tạo mới 'Trigger_GhostVanish' -- Jok tự kéo tới đúng vị trí (hành lang sau) + gán Ghost.");
        }

        // 3) Gắn 3 file MK-DEATH thẳng vào GhostAI._playerDeathVoiceClips[] (AudioClip thô, không cần DialogueAsset)
        var deathClips = new AudioClip[3];
        for (int i = 0; i < 3; i++)
            deathClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>($"{VoiceFolder}VO_Ch1_MK-DEATH-{i + 1:00}.wav");

        var ghost = Object.FindFirstObjectByType<GhostAI>(FindObjectsInactive.Include);
        if (ghost != null)
        {
            var so = new SerializedObject(ghost);
            var arr = so.FindProperty("_playerDeathVoiceClips");
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = deathClips[i];
            so.ApplyModifiedProperties();
            Debug.Log($"[VoD] Đã gắn 3 file MK-DEATH vào GhostAI._playerDeathVoiceClips tại '{ghost.gameObject.name}'.");
        }

        var well = Object.FindFirstObjectByType<WellDeathSequence>(FindObjectsInactive.Include);
        if (well != null)
        {
            var so = new SerializedObject(well);
            so.FindProperty("_playerDeathVoiceClip").objectReferenceValue = deathClips[0];
            so.ApplyModifiedProperties();
            Debug.Log($"[VoD] Đã gắn MK-DEATH-01 vào WellDeathSequence._playerDeathVoiceClip tại '{well.gameObject.name}'.");
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[VoD] XONG. Việc còn lại: mở DialogueAsset_Ch1_MK_Hide (Assets/_Project/Data/Dialogue/Chapter1/) " +
                  "trong Inspector, gõ đúng 6 câu đã thu vào từng dòng 'Text'. Không cần biết gì khác về Unity.");
    }

    private static Transform FindOrCreateScene3Group()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "= CẢNH 3 (Story)") return go.transform;

        var newGroup = new GameObject("= CẢNH 3 (Story)");
        Undo.RegisterCreatedObjectUndo(newGroup, "Create scene3 group");
        return newGroup.transform;
    }

    private static GameObject CreateTriggerGO(string name, Transform parent, Vector3 worldPosition)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create cutscene trigger");
        go.transform.SetParent(parent, false);
        go.transform.position = worldPosition;

        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(2f, 2f, 2f);

        return go;
    }
}
