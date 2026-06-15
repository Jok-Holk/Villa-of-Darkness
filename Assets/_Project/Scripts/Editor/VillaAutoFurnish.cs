#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VillaAutoFurnish
{
    const string K_PATH = "Assets/_Project/Models/Props/Furniture/Kenney";
    const string M_PATH = "Assets/_Project/Materials";
    const string MARKER  = "__Furn";

    // 7-colour colonial palette (URP Lit)
    static readonly (string n, float r, float g, float b, float rough, float metal)[] PALETTE =
    {
        ("Mat_Wall_Ochre",      0.831f, 0.627f, 0.263f, 0.70f, 0.0f),
        ("Mat_Cornice_White",   0.961f, 0.941f, 0.910f, 0.50f, 0.0f),
        ("Mat_Jalousie_Green",  0.176f, 0.353f, 0.176f, 0.80f, 0.0f),
        ("Mat_Roof_TerraCotta", 0.722f, 0.361f, 0.220f, 0.75f, 0.0f),
        ("Mat_Wood_Teak",       0.420f, 0.267f, 0.137f, 0.50f, 0.0f),
        ("Mat_Iron_Railing",    0.102f, 0.102f, 0.180f, 0.60f, 0.8f),
        ("Mat_Perron_Granite",  0.533f, 0.533f, 0.533f, 0.40f, 0.0f),
    };

    // Room-name keyword → list of (Kenney GLB name, offsetX, offsetZ, rotY)
    static readonly Dictionary<string, (string g, float dx, float dz, float ry)[]> SETS = new()
    {
        ["DiningRoom"] = new (string, float, float, float)[]
        {
            ("table",            0f,     0f,    0f),
            ("chair",           -0.9f,   0f,   90f),
            ("chair",            0.9f,   0f,  270f),
            ("chair",            0f,    -0.9f,   0f),
            ("chair",            0f,     0.9f, 180f),
            ("drawers",         -2.2f,   1.8f,  90f),
            ("plantMedium1",     2.2f,   1.8f,   0f),
            ("lampRoundFloor",  -1.8f,  -1.8f,   0f),
        },
        ["FamilyRoom"] = new (string, float, float, float)[]
        {
            ("loungeDesignSofa",  0f,    1.2f,    0f),
            ("loungeChair",      -1.6f, -0.4f,  180f),
            ("loungeChair",       1.6f, -0.4f,  180f),
            ("tableCoffee",       0f,    0.2f,    0f),
            ("lampRoundFloor",   -2.5f,  1.8f,    0f),
            ("lampRoundFloor",    2.5f,  1.8f,    0f),
            ("bookcaseClosed",   -2.5f,  2.0f,   90f),
            ("plantMedium2",      2.5f,  2.0f,    0f),
            ("radio",            -2.0f, -0.5f,   90f),
        },
        ["Entertainment"] = new (string, float, float, float)[]
        {
            ("loungeDesignSofa",        0f,    0.8f,   0f),
            ("loungeDesignSofaCorner", -2.0f,  0.8f,  90f),
            ("tableCoffee",             0f,    0f,     0f),
            ("loungeChair",             2.2f, -0.4f, 180f),
            ("lampRoundFloor",          2.5f,  1.8f,   0f),
            ("plantMedium1",           -2.5f,  1.8f,   0f),
        },
        ["MasterBedroom"] = new (string, float, float, float)[]
        {
            ("bedDouble",           0f,     0.6f,   0f),
            ("sideTableDrawers",   -0.95f,  0.6f,  90f),
            ("sideTableDrawers",    0.95f,  0.6f, 270f),
            ("desk",               -1.8f,  -1.5f,  90f),
            ("chairCushion",       -0.8f,  -1.5f,   0f),
            ("drawers",             1.8f,  -1.2f, 270f),
            ("lampRoundFloor",      2.0f,   1.5f,   0f),
            ("pottedPlant",        -2.0f,   1.5f,   0f),
        },
        ["LinhBedroom"] = new (string, float, float, float)[]
        {
            ("bedSingle",        0f,     0.5f,   0f),
            ("sideTableDrawers",-0.7f,   0.5f,  90f),
            ("desk",             1.5f,  -1.2f, 270f),
            ("chair",            0.7f,  -1.2f,   0f),
            ("bookcaseOpen",    -1.8f,  -1.4f,  90f),
            ("pottedPlant",      1.8f,   1.4f,   0f),
        },
        ["MrsLanRoom"] = new (string, float, float, float)[]
        {
            ("bedDouble",           0f,    0.6f,   0f),
            ("sideTableDrawers",   -0.95f, 0.6f,  90f),
            ("sideTableDrawers",    0.95f, 0.6f, 270f),
            ("drawers",             1.8f, -1.2f, 270f),
            ("lampRoundFloor",     -2.0f,  1.5f,   0f),
            ("pottedPlant",         2.0f,  1.5f,   0f),
        },
        ["SonRoom"] = new (string, float, float, float)[]
        {
            ("bedSingle",        0f,     0.5f,   0f),
            ("desk",             1.5f,  -1.2f, 270f),
            ("chair",            0.7f,  -1.2f,   0f),
            ("bookcaseClosed",  -1.8f,  -1.4f,  90f),
            ("pottedPlant",      1.8f,   1.4f,   0f),
        },
        ["Bathroom"] = new (string, float, float, float)[]
        {
            ("bathtub",          0f,     0.5f,   0f),
            ("bathroomSink",     1.5f,  -0.5f,  90f),
            ("toilet",          -1.5f,  -0.5f, 270f),
            ("bathroomMirror",   1.5f,  -0.3f,   0f),
            ("bathroomCabinet", -1.5f,  -0.3f,   0f),
        },
        ["Bathroom_02"] = new (string, float, float, float)[]
        {
            ("bathroomSink",    0.5f,   0f,    0f),
            ("toilet",         -0.8f,   0f,    0f),
            ("bathroomMirror",  0.5f,   0.3f,  0f),
            ("shower",          0f,     0.8f,  0f),
        },
        ["Closet"] = new (string, float, float, float)[]
        {
            ("cabinetBed",   0f,    0.5f,   0f),
            ("cabinetBed",   1.3f,  0.5f,   0f),
            ("cabinetBed",  -1.3f,  0.5f,   0f),
            ("drawers",      0f,   -0.8f,   0f),
        },
    };

    // Corridor props: placed every CORRIDOR_STEP metres along any Hallway/Corridor object
    static readonly (string g, float dx, float dz, float ry)[] CORRIDOR_PROPS =
    {
        ("sideTable",     0f,    0f,   0f),
        ("lampRoundFloor",0f,   -1.2f, 0f),
        ("pottedPlant",   0f,    0.4f, 0f),
    };

    // ── Villa perimeter (from geometry scan, do NOT change) ───────────────
    const float FRONT_X   = 10.7f;
    const float BACK_X    = 52.8f;
    const float SIDE_Z_LO = 7.78f;
    const float SIDE_Z_HI = 47.56f;
    const float CENTER_X  = (FRONT_X + BACK_X)     * 0.5f;   // 31.75
    const float CENTER_Z  = (SIDE_Z_LO + SIDE_Z_HI) * 0.5f;  // 27.67
    const float VILLA_W   = BACK_X - FRONT_X;                 // 42.1 (X-axis)
    const float VILLA_D   = SIDE_Z_HI - SIDE_Z_LO;            // 39.78 (Z-axis)
    const float Y_GF_TOP  = 38.35f;  // top of ground-floor slab
    const float Y_1F_TOP  = 45.70f;  // top of first-floor slab

    // ════════════════════════════════════════════════════════════════
    //  MENU ITEMS
    // ════════════════════════════════════════════════════════════════

    [MenuItem("VoD/Auto/1 — Create 7 Materials")]
    static void CreateMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) { Debug.LogError("[AutoFurn] URP Lit shader not found"); return; }

        int count = 0;
        foreach (var (name, r, g, b, rough, metal) in PALETTE)
        {
            string path = $"{M_PATH}/{name}.mat";
            var mat     = AssetDatabase.LoadAssetAtPath<Material>(path) ?? new Material(shader);
            mat.name    = name;
            mat.SetColor("_BaseColor",  new Color(r, g, b));
            mat.SetFloat("_Smoothness", 1f - rough);
            mat.SetFloat("_Metallic",   metal);
            if (!AssetDatabase.Contains(mat)) AssetDatabase.CreateAsset(mat, path);
            else EditorUtility.SetDirty(mat);
            count++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[AutoFurn] ✓ {count} materials at {M_PATH}");
    }

    [MenuItem("VoD/Auto/2 — Furnish All Rooms")]
    static void FurnishAllRooms()
    {
        int placed = 0, rooms = 0;
        foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!go.name.StartsWith("Room_")) continue;
            if (go.transform.Find(MARKER) != null) continue;  // already done

            string key = SETS.Keys.FirstOrDefault(k => go.name.Contains(k));
            if (key == null) continue;
            rooms++;

            Vector3 floor = GetFloorCenter(go);
            var anchor    = new GameObject(MARKER);
            anchor.transform.SetParent(go.transform);
            anchor.transform.position = floor;
            Undo.RegisterCreatedObjectUndo(anchor, "AutoFurnish");

            foreach (var (glb, dx, dz, ry) in SETS[key])
                if (Spawn(glb, anchor.transform, dx, dz, ry, floor.y)) placed++;
        }
        MarkDirty();
        Debug.Log($"[AutoFurn] ✓ {placed} items placed in {rooms} rooms");
    }

    [MenuItem("VoD/Auto/3 — Populate Corridors")]
    static void PopulateCorridors()
    {
        int placed = 0;
        var keywords = new[] { "Hallway", "Corridor", "Hall", "Hành", "Landing" };
        foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            bool isHall = keywords.Any(k => go.name.Contains(k));
            if (!isHall) continue;
            if (go.transform.Find(MARKER) != null) continue;

            Vector3 floor = GetFloorCenter(go);
            var anchor    = new GameObject(MARKER);
            anchor.transform.SetParent(go.transform);
            anchor.transform.position = floor;
            Undo.RegisterCreatedObjectUndo(anchor, "CorridorProps");

            foreach (var (glb, dx, dz, ry) in CORRIDOR_PROPS)
                if (Spawn(glb, anchor.transform, dx, dz, ry, floor.y)) placed++;
        }
        MarkDirty();
        Debug.Log($"[AutoFurn] ✓ {placed} corridor props placed");
    }

    [MenuItem("VoD/Auto/4 — Add Cornice Bands")]
    static void AddCornices()
    {
        const float H = 0.35f, D = 0.30f;

        var root = new GameObject("Struct_CorniceGroup");
        Undo.RegisterCreatedObjectUndo(root, "Cornice");

        CorniceRing(root.transform, "GF", Y_GF_TOP + H * 0.5f, H, D);
        CorniceRing(root.transform, "1F", Y_1F_TOP + H * 0.5f, H, D);

        var whiteMat = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        if (whiteMat != null)
            foreach (var rend in root.GetComponentsInChildren<Renderer>())
                rend.sharedMaterial = whiteMat;

        MarkDirty();
        Debug.Log("[AutoFurn] ✓ Cornice bands added — run VoD/Villa/1 Run All for railings etc.");
    }

    [MenuItem("VoD/Auto/5 — Window Sills (all facades)")]
    static void AddWindowSills()
    {
        // Thin horizontal slab under every window position on the facade.
        // Approximate window Z positions on front face (-X): 3 windows, spaced evenly.
        const float SILL_H = 0.12f, SILL_D = 0.25f, SILL_W = 1.8f;
        float[] WIN_Z  = { 14f, 27.67f, 41f };    // approximate window centres Z
        float[] FLOOR_Y = { 32.05f + 1.1f, 39.3f + 1.1f };  // GF + 1F sill heights

        var root = new GameObject("Struct_SillGroup");
        Undo.RegisterCreatedObjectUndo(root, "Sills");

        foreach (float floorY in FLOOR_Y)
            foreach (float wz in WIN_Z)
            {
                // Front face sill
                Slab(root.transform, $"Sill_F_{wz:0}_{floorY:0}",
                     new Vector3(FRONT_X - SILL_D * 0.5f, floorY, wz),
                     new Vector3(SILL_D, SILL_H, SILL_W));
                // Back face sill
                Slab(root.transform, $"Sill_B_{wz:0}_{floorY:0}",
                     new Vector3(BACK_X  + SILL_D * 0.5f, floorY, wz),
                     new Vector3(SILL_D, SILL_H, SILL_W));
            }

        var granMat = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Perron_Granite.mat");
        if (granMat != null)
            foreach (var rend in root.GetComponentsInChildren<Renderer>())
                rend.sharedMaterial = granMat;

        MarkDirty();
        Debug.Log("[AutoFurn] ✓ Window sills added");
    }

    [MenuItem("VoD/Auto/0 — RUN ALL (materials + furniture + cornice + sills)")]
    static void RunAll()
    {
        CreateMaterials();
        FurnishAllRooms();
        PopulateCorridors();
        AddCornices();
        AddWindowSills();
        Debug.Log("[AutoFurn] ══ All steps complete ══");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════

    static void CorniceRing(Transform parent, string tag, float y, float h, float d)
    {
        // Front wall (-X), runs along Z
        Slab(parent, $"Cornice_{tag}_Front",
             new Vector3(FRONT_X - d * 0.5f, y, CENTER_Z),
             new Vector3(d, h, VILLA_D + d * 2f));
        // Back wall (+X)
        Slab(parent, $"Cornice_{tag}_Back",
             new Vector3(BACK_X  + d * 0.5f, y, CENTER_Z),
             new Vector3(d, h, VILLA_D + d * 2f));
        // Low-Z side, runs along X
        Slab(parent, $"Cornice_{tag}_SideLo",
             new Vector3(CENTER_X, y, SIDE_Z_LO - d * 0.5f),
             new Vector3(VILLA_W, h, d));
        // High-Z side
        Slab(parent, $"Cornice_{tag}_SideHi",
             new Vector3(CENTER_X, y, SIDE_Z_HI + d * 0.5f),
             new Vector3(VILLA_W, h, d));
    }

    static void Slab(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<BoxCollider>());
    }

    static Vector3 GetFloorCenter(GameObject room)
    {
        var renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return room.transform.position;
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);
    }

    static bool Spawn(string glbName, Transform parent, float dx, float dz, float ry, float floorY)
    {
        string path  = $"{K_PATH}/{glbName}.glb";
        var    asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (asset == null)
        {
            var guids = AssetDatabase.FindAssets($"{glbName} t:Model", new[] { K_PATH });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[AutoFurn] Not found: {glbName}");
                return false;
            }
            path  = AssetDatabase.GUIDToAssetPath(guids[0]);
            asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return false;
        }

        var go = UnityEngine.Object.Instantiate(asset, parent);
        go.name = asset.name;
        go.transform.position = new Vector3(parent.position.x + dx, floorY, parent.position.z + dz);
        go.transform.rotation = Quaternion.Euler(0f, ry, 0f);
        Undo.RegisterCreatedObjectUndo(go, "AutoFurnish");
        return true;
    }

    static void MarkDirty() =>
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
}
#endif
