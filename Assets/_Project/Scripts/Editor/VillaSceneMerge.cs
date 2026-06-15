using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public static class VillaSceneMerge
{
    const string CHAPTER1 = "Assets/_Project/Scenes/Chapter1.unity";
    const string SCENE_GAMEPLAY = "Assets/_Project/Scenes/gameplay-ch1-test-scene.unity";
    const string SCENE_AUDIO    = "Assets/_Project/Scenes/phase2featureaudio-sanity-ch1.unity";
    const string SCENE_TRIGGER  = "Assets/_Project/Scenes/Trigger-TestScene.unity";

    // ─────────────────────────── TOP-LEVEL MENU ───────────────────────────

    [MenuItem("VoD/SceneMerge/1 Merge Gameplay Scene")]
    public static void MergeGameplay() => MergeFrom(SCENE_GAMEPLAY, GameplayFilter);

    [MenuItem("VoD/SceneMerge/2 Merge Audio-Sanity Scene")]
    public static void MergeAudio() => MergeFrom(SCENE_AUDIO, AudioFilter);

    [MenuItem("VoD/SceneMerge/3 Merge Trigger Scene")]
    public static void MergeTrigger() => MergeFrom(SCENE_TRIGGER, TriggerFilter);

    [MenuItem("VoD/SceneMerge/4 Delete Test Scenes + Fix Build Settings")]
    public static void DeleteTestScenes()
    {
        string[] toDelete = {
            SCENE_GAMEPLAY,
            SCENE_AUDIO,
            SCENE_TRIGGER,
            "Assets/_Project/Scenes/TestScene.unity",
            "Assets/_Project/Scenes/TestMenu.unity",
        };
        foreach (var p in toDelete)
        {
            if (!File.Exists(p)) continue;
            if (AssetDatabase.DeleteAsset(p))
                Debug.Log($"[Merge] Deleted {p}");
            else
                Debug.LogWarning($"[Merge] Could not delete {p}");
        }
        AssetDatabase.Refresh();
        FixBuildSettings();
    }

    static void FixBuildSettings()
    {
        var keep = new[]
        {
            "Assets/_Project/Scenes/MainMenu.unity",
            CHAPTER1,
            "Assets/_Project/Scenes/Chapter2.unity",
            "Assets/_Project/Scenes/Chapter3.unity",
            "Assets/_Project/Scenes/Chapter4.unity",
        };
        var list = new List<EditorBuildSettingsScene>();
        foreach (var p in keep)
        {
            if (File.Exists(p))
                list.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("[Merge] Build Settings updated — MainMenu(0) + Chapter1(1) + chapters.");
    }

    [MenuItem("VoD/SceneMerge/0 Run All (Merge + Delete)")]
    public static void RunAll()
    {
        MergeGameplay();
        MergeAudio();
        MergeTrigger();
        DeleteTestScenes();
        var ch1 = SceneManager.GetSceneByPath(CHAPTER1);
        if (ch1.IsValid()) EditorSceneManager.SaveScene(ch1);
        Debug.Log("[Merge] All done — Chapter1 saved.");
    }

    // ────────────────────────── CORE MERGE ────────────────────────────────

    static void MergeFrom(string srcPath, System.Func<GameObject, bool> filter)
    {
        if (!File.Exists(srcPath))
        {
            Debug.LogWarning($"[Merge] Source not found: {srcPath}");
            return;
        }

        // Ensure Chapter1 is open
        Scene ch1 = SceneManager.GetSceneByPath(CHAPTER1);
        if (!ch1.IsValid())
        {
            ch1 = EditorSceneManager.OpenScene(CHAPTER1, OpenSceneMode.Single);
        }

        // Load source additively
        Scene src = EditorSceneManager.OpenScene(srcPath, OpenSceneMode.Additive);

        // Collect roots to migrate
        var roots = src.GetRootGameObjects();
        int moved = 0;
        foreach (var go in roots)
        {
            if (!filter(go)) continue;

            // Skip duplicates (same name already in Chapter1)
            if (FindInScene(ch1, go.name) != null)
            {
                Debug.Log($"[Merge] Skipping duplicate: {go.name}");
                continue;
            }

            SceneManager.MoveGameObjectToScene(go, ch1);
            moved++;
            Debug.Log($"[Merge] Moved: {go.name}");
        }

        // Unload source without saving
        EditorSceneManager.CloseScene(src, true);
        EditorSceneManager.SaveScene(ch1);
        Debug.Log($"[Merge] Done — {moved} objects moved from {Path.GetFileName(srcPath)}");
    }

    static GameObject FindInScene(Scene scene, string name)
    {
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == name) return r;
        return null;
    }

    // ───────────────────────── FILTERS ────────────────────────────────────

    // From gameplay-ch1-test-scene: Piano, SpawnManager, Canvas UI,
    // InventorySystem, EventSystem, GlobalVolume, Props group, Door
    static bool GameplayFilter(GameObject go)
    {
        string n = go.name;
        // Skip environment/lighting that's already in Chapter1
        if (n == "Directional Light" || n == "Main Camera" || n == "Player" ||
            n == "SanitySystem"       || n == "AudioManager" || n == "GameManager" ||
            n.StartsWith("Room_")    || n.StartsWith("Arch_") || n.StartsWith("Floor") ||
            n.StartsWith("Ceil")     || n.StartsWith("Wall") || n == "NavMesh" ||
            n == "Ground")
            return false;

        return true; // take everything else (Piano, SpawnManager, Canvas, etc.)
    }

    // From phase2featureaudio-sanity-ch1: AmbientZones group, AudioLog_Diary,
    // AudioLog_MusicBox, SanityManager
    static bool AudioFilter(GameObject go)
    {
        string n = go.name;
        return n == "AmbientZones"     ||
               n == "AudioLog_Diary"   ||
               n == "AudioLog_MusicBox"||
               n == "SanityManager"    ||
               n.StartsWith("Ambient") ||
               n.StartsWith("AudioLog");
    }

    // From Trigger-TestScene: MirrorCamera, Mirror_Surface, DeathScreenUI,
    // waypoints (WP_*), Zones (Zone_*), SpawnManager variants
    static bool TriggerFilter(GameObject go)
    {
        string n = go.name;
        return n == "MirrorCamera"     ||
               n == "Mirror_Surface"   ||
               n == "DeathScreenUI"    ||
               n == "SpawnManager_GO"  ||
               n.StartsWith("WP_")    ||
               n.StartsWith("Zone_");
    }
}
