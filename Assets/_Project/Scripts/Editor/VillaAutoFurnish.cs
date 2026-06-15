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
        ["Kitchen"] = new (string, float, float, float)[]
        {
            ("kitchenFridge",  -1.5f,  1.8f,  90f),
            ("kitchenStove",   -1.5f,  0.6f,  90f),
            ("kitchenSink",    -1.5f, -0.6f,  90f),
            ("table",           0.8f,  0.2f,   0f),
            ("chair",           0.2f, -0.9f,   0f),
            ("chair",           1.4f, -0.9f,   0f),
            ("drawers",         1.8f,  1.5f, 270f),
            ("plantMedium1",    1.8f, -1.5f,   0f),
        },
        ["Study"] = new (string, float, float, float)[]
        {
            ("desk",             0f,     1.2f, 180f),
            ("chair",            0f,     0.4f,   0f),
            ("bookcaseClosed",  -2.0f,   1.6f,  90f),
            ("bookcaseClosed",  -2.0f,   0.2f,  90f),
            ("bookcaseOpen",     2.0f,   1.6f, 270f),
            ("lampRoundFloor",   1.8f,   0.5f,   0f),
            ("loungeChair",     -1.5f,  -0.8f,  45f),
            ("tableCoffee",     -0.6f,  -0.8f,   0f),
            ("pottedPlant",     -2.0f,  -1.5f,   0f),
        },
        ["Library"] = new (string, float, float, float)[]
        {
            ("bookcaseClosed",  -2.2f,   1.6f,  90f),
            ("bookcaseClosed",  -2.2f,   0.2f,  90f),
            ("bookcaseClosed",  -2.2f,  -1.2f,  90f),
            ("bookcaseOpen",     2.2f,   1.6f, 270f),
            ("bookcaseOpen",     2.2f,   0.2f, 270f),
            ("desk",             0f,     0.8f, 180f),
            ("chair",            0f,     0.1f,   0f),
            ("lampRoundFloor",   1.5f,   0.2f,   0f),
            ("loungeChair",     -1.5f,  -1.0f,  90f),
        },
        ["ServantRoom"] = new (string, float, float, float)[]
        {
            ("bedSingle",    0f,    0.5f,   0f),
            ("sideTable",   -0.8f,  0.5f,  90f),
            ("chair",        0.8f, -0.8f,   0f),
            ("drawers",      0.8f,  0.8f, 270f),
        },
        ["Storage"] = new (string, float, float, float)[]
        {
            ("cabinetBed",   0f,    0.5f,   0f),
            ("cabinetBed",   1.3f,  0.5f,   0f),
            ("drawers",     -1.3f,  0.5f, 180f),
        },
        ["Pantry"] = new (string, float, float, float)[]
        {
            ("cabinetBed",   0f,    0.7f,   0f),
            ("cabinetBed",   1.3f,  0.7f,   0f),
            ("cabinetBed",  -1.3f,  0.7f,   0f),
            ("drawers",      0f,   -0.8f, 180f),
        },
    };

    // Corridor props — luxury colonial: console clusters, plants, seating, ceiling lights
    static readonly (string g, float dx, float dz, float ry)[] CORRIDOR_PROPS =
    {
        // Console table + mirror niche
        ("sideTableDrawers",  0f,   -1.8f,   0f),
        ("bathroomMirror",    0f,   -1.8f,   0f),
        // Flanking floor lamps
        ("lampRoundFloor",    0.9f, -1.6f,   0f),
        ("lampRoundFloor",   -0.9f, -1.6f, 180f),
        // Central display plants
        ("plantMedium1",      1.6f,  0.3f,   0f),
        ("plantMedium2",     -1.6f,  0.3f,   0f),
        ("pottedPlant",       0f,    0.4f,   0f),
        // Seating nook
        ("loungeChair",      -1.5f,  0.8f,  90f),
        ("loungeChair",       1.5f,  0.8f, 270f),
        ("tableCoffee",       0f,    0.8f,   0f),
        // Rug underfoot
        ("rugRectangle",      0f,    0f,    90f),
        // Ceiling fixture above
        ("lampSquareCeiling", 0f,    0f,     0f),
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

    [MenuItem("VoD/Auto/6 — Façade Ornaments (pilasters + quoins + pediment)")]
    static void AddFacadeOrnaments()
    {
        AddPilasters();
        AddQuoins();
        AddEntrancePediment();
        MarkDirty();
        Debug.Log("[AutoFurn] ✓ Façade ornaments complete");
    }

    [MenuItem("VoD/Auto/7 — Add String Courses (chỉ ngang tường)")]
    static void AddStringCourses()
    {
        const float SC_H = 0.08f, SC_D = 0.16f;

        // Y positions: base, mid-GF, cornice-line sub-band on each floor
        float[] scY = { 33.55f, 36.0f, Y_GF_TOP - 0.45f, 40.55f, 43.5f, Y_1F_TOP - 0.45f };

        var root   = new GameObject("Struct_StringCourseGroup");
        var matW   = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        Undo.RegisterCreatedObjectUndo(root, "StringCourses");

        foreach (float y in scY)
            CorniceRing(root.transform, $"SC_{y:0}", y, SC_H, SC_D);

        if (matW != null)
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = matW;

        MarkDirty();
        Debug.Log($"[AutoFurn] ✓ {scY.Length * 4} string course bands added");
    }

    [MenuItem("VoD/Auto/8 — Add Rusticated Base (đá chân tường)")]
    static void AddRusticatedBase()
    {
        const float RB_H_TALL = 0.40f, RB_H_SHORT = 0.28f;
        const float RB_PROT   = 0.10f;  // protrusion from wall face
        const float RB_BASE_Y = 32.55f; // Y of first block bottom
        const float BLOCK_GAP = 0.03f;
        const float TOTAL_H   = 1.6f;   // height of rusticated band

        var matGran = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Perron_Granite.mat");
        var root    = new GameObject("Struct_RusticatedBase");
        Undo.RegisterCreatedObjectUndo(root, "RusticBase");

        // Front face (−X), blocks stacked alternating, each block = full Z span, different heights
        float curY = RB_BASE_Y;
        bool  tall = true;
        while (curY < RB_BASE_Y + TOTAL_H - 0.10f)
        {
            float bh   = tall ? RB_H_TALL : RB_H_SHORT;
            float posY = curY + bh * 0.5f;
            // Front
            var f = Slab(root.transform, $"RB_F_{curY:00}",
                new Vector3(FRONT_X - RB_PROT * 0.5f, posY, CENTER_Z),
                new Vector3(RB_PROT, bh, VILLA_D + RB_PROT * 2f));
            // Back
            var b2 = Slab(root.transform, $"RB_B_{curY:00}",
                new Vector3(BACK_X + RB_PROT * 0.5f, posY, CENTER_Z),
                new Vector3(RB_PROT, bh, VILLA_D + RB_PROT * 2f));
            // Side Lo
            var sl = Slab(root.transform, $"RB_SL_{curY:00}",
                new Vector3(CENTER_X, posY, SIDE_Z_LO - RB_PROT * 0.5f),
                new Vector3(VILLA_W, bh, RB_PROT));
            // Side Hi
            var sh = Slab(root.transform, $"RB_SH_{curY:00}",
                new Vector3(CENTER_X, posY, SIDE_Z_HI + RB_PROT * 0.5f),
                new Vector3(VILLA_W, bh, RB_PROT));
            if (matGran != null)
                foreach (var go in new[] { f, b2, sl, sh })
                    go.GetComponent<Renderer>().sharedMaterial = matGran;

            curY += bh + BLOCK_GAP;
            tall  = !tall;
        }

        MarkDirty();
        Debug.Log("[AutoFurn] ✓ Rusticated base added — full perimeter");
    }

    [MenuItem("VoD/Auto/9 — Add Window Crowns + Modillions")]
    static void AddWindowCrownsAndModillions()
    {
        var matW   = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        var root   = new GameObject("Struct_WindowCrowns");
        Undo.RegisterCreatedObjectUndo(root, "WindowCrowns");

        // Window Z positions on front/back facade (same as sill script)
        float[] winZ   = { CENTER_Z - 11f, CENTER_Z - 5f, CENTER_Z, CENTER_Z + 5f, CENTER_Z + 11f };
        float[] floorY = { Y_GF_TOP - 1.05f, Y_1F_TOP - 1.05f }; // approx top of window

        const float CW_W = 1.9f, CW_H = 0.14f, CW_D = 0.22f; // crown width, height, depth

        // Window crowns on front face
        foreach (float wz in winZ)
            foreach (float fy in floorY)
            {
                var cro = Slab(root.transform, $"WCrown_F_{wz:0}_{fy:0}",
                    new Vector3(FRONT_X - CW_D * 0.5f, fy, wz),
                    new Vector3(CW_D, CW_H, CW_W));
                if (matW != null) cro.GetComponent<Renderer>().sharedMaterial = matW;
            }

        // Window crowns on back face
        foreach (float wz in winZ)
            foreach (float fy in floorY)
            {
                var cro = Slab(root.transform, $"WCrown_B_{wz:0}_{fy:0}",
                    new Vector3(BACK_X + CW_D * 0.5f, fy, wz),
                    new Vector3(CW_D, CW_H, CW_W));
                if (matW != null) cro.GetComponent<Renderer>().sharedMaterial = matW;
            }

        // Modillions: bracket blocks under each cornice band, every MOD_STEP metres
        const float MOD_W  = 0.14f, MOD_H  = 0.22f, MOD_D = 0.18f;
        const float MOD_STEP = 1.5f;

        var modRoot = new GameObject("Struct_Modillions");
        Undo.RegisterCreatedObjectUndo(modRoot, "Modillions");

        float[] corniceY = { Y_GF_TOP + 0.02f, Y_1F_TOP + 0.02f };
        foreach (float cy in corniceY)
        {
            // Front face — modillions along Z
            for (float mz = SIDE_Z_LO + MOD_STEP * 0.5f; mz < SIDE_Z_HI; mz += MOD_STEP)
            {
                var m = Slab(modRoot.transform, $"Mod_F_{cy:0}_{mz:0}",
                    new Vector3(FRONT_X - MOD_D * 0.5f, cy + MOD_H * 0.5f, mz),
                    new Vector3(MOD_D, MOD_H, MOD_W));
                if (matW != null) m.GetComponent<Renderer>().sharedMaterial = matW;
            }
            // Back face
            for (float mz = SIDE_Z_LO + MOD_STEP * 0.5f; mz < SIDE_Z_HI; mz += MOD_STEP)
            {
                var m = Slab(modRoot.transform, $"Mod_B_{cy:0}_{mz:0}",
                    new Vector3(BACK_X + MOD_D * 0.5f, cy + MOD_H * 0.5f, mz),
                    new Vector3(MOD_D, MOD_H, MOD_W));
                if (matW != null) m.GetComponent<Renderer>().sharedMaterial = matW;
            }
            // Side Lo
            for (float mx = FRONT_X + MOD_STEP * 0.5f; mx < BACK_X; mx += MOD_STEP)
            {
                var m = Slab(modRoot.transform, $"Mod_SL_{cy:0}_{mx:0}",
                    new Vector3(mx, cy + MOD_H * 0.5f, SIDE_Z_LO - MOD_D * 0.5f),
                    new Vector3(MOD_W, MOD_H, MOD_D));
                if (matW != null) m.GetComponent<Renderer>().sharedMaterial = matW;
            }
            // Side Hi
            for (float mx = FRONT_X + MOD_STEP * 0.5f; mx < BACK_X; mx += MOD_STEP)
            {
                var m = Slab(modRoot.transform, $"Mod_SH_{cy:0}_{mx:0}",
                    new Vector3(mx, cy + MOD_H * 0.5f, SIDE_Z_HI + MOD_D * 0.5f),
                    new Vector3(MOD_W, MOD_H, MOD_D));
                if (matW != null) m.GetComponent<Renderer>().sharedMaterial = matW;
            }
        }

        MarkDirty();
        Debug.Log("[AutoFurn] ✓ Window crowns + modillions added");
    }

    [MenuItem("VoD/Auto/A — Add Bay Windows (vịnh cửa lồi ra ngoài)")]
    static void AddBayWindows()
    {
        const float BW_DEPTH  = 0.55f;  // how far the bay protrudes from wall
        const float BW_W      = 1.55f;  // Z width of bay
        const float BW_WALL_T = 0.12f;  // side wall thickness
        const float BW_CAP_H  = 0.14f;  // cap slab height

        // Window bays — skip CENTER_Z (entrance pediment already there)
        float[] winZ   = { CENTER_Z - 11f, CENTER_Z - 5f, CENTER_Z + 5f, CENTER_Z + 11f };
        // (bottomY, topY) of window opening on each floor
        (float bot, float top)[] wFloors = { (33.5f, 37.7f), (40.6f, 44.8f) };

        var root    = new GameObject("Struct_BayWindows");
        var matWall = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Wall_Ochre.mat");
        var matCap  = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        Undo.RegisterCreatedObjectUndo(root, "BayWindows");

        foreach (float wz in winZ)
        foreach (var (bot, top) in wFloors)
        {
            float h  = top - bot;
            float cy = bot + h * 0.5f;
            float bx = FRONT_X - BW_DEPTH * 0.5f;  // centre of bay protrusion

            // Front panel of bay (faces outward in -X)
            var front = Slab(root.transform, $"Bay_Front_{wz:0}_{bot:0}",
                new Vector3(FRONT_X - BW_DEPTH, cy, wz),
                new Vector3(BW_WALL_T, h, BW_W));
            if (matWall != null) front.GetComponent<Renderer>().sharedMaterial = matWall;

            // Side return walls
            foreach (float sz in new[] { wz - BW_W * 0.5f, wz + BW_W * 0.5f })
            {
                var side = Slab(root.transform, $"Bay_Side_{wz:0}_{sz:0}_{bot:0}",
                    new Vector3(FRONT_X - BW_DEPTH * 0.5f, cy, sz),
                    new Vector3(BW_DEPTH, h, BW_WALL_T));
                if (matWall != null) side.GetComponent<Renderer>().sharedMaterial = matWall;
            }

            // Cap/hood on top of each bay
            var cap = Slab(root.transform, $"Bay_Cap_{wz:0}_{bot:0}",
                new Vector3(FRONT_X - BW_DEPTH * 0.5f, top + BW_CAP_H * 0.5f, wz),
                new Vector3(BW_DEPTH + 0.08f, BW_CAP_H, BW_W + 0.12f));
            if (matCap != null) cap.GetComponent<Renderer>().sharedMaterial = matCap;
        }

        MarkDirty();
        Debug.Log($"[AutoFurn] ✓ {winZ.Length * wFloors.Length} bay windows added to front facade");
    }

    [MenuItem("VoD/Auto/B — Add Chimneys (ống khói)")]
    static void AddChimneys()
    {
        const float CH_W  = 0.90f;  // chimney shaft width (square)
        const float CH_H  = 4.80f;  // shaft height above roof
        const float CAP_W = 1.15f;  // cap wider
        const float CAP_H = 0.28f;

        // 2 chimneys symmetric about CENTER_Z, placed toward back
        float[] chZ = { CENTER_Z - 9f, CENTER_Z + 9f };
        float   chX = BACK_X - 10f;  // deep inside roof footprint
        float   chBaseY = Y_1F_TOP + 0.35f;

        var root    = new GameObject("Struct_Chimneys");
        var matW    = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Perron_Granite.mat");
        var matCap  = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        Undo.RegisterCreatedObjectUndo(root, "Chimneys");

        foreach (float cz in chZ)
        {
            float midY = chBaseY + CH_H * 0.5f;
            // Shaft
            var shaft = Slab(root.transform, $"Chimney_Shaft_{cz:0}",
                new Vector3(chX, midY, cz),
                new Vector3(CH_W, CH_H, CH_W));
            if (matW != null) shaft.GetComponent<Renderer>().sharedMaterial = matW;

            // Neck band
            var neck = Slab(root.transform, $"Chimney_Neck_{cz:0}",
                new Vector3(chX, chBaseY + CH_H - 0.45f, cz),
                new Vector3(CH_W + 0.12f, 0.22f, CH_W + 0.12f));
            if (matCap != null) neck.GetComponent<Renderer>().sharedMaterial = matCap;

            // Cap
            var cap = Slab(root.transform, $"Chimney_Cap_{cz:0}",
                new Vector3(chX, chBaseY + CH_H + CAP_H * 0.5f, cz),
                new Vector3(CAP_W, CAP_H, CAP_W));
            if (matCap != null) cap.GetComponent<Renderer>().sharedMaterial = matCap;

            // Pots (2 smaller stacks on top of cap)
            foreach (float pz in new[] { cz - CH_W * 0.22f, cz + CH_W * 0.22f })
            {
                Slab(root.transform, $"Chimney_Pot_{cz:0}_{pz:0}",
                    new Vector3(chX, chBaseY + CH_H + CAP_H + 0.28f, pz),
                    new Vector3(0.24f, 0.55f, 0.24f));
            }
        }

        MarkDirty();
        Debug.Log($"[AutoFurn] ✓ {chZ.Length} chimneys added to roof");
    }

    [MenuItem("VoD/Auto/0 — RUN ALL (materials + furniture + cornice + sills + ornaments)")]
    static void RunAll()
    {
        CreateMaterials();
        FurnishAllRooms();
        PopulateCorridors();
        AddCornices();
        AddWindowSills();
        AddFacadeOrnaments();
        AddStringCourses();
        AddRusticatedBase();
        AddWindowCrownsAndModillions();
        AddBayWindows();
        AddChimneys();
        Debug.Log("[AutoFurn] ══ All steps complete ══");
    }

    // ── Pilasters (trụ giả) — vertical strips flanking each window bay ────────
    static void AddPilasters()
    {
        const float PW = 0.50f, PD = 0.15f;  // pilaster width (Z) and depth (X)
        const float CAP_H = 0.40f, CAP_EXTRA = 0.12f; // capital height, extra width

        // Pilasters flanking windows — entrance bay (CENTER_Z) stays open, no pilaster there
        // Front & back faces (Z-spaced): 6 symmetric positions skip CENTER_Z
        float[] pilZ = {
            SIDE_Z_LO + 4.5f, SIDE_Z_LO + 12f,
            CENTER_Z - 7f,     CENTER_Z + 7f,
            SIDE_Z_HI - 12f,   SIDE_Z_HI - 4.5f
        };

        // floor slabs: (bottomY, topY)
        (float bot, float top)[] floors = { (32.05f, Y_GF_TOP), (39.30f, Y_1F_TOP) };

        var root    = new GameObject("Struct_PilasterGroup");
        var matWall = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Wall_Ochre.mat");
        var matCap  = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        Undo.RegisterCreatedObjectUndo(root, "Pilasters");

        foreach (var (bot, top) in floors)
        {
            float h  = top - bot;
            float cy = bot + h * 0.5f;

            // Front face (pilZ spaced along Z)
            foreach (float pz in pilZ)
            {
                var shaft = Slab(root.transform, $"Pil_F_{pz:0}_{bot:0}",
                    new Vector3(FRONT_X - PD * 0.5f, cy, pz),
                    new Vector3(PD, h, PW));
                if (matWall != null) shaft.GetComponent<Renderer>().sharedMaterial = matWall;
                var cap = Slab(root.transform, $"Cap_F_{pz:0}_{bot:0}",
                    new Vector3(FRONT_X - (PD + CAP_EXTRA) * 0.5f, top - CAP_H * 0.5f, pz),
                    new Vector3(PD + CAP_EXTRA, CAP_H, PW + CAP_EXTRA));
                if (matCap != null) cap.GetComponent<Renderer>().sharedMaterial = matCap;
            }

            // Back face — symmetric (mirror on X axis)
            foreach (float pz in pilZ)
            {
                var shaft = Slab(root.transform, $"Pil_B_{pz:0}_{bot:0}",
                    new Vector3(BACK_X + PD * 0.5f, cy, pz),
                    new Vector3(PD, h, PW));
                if (matWall != null) shaft.GetComponent<Renderer>().sharedMaterial = matWall;
                var cap = Slab(root.transform, $"Cap_B_{pz:0}_{bot:0}",
                    new Vector3(BACK_X + (PD + CAP_EXTRA) * 0.5f, top - CAP_H * 0.5f, pz),
                    new Vector3(PD + CAP_EXTRA, CAP_H, PW + CAP_EXTRA));
                if (matCap != null) cap.GetComponent<Renderer>().sharedMaterial = matCap;
            }
        }
    }

    // ── Quoins (đá góc) — alternating stone blocks at all 4 corners ──────────
    static void AddQuoins()
    {
        const float QD    = 0.35f;   // protrusion from wall
        const float GTALL = 0.65f;   // tall quoin height
        const float GSHRT = 0.40f;   // short quoin height
        const float GAP   = 0.05f;   // gap between quoins

        var root   = new GameObject("Struct_QuoinGroup");
        var matQ   = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Perron_Granite.mat");
        Undo.RegisterCreatedObjectUndo(root, "Quoins");

        // 4 corners: (face-X, face-Z, sign-X, sign-Z)
        var corners = new (float fx, float fz, float sx, float sz, string tag)[]
        {
            (FRONT_X, SIDE_Z_LO, -1f, -1f, "FL"),
            (FRONT_X, SIDE_Z_HI, -1f,  1f, "FH"),
            (BACK_X,  SIDE_Z_LO,  1f, -1f, "BL"),
            (BACK_X,  SIDE_Z_HI,  1f,  1f, "BH"),
        };

        float startY = 32.05f, endY = Y_1F_TOP;

        foreach (var (fx, fz, sx, sz, tag) in corners)
        {
            float y    = startY;
            bool  tall = true;
            int   idx  = 0;
            while (y < endY - 0.1f)
            {
                float qh   = tall ? GTALL : GSHRT;
                float longW = tall ? 1.60f : 1.10f;
                float shrtW = tall ? 1.10f : 1.60f;
                float cy    = y + qh * 0.5f;

                // Quoin on the X-facing wall
                var qx = Slab(root.transform, $"Q_{tag}_X{idx}",
                    new Vector3(fx + sx * QD * 0.5f, cy, fz),
                    new Vector3(QD, qh, shrtW));
                if (matQ != null) qx.GetComponent<Renderer>().sharedMaterial = matQ;

                // Quoin on the Z-facing wall (interlocks with X piece)
                var qz = Slab(root.transform, $"Q_{tag}_Z{idx}",
                    new Vector3(fx, cy, fz + sz * QD * 0.5f),
                    new Vector3(longW, qh, QD));
                if (matQ != null) qz.GetComponent<Renderer>().sharedMaterial = matQ;

                y   += qh + GAP;
                tall = !tall;
                idx++;
            }
        }
    }

    // ── Entrance pediment (đầu hồi cổng) — triangular gable above portal ─────
    static void AddEntrancePediment()
    {
        const float PED_W = 7.5f;   // base width (Z)
        const float PED_H = 2.4f;   // triangle height (Y)
        const float PED_D = 0.28f;  // protrusion from wall (X)
        float posX   = FRONT_X - PED_D * 0.5f;
        float baseY  = Y_1F_TOP + 0.45f;  // sits atop 1F cornice
        float half   = PED_W * 0.5f;

        var root   = new GameObject("Struct_EntrancePediment");
        var matW   = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Cornice_White.mat");
        Undo.RegisterCreatedObjectUndo(root, "Pediment");

        // 1. Entablature — wide horizontal base of the pediment
        Slab(root.transform, "Ped_Entablature",
             new Vector3(posX, baseY + 0.18f, CENTER_Z),
             new Vector3(PED_D, 0.35f, PED_W + 0.6f));

        // 2. Left raking cornice (angled line from bottom-left to apex)
        float rakeLen = Mathf.Sqrt(PED_H * PED_H + half * half);
        var   leftDir = new Vector3(0f, PED_H, half).normalized;
        var   leftMid = new Vector3(posX, baseY + PED_H * 0.5f, CENTER_Z - half * 0.5f);
        var   lRake   = Slab(root.transform, "Ped_RakeLeft", leftMid, new Vector3(PED_D - 0.06f, 0.22f, rakeLen));
        lRake.transform.rotation = Quaternion.FromToRotation(Vector3.forward, leftDir);

        // 3. Right raking cornice (mirror)
        var  rightDir = new Vector3(0f, PED_H, -half).normalized;
        var  rightMid = new Vector3(posX, baseY + PED_H * 0.5f, CENTER_Z + half * 0.5f);
        var  rRake    = Slab(root.transform, "Ped_RakeRight", rightMid, new Vector3(PED_D - 0.06f, 0.22f, rakeLen));
        rRake.transform.rotation = Quaternion.FromToRotation(Vector3.forward, rightDir);

        // 4. Tympanum fill — flat wall panel inside the triangle (wall-coloured)
        var matTym = AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Wall_Ochre.mat");
        var tym    = Slab(root.transform, "Ped_Tympanum",
                          new Vector3(FRONT_X - 0.05f, baseY + PED_H * 0.45f, CENTER_Z),
                          new Vector3(0.08f, PED_H * 0.9f, PED_W * 0.78f));
        if (matTym != null) tym.GetComponent<Renderer>().sharedMaterial = matTym;

        // 5. Apex finial — decorative urn (small cylinder stack)
        Slab(root.transform, "Ped_Finial_Base",
             new Vector3(posX, baseY + PED_H + 0.22f, CENTER_Z),
             new Vector3(PED_D + 0.12f, 0.18f, 0.55f));
        Slab(root.transform, "Ped_Finial_Neck",
             new Vector3(posX, baseY + PED_H + 0.50f, CENTER_Z),
             new Vector3(PED_D,         0.30f, 0.30f));
        Slab(root.transform, "Ped_Finial_Cap",
             new Vector3(posX, baseY + PED_H + 0.72f, CENTER_Z),
             new Vector3(PED_D + 0.08f, 0.14f, 0.45f));

        // Apply white to all except tympanum (already assigned above)
        if (matW != null)
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                if (r.sharedMaterial == null || r.sharedMaterial != matTym)
                    r.sharedMaterial = matW;
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

    static GameObject Slab(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        return go;
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
