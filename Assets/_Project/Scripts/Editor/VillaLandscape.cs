#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Generates all exterior landscaping: lawn, parterre garden, fountain, lamp posts,
/// perimeter fence, servant house, palms, hedges, paths.
/// Run VoD/Landscape/0 after VoD/Auto/0 and VoD/Villa/9.
/// </summary>
public static class VillaLandscape
{
    // ── Geometry constants (must match VillaAutoFurnish + VillaArchitectureFix) ──
    const float FRONT_X   = 10.7f;
    const float BACK_X    = 52.8f;
    const float SIDE_Z_LO = 7.78f;
    const float SIDE_Z_HI = 47.56f;
    const float CENTER_X  = (FRONT_X + BACK_X) * 0.5f;
    const float CENTER_Z  = (SIDE_Z_LO + SIDE_Z_HI) * 0.5f;
    const float Y_GND     = 32.44f;  // ground surface Y
    const float GATE_X    = -50f;    // from BuildGrandApproach

    // Property boundary (2m outside villa walls)
    const float PROP_X_MIN = GATE_X - 2f;
    const float PROP_X_MAX = BACK_X + 18f;
    const float PROP_Z_MIN = SIDE_Z_LO - 8f;
    const float PROP_Z_MAX = SIDE_Z_HI + 8f;

    const string M_PATH  = "Assets/_Project/Materials";
    const string NATURE  = "Assets/_Project/Models/Props/Nature/";
    const string K_PATH  = "Assets/_Project/Models/Props/Furniture/Kenney";

    // ════════════════════════════════════════════════════════════════════════
    //  MATERIALS
    // ════════════════════════════════════════════════════════════════════════

