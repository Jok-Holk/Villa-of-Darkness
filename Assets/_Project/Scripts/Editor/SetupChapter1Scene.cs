using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// One-shot setup: import GLB → apply materials → organize hierarchy → create additive zones.
/// Menu: VoD / Setup Chapter1 Scene
/// </summary>
public static class SetupChapter1Scene
{
    const string GLB_ASSET = "Assets/_Project/Models/Architecture/Chapter1_WithHipRoof.glb";
    const string CHAPTER1_SCENE = "Assets/_Project/Scenes/Chapter1.unity";
    const string ZONES_FOLDER = "Assets/_Project/Scenes/Zones";

    // ── Material map: name-prefix → material path ─────────────────────────
    static readonly (string prefix, string matPath)[] MatRules = new[]
    {
        ("HipRoof",         "Assets/_Project/Materials/Mat_Roof_TerraCotta.mat"),
        ("roof",            "Assets/_Project/Materials/Mat_Roof_TerraCotta.mat"),
        ("Roof",            "Assets/_Project/Materials/Mat_Roof_TerraCotta.mat"),
        ("Wall_Ext",        "Assets/_Project/Materials/Mat_Wall_Exterior_Ochre.mat"),
        ("Struct_Balcony",  "Assets/_Project/Materials/Mat_Wall_Exterior_Ochre.mat"),
        ("Struct_Entrance", "Assets/_Project/Materials/Mat_Cornice_White.mat"),
        ("Wall_Int",        "Assets/_Project/Materials/Mat_Wall_Interior_Cream.mat"),
        ("Ped_",            "Assets/_Project/Materials/Mat_Cornice_White.mat"),
        ("Quoin",           "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("Chimney",         "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("WT_",             "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("FencePillar",     "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("GatePillar",      "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("Balcony_",        "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("FenceRail",       "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("Fence_",          "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("Fence_Post",      "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("LadderFolding",   "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("Lamp_",           "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("SupportPillar",   "Assets/_Project/Materials/Mat_Wall_Exterior_Ochre.mat"),
        ("Slab_",           "Assets/_Project/Materials/Mat_Floor_CementTile.mat"),
        ("Floor_",          "Assets/_Project/Materials/Mat_Floor_CementTile.mat"),
        ("Stair",           "Assets/_Project/Materials/Mat_Floor_CementTile.mat"),
        ("Ground_",         "Assets/_Project/Materials/Mat_Floor_CementTile.mat"),
        ("Approach_",       "Assets/_Project/Materials/Mat_Floor_Courtyard.mat"),
        ("Path_",           "Assets/_Project/Materials/Mat_Floor_Courtyard.mat"),
        ("PathFlag",        "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("Lawn_",           "Assets/_Project/Materials/Mat_Garden_Grass.mat"),
        ("GardenBed_Soil",  "Assets/_Project/Materials/Mat_Garden_Gravel.mat"),
        ("GardenBed_",      "Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("Shrub_",          "Assets/_Project/Materials/Mat_Garden_Hedge.mat"),
        ("DeadTree",        "Assets/_Project/Materials/Mat_Palm_Trunk.mat"),
        ("Prop_DryFountain","Assets/_Project/Materials/Mat_Wall_Stone.mat"),
        ("PSX_",            "Assets/_Project/Materials/Mat_Wood_Furniture.mat"),
        ("Bed_Flower",      "Assets/_Project/Materials/Mat_Garden_FlowerY.mat"),
        ("Bed_Hedge",       "Assets/_Project/Materials/Mat_Garden_Hedge.mat"),
        ("wicket",          "Assets/_Project/Materials/Mat_Iron_Fence.mat"),
        ("Curtain_",        "Assets/_Project/Materials/Mat_Jalousie_Green.mat"),
    };

    // ── Hierarchy groups ───────────────────────────────────────────────────
    // Each entry: (separator label, header color hex, prefix list)
    static readonly (string sep, string color, string[] prefixes)[] Groups = new[]
    {
        ("STRUCTURE", "4A90D9",   new[]{"HipRoof","roof","Roof","Wall_Ext","Wall_Int","Slab_",
                                         "Floor_","Stair","Struct_","Ped_","Quoin","Chimney",
                                         "SupportPillar","LadderFolding","wicket"}),
        ("GALERIE",   "E8A838",   new[]{"Balcony_","Balcony_1F","Balcony_2F","Balcony_3F"}),
        ("FENCE",     "7B7B7B",   new[]{"FencePillar","FenceRail","Fence_","GatePillar",
                                         "Fence_Post","Fence_Side","Fence_Back","Fence_Front"}),
        ("WATCHTOWER","9B59B6",   new[]{"WT_","Prop_Chair","table (","Prop_Chair_20","table (2)"}),
        ("EXTERIOR",  "2ECC71",   new[]{"Ground_","Approach_","Lawn_","Path_","PathFlag",
                                         "StonePath","ApproachWall"}),
        ("GARDEN",    "27AE60",   new[]{"GardenBed_","Shrub_","Prop_DryFountain",
                                         "Bed_Flower","Bed_Hedge"}),
        ("TREES",     "5D4037",   new[]{"DeadTree","Lamp_"}),
        ("FURNITURE", "795548",   new[]{"PSX_","Curtain_"}),
    };

    // ── Additive zone scenes ──────────────────────────────────────────────
    static readonly (string sceneName, string[] assignedTo, string description)[] Zones = new[]
    {
        ("Chapter1_Zone_Struct",    new[]{"Tuấn Anh"}, "Shell building — walls, roof, stairs"),
        ("Chapter1_Zone_GF_Decor",  new[]{"Thuận"},    "Ground Floor props & gameplay items"),
        ("Chapter1_Zone_L1_Decor",  new[]{"Tân"},      "1st Floor props & ambient zones"),
        ("Chapter1_Zone_L2_Decor",  new[]{"Phúc"},     "2nd Floor props & ghost waypoints"),
        ("Chapter1_Zone_Exterior",  new[]{"Tuấn Anh"}, "Garden, fence, allee, trees"),
        ("Chapter1_Zone_UI_FX",     new[]{"Vũ"},       "Lighting, particle FX, post-process"),
    };

    // ──────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/1 – Setup Chapter1 Scene %#1")]
    public static void Run()
    {
        EditorUtility.DisplayProgressBar("VoD Setup", "Starting…", 0f);
        try
        {
            // 1. Ensure GLB is imported
            AssetDatabase.ImportAsset(GLB_ASSET, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar("VoD Setup", "GLB imported", 0.1f);

            // 2. Open Chapter1
            var scene = EditorSceneManager.OpenScene(CHAPTER1_SCENE, OpenSceneMode.Single);

            // 3. Remove old GLB root if any
            foreach (var old in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (old.name == "Chapter1_WithHipRoof" && old.transform.parent == null)
                    Object.DestroyImmediate(old);

            // 4. Instantiate GLB prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GLB_ASSET);
            if (prefab == null) { Debug.LogError("GLB not found at " + GLB_ASSET); return; }
            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            root.name = "VillaChapter1";
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            EditorUtility.DisplayProgressBar("VoD Setup", "GLB instantiated", 0.25f);

            // 5. Apply materials
            ApplyMaterials(root);
            EditorUtility.DisplayProgressBar("VoD Setup", "Materials applied", 0.5f);

            // 6. Build hierarchy groups inside the root
            OrganizeHierarchy(root);
            EditorUtility.DisplayProgressBar("VoD Setup", "Hierarchy organized", 0.7f);

            // 7. Create additive zone scenes
            CreateZoneScenes();
            EditorUtility.DisplayProgressBar("VoD Setup", "Zone scenes created", 0.9f);

            // 8. Save Chapter1
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("VoD Setup", "Done!", 1f);
            Debug.Log("[VoD] Chapter1 setup complete.");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // ── Material assignment ────────────────────────────────────────────────
    static void ApplyMaterials(GameObject root)
    {
        // Cache materials
        var matCache = new Dictionary<string, Material>();
        Material GetMat(string path)
        {
            if (!matCache.TryGetValue(path, out var m))
                matCache[path] = m = AssetDatabase.LoadAssetAtPath<Material>(path);
            return m;
        }

        int applied = 0;
        foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            string n = mr.gameObject.name;
            foreach (var (prefix, matPath) in MatRules)
            {
                if (n.StartsWith(prefix) || n.Contains(prefix))
                {
                    var mat = GetMat(matPath);
                    if (mat != null)
                    {
                        // Apply to all sub-materials slots
                        var mats = new Material[mr.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                        mr.sharedMaterials = mats;
                        applied++;
                        break;
                    }
                }
            }
        }
        Debug.Log($"[VoD] Applied materials to {applied} renderers.");
    }

    // ── Hierarchy organizer ───────────────────────────────────────────────
    static void OrganizeHierarchy(GameObject root)
    {
        // Build prefix → group name lookup
        var lookup = new Dictionary<string, string>();
        foreach (var (sep, _, prefixes) in Groups)
            foreach (var p in prefixes)
                lookup[p] = sep;

        // Create group GameObjects under root
        var groupGOs = new Dictionary<string, GameObject>();
        foreach (var (sep, color, _) in Groups)
        {
            var g = new GameObject($"[{sep}]");
            g.transform.SetParent(root.transform, false);
            groupGOs[sep] = g;
        }

        // Reparent each direct child by name prefix
        var children = new List<Transform>();
        foreach (Transform t in root.transform)
            if (!t.name.StartsWith("["))
                children.Add(t);

        foreach (var child in children)
        {
            string groupName = null;
            foreach (var (prefix, gName) in lookup)
                if (child.name.StartsWith(prefix) || child.name.Contains(prefix))
                { groupName = gName; break; }

            if (groupName != null && groupGOs.TryGetValue(groupName, out var g))
                child.SetParent(g.transform, true);
        }

        // Move ungrouped to a catch-all
        var misc = new GameObject("[MISC]");
        misc.transform.SetParent(root.transform, false);
        foreach (Transform t in root.transform.Cast<Transform>().ToList())
            if (!t.name.StartsWith("[") && t.parent == root.transform)
                t.SetParent(misc.transform, true);

        Debug.Log("[VoD] Hierarchy organized.");
    }

    // ── Create additive zone scenes ───────────────────────────────────────
    static void CreateZoneScenes()
    {
        if (!AssetDatabase.IsValidFolder(ZONES_FOLDER))
            AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Zones");

        foreach (var (sceneName, assignedTo, description) in Zones)
        {
            string path = $"{ZONES_FOLDER}/{sceneName}.unity";
            if (File.Exists(Path.GetFullPath(path))) continue; // Don't overwrite

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            newScene.name = sceneName;

            // Add a single root GameObject with assignment info
            var info = new GameObject($"[ZONE: {sceneName}]");
            info.AddComponent<ZoneInfo>().Setup(description, assignedTo);
            SceneManager.MoveGameObjectToScene(info, newScene);

            EditorSceneManager.SaveScene(newScene, path);
            EditorSceneManager.CloseScene(newScene, true);
            Debug.Log($"[VoD] Created zone scene: {path} → {string.Join(", ", assignedTo)}");
        }

        // Update Build Settings
        UpdateBuildSettings();
    }

    static void UpdateBuildSettings()
    {
        var existing = EditorBuildSettings.scenes.ToList();
        var existingPaths = new HashSet<string>(existing.Select(s => s.path));

        // Chapter1 main stays
        string[] zonePaths = Zones.Select(z => $"{ZONES_FOLDER}/{z.sceneName}.unity").ToArray();
        foreach (var p in zonePaths)
            if (!existingPaths.Contains(p))
                existing.Add(new EditorBuildSettingsScene(p, false)); // disabled by default

        EditorBuildSettings.scenes = existing.ToArray();
        Debug.Log("[VoD] Build Settings updated with zone scenes.");
    }

    [MenuItem("VoD/2 – Print Zone Assignment Table")]
    public static void PrintZones()
    {
        Debug.Log("=== ZONE ASSIGNMENT TABLE ===");
        foreach (var (name, assigned, desc) in Zones)
            Debug.Log($"{name,-35} → {string.Join(", ", assigned),-15} | {desc}");
    }
}