    static Material GetOrCreateMat(string name, Color col, float rough = 0.8f, float metal = 0f)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) return null;
        string path = $"{M_PATH}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path) ?? new Material(shader);
        mat.name = name;
        mat.SetColor("_BaseColor", col);
        mat.SetFloat("_Smoothness", 1f - rough);
        mat.SetFloat("_Metallic", metal);
        if (!AssetDatabase.Contains(mat)) AssetDatabase.CreateAsset(mat, path);
        else EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material GrassMat()   => GetOrCreateMat("Mat_Garden_Grass",   new Color(0.21f, 0.40f, 0.16f), 0.95f);
    static Material GravelMat()  => GetOrCreateMat("Mat_Garden_Gravel",  new Color(0.72f, 0.70f, 0.65f), 0.90f);
    static Material WaterMat()   => GetOrCreateMat("Mat_Garden_Water",   new Color(0.18f, 0.38f, 0.68f), 0.05f);
    static Material FlowerYMat() => GetOrCreateMat("Mat_Garden_FlowerY", new Color(0.96f, 0.82f, 0.12f), 0.85f);
    static Material FlowerRMat() => GetOrCreateMat("Mat_Garden_FlowerR", new Color(0.82f, 0.12f, 0.12f), 0.85f);
    static Material FlowerWMat() => GetOrCreateMat("Mat_Garden_FlowerW", new Color(0.94f, 0.90f, 0.88f), 0.85f);
    static Material HedgeMat()   => GetOrCreateMat("Mat_Garden_Hedge",   new Color(0.16f, 0.32f, 0.12f), 0.92f);
    static Material IronMat()    => GetOrCreateMat("Mat_Iron_Fence",     new Color(0.08f, 0.08f, 0.10f), 0.60f, 0.85f);
    static Material GraniteMat() => AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Perron_Granite.mat")
                                     ?? GetOrCreateMat("Mat_Perron_Granite", new Color(0.53f, 0.53f, 0.53f), 0.40f);
    static Material OchreMat()   => AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Wall_Ochre.mat")
                                     ?? GetOrCreateMat("Mat_Wall_Ochre", new Color(0.83f, 0.63f, 0.26f), 0.70f);
    static Material TerracottaMat() => AssetDatabase.LoadAssetAtPath<Material>($"{M_PATH}/Mat_Roof_TerraCotta.mat")
                                     ?? GetOrCreateMat("Mat_Roof_TerraCotta", new Color(0.72f, 0.36f, 0.22f), 0.75f);

    // ════════════════════════════════════════════════════════════════════════
    //  MENU ITEMS
    // ════════════════════════════════════════════════════════════════════════

    [MenuItem("VoD/Landscape/1 — Lawn + Garden Ground")]
    public static void CreateLawnAndGround()
    {
        var old = GameObject.Find("_Landscape_Lawn");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("_Landscape_Lawn");
        Undo.RegisterCreatedObjectUndo(root, "Lawn");

        var grass   = GrassMat();
        var gravel  = GravelMat();
        var granite = GraniteMat();

        // ── Front yard: two grass panels flanking central driveway ──────────
        // Left panel (low-Z side)
        Slab(root, "Lawn_Front_Lo",
             Y_GND - 0.03f,
             (GATE_X + FRONT_X) * 0.5f, CENTER_Z - 8.5f,
             (FRONT_X - GATE_X), (CENTER_Z - 5f - SIDE_Z_LO - 2f),
             grass);
        // Right panel (high-Z side)
        Slab(root, "Lawn_Front_Hi",
             Y_GND - 0.03f,
             (GATE_X + FRONT_X) * 0.5f, CENTER_Z + 8.5f,
             (FRONT_X - GATE_X), (SIDE_Z_HI + 2f - CENTER_Z - 5f),
             grass);

        // ── Back yard: full width behind villa ───────────────────────────────
        float backCenterX = BACK_X + 9f;
        Slab(root, "Lawn_Back",
             Y_GND - 0.03f,
             backCenterX, CENTER_Z,
             18f, PROP_Z_MAX - PROP_Z_MIN,
             grass);

        // ── Side yards between villa and property boundary ───────────────────
        float sideW = SIDE_Z_LO - PROP_Z_MIN;
        Slab(root, "Lawn_Side_Lo",
             Y_GND - 0.03f,
             CENTER_X, PROP_Z_MIN + sideW * 0.5f,
             BACK_X - FRONT_X + 4f, sideW,
             grass);
        Slab(root, "Lawn_Side_Hi",
             Y_GND - 0.03f,
             CENTER_X, PROP_Z_MAX - sideW * 0.5f,
             BACK_X - FRONT_X + 4f, sideW,
             grass);

        // ── Gravel garden paths cross pattern in front yard ──────────────────
        float pathW = 1.4f;
        // Main axis extension (between driveway and garden beds)
        Slab(root, "Path_Axis_Lo",
             Y_GND,
             (GATE_X + FRONT_X) * 0.5f, CENTER_Z - 5.5f - pathW * 0.5f,
             FRONT_X - GATE_X, pathW,
             gravel);
        Slab(root, "Path_Axis_Hi",
             Y_GND,
             (GATE_X + FRONT_X) * 0.5f, CENTER_Z + 5.5f + pathW * 0.5f,
             FRONT_X - GATE_X, pathW,
             gravel);
        // Cross paths at X = -20 leading to side lawns
        float gardenX = GATE_X + (FRONT_X - GATE_X) * 0.55f; // ~X=-23
        Slab(root, "Path_Cross_Lo",
             Y_GND,
             gardenX, PROP_Z_MIN + sideW + (CENTER_Z - 5f - PROP_Z_MIN - sideW) * 0.5f,
             pathW, CENTER_Z - 5f - PROP_Z_MIN - sideW - 1f,
             gravel);
        Slab(root, "Path_Cross_Hi",
             Y_GND,
             gardenX, CENTER_Z + 5f + (PROP_Z_MAX - sideW - CENTER_Z - 5f) * 0.5f,
             pathW, PROP_Z_MAX - sideW - CENTER_Z - 5f - 1f,
             gravel);

        MarkDirty();
        Debug.Log("[VoD Landscape] ✓ Lawn + garden ground created");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Landscape/2 — Parterre Garden + Fountain")]
    public static void CreateParterreGarden()
    {
        var old = GameObject.Find("_Landscape_Parterre");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("_Landscape_Parterre");
        Undo.RegisterCreatedObjectUndo(root, "Parterre");

        float cx = GATE_X + (FRONT_X - GATE_X) * 0.55f;  // garden center X ≈ -23
        float cz = CENTER_Z;

        var gravel  = GravelMat();
        var flY     = FlowerYMat();
        var flR     = FlowerRMat();
        var flW     = FlowerWMat();
        var hedge   = HedgeMat();
        var granite = GraniteMat();
        var water   = WaterMat();
        var ochre   = OchreMat();

        // ── Central gravel plaza (8×8m) ──────────────────────────────────────
        Slab(root, "Parterre_Plaza",
             Y_GND + 0.02f, cx, cz, 16f, 16f, gravel);

        // ── 4 flower beds in quadrants ────────────────────────────────────────
        float[] bx = { cx - 4.5f, cx + 4.5f };
        float[] bz = { cz - 4.5f, cz + 4.5f };
        Material[] bedMats = { flY, flR, flW, flR };
        int mi = 0;
        foreach (float x in bx)
            foreach (float z in bz)
            {
                // Bed surround (hedge low box)
                Slab(root, $"Bed_Hedge_{x:0}_{z:0}",
                     Y_GND + 0.40f, x, z, 5.5f, 5.5f, hedge);
                // Flower fill
                Slab(root, $"Bed_Flower_{x:0}_{z:0}",
                     Y_GND + 0.55f, x, z, 4.8f, 4.8f, bedMats[mi % 4]);
                mi++;
            }

        // ── Corner obelisks (tall narrow granite columns) ─────────────────────
        foreach (float ox in bx)
            foreach (float oz in new[] { cz - 7.5f, cz + 7.5f })
            {
                var ob = Box(root, $"Obelisk_{ox:0}_{oz:0}",
                             new Vector3(ox * 0.55f + cx * 0.45f, Y_GND + 1.3f, oz),
                             new Vector3(0.22f, 2.6f, 0.22f));
                if (granite != null) ob.GetComponent<Renderer>().sharedMaterial = granite;
            }

        // ── Fountain ─────────────────────────────────────────────────────────
        // Pool rim
        var rim = Box(root, "Fountain_Rim",
                      new Vector3(cx, Y_GND + 0.20f, cz),
                      new Vector3(4.2f, 0.36f, 4.2f));
        if (granite != null) rim.GetComponent<Renderer>().sharedMaterial = granite;
        // Inner pool water
        var pool = Box(root, "Fountain_Water",
                       new Vector3(cx, Y_GND + 0.28f, cz),
                       new Vector3(3.5f, 0.12f, 3.5f));
        if (water != null) pool.GetComponent<Renderer>().sharedMaterial = water;
        // Pedestal
        var ped = Box(root, "Fountain_Pedestal",
                      new Vector3(cx, Y_GND + 0.70f, cz),
                      new Vector3(0.55f, 0.80f, 0.55f));
        if (granite != null) ped.GetComponent<Renderer>().sharedMaterial = granite;
        // Bowl
        var bowl = Box(root, "Fountain_Bowl",
                       new Vector3(cx, Y_GND + 1.22f, cz),
                       new Vector3(1.40f, 0.26f, 1.40f));
        if (granite != null) bowl.GetComponent<Renderer>().sharedMaterial = granite;
        // Upper pedestal
        var uped = Box(root, "Fountain_Upper_Ped",
                       new Vector3(cx, Y_GND + 1.60f, cz),
                       new Vector3(0.30f, 0.50f, 0.30f));
        if (granite != null) uped.GetComponent<Renderer>().sharedMaterial = granite;
        // Upper bowl (smaller)
        var ubowl = Box(root, "Fountain_Upper_Bowl",
                        new Vector3(cx, Y_GND + 1.98f, cz),
                        new Vector3(0.75f, 0.18f, 0.75f));
        if (granite != null) ubowl.GetComponent<Renderer>().sharedMaterial = granite;

        // ── Perimeter hedge ring around parterre ──────────────────────────────
        float hn = 0.90f;  // hedge height
        float ht = 0.55f;  // hedge thickness
        Slab(root, "PH_Front",
             Y_GND + hn * 0.5f, cx, cz - 8.5f,
             16.5f + ht * 2f, ht, hedge);
        Slab(root, "PH_Back",
             Y_GND + hn * 0.5f, cx, cz + 8.5f,
             16.5f + ht * 2f, ht, hedge);
        Slab(root, "PH_Left",
             Y_GND + hn * 0.5f, cx - 8.5f, cz,
             ht, 16.5f, hedge);
        Slab(root, "PH_Right",
             Y_GND + hn * 0.5f, cx + 8.5f, cz,
             ht, 16.5f, hedge);

        MarkDirty();
        Debug.Log("[VoD Landscape] ✓ Parterre garden + fountain created");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Landscape/3 — Lamp Posts + Lanterns")]
    public static void CreateLampPosts()
    {
        var old = GameObject.Find("_Landscape_Lamps");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("_Landscape_Lamps");
        Undo.RegisterCreatedObjectUndo(root, "LampPosts");

        var iron    = IronMat();
        var granite = GraniteMat();

        // Post positions: along driveway (7 pairs) + 2 at entrance perron + 2 at gate
        var posts = new System.Collections.Generic.List<(float x, float z)>();

        // Driveway posts — between allée trees, at Z = CENTER_Z ± 6
        for (int i = 0; i < 6; i++)
        {
            float px = GATE_X + 6f + i * 7.5f;
            posts.Add((px, CENTER_Z - 6.5f));
            posts.Add((px, CENTER_Z + 6.5f));
        }
        // Entrance pair flanking perron
        posts.Add((FRONT_X + 1f, CENTER_Z - 5f));
        posts.Add((FRONT_X + 1f, CENTER_Z + 5f));
        // Gate posts
        posts.Add((GATE_X + 1f, CENTER_Z - 7f));
        posts.Add((GATE_X + 1f, CENTER_Z + 7f));

        foreach (var (px, pz) in posts)
        {
            // Base plinth
            var plinth = Box(root, $"Lamp_Plinth_{px:0}_{pz:0}",
                             new Vector3(px, Y_GND + 0.15f, pz),
                             new Vector3(0.40f, 0.28f, 0.40f));
            if (granite != null) plinth.GetComponent<Renderer>().sharedMaterial = granite;

            // Post shaft
            var shaft = Box(root, $"Lamp_Shaft_{px:0}_{pz:0}",
                            new Vector3(px, Y_GND + 2.0f, pz),
                            new Vector3(0.10f, 3.6f, 0.10f));
            if (iron != null) shaft.GetComponent<Renderer>().sharedMaterial = iron;

            // Scroll arm (horizontal, very thin)
            var arm = Box(root, $"Lamp_Arm_{px:0}_{pz:0}",
                          new Vector3(px + 0.18f, Y_GND + 3.9f, pz),
                          new Vector3(0.36f, 0.06f, 0.06f));
            if (iron != null) arm.GetComponent<Renderer>().sharedMaterial = iron;

            // Lantern globe
            var globe = Box(root, $"Lamp_Globe_{px:0}_{pz:0}",
                            new Vector3(px + 0.36f, Y_GND + 4.05f, pz),
                            new Vector3(0.28f, 0.38f, 0.28f));
            // Globe stays unlit white for now (no emissive without extra setup)
            if (granite != null) globe.GetComponent<Renderer>().sharedMaterial = granite;
        }

        MarkDirty();
        Debug.Log($"[VoD Landscape] ✓ {posts.Count} lamp posts created");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Landscape/4 — Perimeter Iron Fence")]
    public static void CreatePerimeterFence()
    {
        var old = GameObject.Find("_Landscape_Fence");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("_Landscape_Fence");
        Undo.RegisterCreatedObjectUndo(root, "PerimeterFence");

        var iron    = IronMat();
        var granite = GraniteMat();
        const float POST_W = 0.10f, POST_H = 1.80f, POST_STEP = 2.2f;
        const float RAIL_H = 0.06f, PILLAR_W = 0.35f, PILLAR_H = 2.20f;

        // Sides: Z-facing fence runs along X (two runs per side)
        // Front fence at X = GATE_X - 1, from PROP_Z_MIN to (CENTER_Z-5) and (CENTER_Z+5) to PROP_Z_MAX (gate opening)
        FenceRun(root, "Fence_Front_Lo", GATE_X - 0.5f, Y_GND,
                 PROP_Z_MIN, CENTER_Z - 5.0f, false, POST_W, POST_H, RAIL_H, POST_STEP, iron, granite);
        FenceRun(root, "Fence_Front_Hi", GATE_X - 0.5f, Y_GND,
                 CENTER_Z + 5.0f, PROP_Z_MAX, false, POST_W, POST_H, RAIL_H, POST_STEP, iron, granite);

        // Back fence at X = PROP_X_MAX
        FenceRun(root, "Fence_Back", PROP_X_MAX, Y_GND,
                 PROP_Z_MIN, PROP_Z_MAX, false, POST_W, POST_H, RAIL_H, POST_STEP, iron, granite);

        // Side fence Lo (Z = PROP_Z_MIN), runs along X
        FenceRun(root, "Fence_Side_Lo", PROP_Z_MIN, Y_GND,
                 PROP_X_MIN, PROP_X_MAX, true, POST_W, POST_H, RAIL_H, POST_STEP, iron, granite);

        // Side fence Hi (Z = PROP_Z_MAX)
        FenceRun(root, "Fence_Side_Hi", PROP_Z_MAX, Y_GND,
                 PROP_X_MIN, PROP_X_MAX, true, POST_W, POST_H, RAIL_H, POST_STEP, iron, granite);

        MarkDirty();
        Debug.Log("[VoD Landscape] ✓ Perimeter iron fence created");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Landscape/5 — Back Garden + Servant House")]
    public static void CreateBackGarden()
    {
        var old = GameObject.Find("_Landscape_Back");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("_Landscape_Back");
        Undo.RegisterCreatedObjectUndo(root, "BackGarden");

        var ochre    = OchreMat();
        var roof     = TerracottaMat();
        var granite  = GraniteMat();
        var hedge    = HedgeMat();
        var gravel   = GravelMat();

        // ── Servant house ─────────────────────────────────────────────────────
        float shX = BACK_X + 10f;
        float shZ = SIDE_Z_LO + 7f;
        const float SH_W = 8.0f, SH_D = 5.5f, SH_H = 3.5f;

        // Walls (4 sides, thin)
        foreach (var (wx, wz, wsx, wsz) in new[]
        {
            (shX,          shZ - SH_D * 0.5f, SH_W, 0.28f),  // front wall
            (shX,          shZ + SH_D * 0.5f, SH_W, 0.28f),  // back wall
            (shX - SH_W * 0.5f, shZ, 0.28f, SH_D),           // left wall
            (shX + SH_W * 0.5f, shZ, 0.28f, SH_D),           // right wall
        })
        {
            var w = Box(root, $"SH_Wall_{wx:0}_{wz:0}",
                        new Vector3(wx, Y_GND + SH_H * 0.5f, wz),
                        new Vector3(wsx, SH_H, wsz));
            if (ochre != null) w.GetComponent<Renderer>().sharedMaterial = ochre;
        }
        // Roof slab (flat — servant house simpler than main villa)
        var ro = Box(root, "SH_Roof",
                     new Vector3(shX, Y_GND + SH_H + 0.15f, shZ),
                     new Vector3(SH_W + 0.3f, 0.25f, SH_D + 0.3f));
        if (roof != null) ro.GetComponent<Renderer>().sharedMaterial = roof;

        // ── Carriage house / stable ───────────────────────────────────────────
        float chX = BACK_X + 10f;
        float chZ = SIDE_Z_HI - 7f;
        const float CH_W = 10.0f, CH_D = 7.0f, CH_H = 4.0f;
        foreach (var (wx, wz, wsx, wsz) in new[]
        {
            (chX, chZ - CH_D * 0.5f, CH_W, 0.30f),
            (chX, chZ + CH_D * 0.5f, CH_W, 0.30f),
            (chX - CH_W * 0.5f, chZ, 0.30f, CH_D),
            (chX + CH_W * 0.5f, chZ, 0.30f, CH_D),
        })
        {
            var w = Box(root, $"CH_Wall_{wx:0}_{wz:0}",
                        new Vector3(wx, Y_GND + CH_H * 0.5f, wz),
                        new Vector3(wsx, CH_H, wsz));
            if (ochre != null) w.GetComponent<Renderer>().sharedMaterial = ochre;
        }
        // Gabled roof
        var ch_roof = Box(root, "CH_Roof",
                          new Vector3(chX, Y_GND + CH_H + 0.18f, chZ),
                          new Vector3(CH_W + 0.4f, 0.30f, CH_D + 0.4f));
        if (roof != null) ch_roof.GetComponent<Renderer>().sharedMaterial = roof;

        // ── Connecting path between buildings ─────────────────────────────────
        Slab(root, "Back_Path",
             Y_GND + 0.02f, BACK_X + 5f, CENTER_Z, 10f, 1.4f, gravel);

        // ── Low hedge along back property separation ──────────────────────────
        Slab(root, "Back_Hedge",
             Y_GND + 0.4f, PROP_X_MAX - 0.4f, CENTER_Z,
             0.55f, PROP_Z_MAX - PROP_Z_MIN, hedge);

        MarkDirty();
        Debug.Log("[VoD Landscape] ✓ Back garden + servant house + carriage house created");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Landscape/6 — Natural Planting (palms, hedges, flower borders)")]
    public static void PlantLandscaping()
    {
        var old = GameObject.Find("_Landscape_Plants");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("_Landscape_Plants");
        Undo.RegisterCreatedObjectUndo(root, "Planting");

        var hedge  = HedgeMat();
        var flY    = FlowerYMat();
        var flW    = FlowerWMat();

        // ── Side boundary hedges (property edge along Z sides) ────────────────
        float hedgeH = 1.6f;
        Slab(root, "Hedge_Side_Lo",
             Y_GND + hedgeH * 0.5f, CENTER_X, PROP_Z_MIN + 0.3f,
             BACK_X - FRONT_X + 5f, 0.6f, hedge);
        Slab(root, "Hedge_Side_Hi",
             Y_GND + hedgeH * 0.5f, CENTER_X, PROP_Z_MAX - 0.3f,
             BACK_X - FRONT_X + 5f, 0.6f, hedge);

        // ── Palm trees flanking villa sides (every 6m, just outside side walls) ─
        for (float tx = FRONT_X + 6f; tx < BACK_X - 4f; tx += 7.5f)
        {
            PlacePalm(root, tx, SIDE_Z_LO - 4.5f, $"Palm_Lo_{tx:0}");
            PlacePalm(root, tx, SIDE_Z_HI + 4.5f, $"Palm_Hi_{tx:0}");
        }

        // ── Frangipanis / flowering trees at parterre corners ─────────────────
        float cx = GATE_X + (FRONT_X - GATE_X) * 0.55f;
        foreach (float fz in new[] { CENTER_Z - 9f, CENTER_Z + 9f })
        {
            PlaceFloweringTree(root, cx - 9f, fz, $"FTree_L_{fz:0}");
            PlaceFloweringTree(root, cx + 9f, fz, $"FTree_R_{fz:0}");
        }

        // ── Flower borders along property fence (Lo + Hi sides) ──────────────
        Slab(root, "FlowerBorder_Lo",
             Y_GND + 0.10f, CENTER_X, SIDE_Z_LO - 2.0f,
             BACK_X - FRONT_X + 2f, 1.2f, flY);
        Slab(root, "FlowerBorder_Hi",
             Y_GND + 0.10f, CENTER_X, SIDE_Z_HI + 2.0f,
             BACK_X - FRONT_X + 2f, 1.2f, flW);

        // ── Vine/hedge strips directly against villa walls (ornamental) ───────
        float vineH = 2.5f;
        Slab(root, "Vine_Front",
             Y_GND + vineH * 0.5f, FRONT_X - 0.06f, CENTER_Z,
             0.08f, 30f, hedge);

        // ── Garden urns at villa entrance flanking perron ─────────────────────
        var granite = GraniteMat();
        foreach (float uz in new[] { CENTER_Z - 3.5f, CENTER_Z + 3.5f })
        {
            var urnBase = Box(root, $"Urn_Base_{uz:0}",
                              new Vector3(FRONT_X + 0.5f, Y_GND + 0.20f, uz),
                              new Vector3(0.45f, 0.35f, 0.45f));
            if (granite != null) urnBase.GetComponent<Renderer>().sharedMaterial = granite;
            var urnBody = Box(root, $"Urn_Body_{uz:0}",
                              new Vector3(FRONT_X + 0.5f, Y_GND + 0.72f, uz),
                              new Vector3(0.55f, 0.65f, 0.55f));
            if (granite != null) urnBody.GetComponent<Renderer>().sharedMaterial = granite;
            var urnRim = Box(root, $"Urn_Rim_{uz:0}",
                             new Vector3(FRONT_X + 0.5f, Y_GND + 1.12f, uz),
                             new Vector3(0.68f, 0.14f, 0.68f));
            if (granite != null) urnRim.GetComponent<Renderer>().sharedMaterial = granite;
            // Plant in urn
            Slab(root, $"Urn_Plant_{uz:0}",
                 Y_GND + 1.45f, FRONT_X + 0.5f, uz, 0.40f, 0.40f, hedge);
        }

        MarkDirty();
        Debug.Log("[VoD Landscape] ✓ Natural planting, palms, hedges, urns added");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Landscape/0 — RUN ALL LANDSCAPE")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("VoD Landscape",
            "Generate ALL landscape:\n1. Lawn + Ground\n2. Parterre + Fountain\n3. Lamp Posts\n4. Iron Fence\n5. Back Garden + Buildings\n6. Planting\n\nScene phải đang mở.",
            "Run", "Cancel")) return;

        CreateLawnAndGround();
        CreateParterreGarden();
        CreateLampPosts();
        CreatePerimeterFence();
        CreateBackGarden();
        PlantLandscaping();

        AssetDatabase.SaveAssets();
        Debug.Log("[VoD Landscape] ══ All landscape complete ══");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════════════

    static void PlacePalm(GameObject parent, float x, float z, string name)
    {
        // Try real model first
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NATURE + "Tree_Large.glb");
        if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NATURE + "Tree_Large.fbx");
        if (prefab != null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent.transform, true);
            go.transform.position = new Vector3(x, Y_GND, z);
            Undo.RegisterCreatedObjectUndo(go, "Palm");
            return;
        }
        // Fallback: primitive palm
        var trunk = Box(parent, $"{name}_Trunk",
                        new Vector3(x, Y_GND + 3.5f, z),
                        new Vector3(0.30f, 7.0f, 0.30f));
        trunk.GetComponent<Renderer>().sharedMaterial =
            GetOrCreateMat("Mat_Palm_Trunk", new Color(0.48f, 0.32f, 0.12f), 0.85f);
        // Canopy
        var canopy = Box(parent, $"{name}_Canopy",
                         new Vector3(x, Y_GND + 7.8f, z),
                         new Vector3(2.8f, 0.6f, 2.8f));
        canopy.GetComponent<Renderer>().sharedMaterial =
            GetOrCreateMat("Mat_Palm_Frond", new Color(0.16f, 0.40f, 0.10f), 0.90f);
    }

    static void PlaceFloweringTree(GameObject parent, float x, float z, string name)
    {
        var trunk = Box(parent, $"{name}_Trunk",
                        new Vector3(x, Y_GND + 2.0f, z),
                        new Vector3(0.22f, 4.0f, 0.22f));
        trunk.GetComponent<Renderer>().sharedMaterial =
            GetOrCreateMat("Mat_Palm_Trunk", new Color(0.48f, 0.32f, 0.12f), 0.85f);
        var bloom = Box(parent, $"{name}_Bloom",
                        new Vector3(x, Y_GND + 4.8f, z),
                        new Vector3(2.2f, 1.8f, 2.2f));
        bloom.GetComponent<Renderer>().sharedMaterial = FlowerYMat();
    }

    static void FenceRun(GameObject parent, string tag, float fixedCoord, float groundY,
                         float start, float end, bool runAlongX,
                         float postW, float postH, float railH, float step,
                         Material ironMat, Material graniteMat)
    {
        float railY1 = groundY + postH * 0.35f;
        float railY2 = groundY + postH * 0.80f;
        float railT  = 0.06f;

        float len = end - start;
        if (len <= 0) return;

        // Rails (two horizontal bars running the full length)
        for (int ri = 0; ri < 2; ri++)
        {
            float ry = ri == 0 ? railY1 : railY2;
            var rail = Box(parent, $"{tag}_Rail{ri}",
                runAlongX
                    ? new Vector3((start + end) * 0.5f, ry, fixedCoord)
                    : new Vector3(fixedCoord, ry, (start + end) * 0.5f),
                runAlongX
                    ? new Vector3(len, railT, railT)
                    : new Vector3(railT, railT, len));
            if (ironMat != null) rail.GetComponent<Renderer>().sharedMaterial = ironMat;
        }

        // Posts + major pillars every 5th post
        int n = Mathf.FloorToInt(len / step);
        for (int i = 0; i <= n; i++)
        {
            float t    = start + i * step;
            bool  major = i % 5 == 0;
            float pw   = major ? 0.28f : postW;
            float ph   = major ? postH + 0.30f : postH;

            var post = Box(parent, $"{tag}_Post{i}",
                runAlongX
                    ? new Vector3(t, groundY + ph * 0.5f, fixedCoord)
                    : new Vector3(fixedCoord, groundY + ph * 0.5f, t),
                runAlongX
                    ? new Vector3(pw, ph, pw)
                    : new Vector3(pw, ph, pw));
            if (major && graniteMat != null)
                post.GetComponent<Renderer>().sharedMaterial = graniteMat;
            else if (ironMat != null)
                post.GetComponent<Renderer>().sharedMaterial = ironMat;
        }
    }

    // Shorthand box creator (no collider)
    static GameObject Box(GameObject parent, string name, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, true);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        Undo.RegisterCreatedObjectUndo(go, name);
        return go;
    }

    // Slab helper (flat box at given Y)
    static void Slab(GameObject parent, string name, float y,
                     float cx, float cz, float sx, float sz, Material mat)
    {
        var go = Box(parent, name,
                     new Vector3(cx, y, cz),
                     new Vector3(sx, 0.06f, sz));
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void MarkDirty() =>
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
}
#endif
