using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// VoD / Create Missing Geo — tạo geometry từ Unity primitives cho phần không có GLB
/// Menu: VoD/Create/...
public static class VoD_CreateMissingGeo
{
    const string MAT = "Assets/_Project/Materials/";

    // ── Helpers ────────────────────────────────────────────────────────────────
    static Material M(string name) =>
        AssetDatabase.LoadAssetAtPath<Material>(MAT + name + ".mat");

    static GameObject Cube(string n, GameObject p, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = n;
        go.transform.SetParent(p.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject Cyl(string n, GameObject p, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = n;
        go.transform.SetParent(p.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 1. IRON FENCE SECTION (1 đoạn 3m — nhân bản và xoay để lắp viền vườn)
    // ══════════════════════════════════════════════════════════════════════════
    [MenuItem("VoD/Create/Iron Fence Section (3m)")]
    public static void CreateIronFenceSection()
    {
        if (!RequireChapter1()) return;

        var iron = M("Mat_Iron_Fence");

        var root = new GameObject("Arch_Fence_Section");
        root.transform.position = new Vector3(-14f, 0f, 24f); // mặt tiền trái

        // 2 trụ đứng (đầu và cuối đoạn 3m)
        Cyl("Post_A", root, new Vector3(-1.5f, 1.05f, 0f), new Vector3(0.09f, 1.05f, 0.09f), iron);
        Cyl("Post_B", root, new Vector3( 1.5f, 1.05f, 0f), new Vector3(0.09f, 1.05f, 0.09f), iron);

        // 3 thanh ngang
        Cube("Rail_Top",  root, new Vector3(0f, 1.85f, 0f), new Vector3(3.0f, 0.045f, 0.045f), iron);
        Cube("Rail_Mid",  root, new Vector3(0f, 1.10f, 0f), new Vector3(3.0f, 0.045f, 0.045f), iron);
        Cube("Rail_Bot",  root, new Vector3(0f, 0.35f, 0f), new Vector3(3.0f, 0.045f, 0.045f), iron);

        // 9 con tiện đứng (cách nhau 0.3m, từ -1.2→+1.2)
        for (int i = 0; i < 9; i++)
        {
            float xPos = -1.35f + i * 0.3375f;
            var pick = Cyl($"Picket_{i:D2}", root, new Vector3(xPos, 0.95f, 0f),
                           new Vector3(0.028f, 0.95f, 0.028f), iron);
            // Đỉnh nhọn: cube nhỏ xoay 45°
            var tip = Cube($"Tip_{i:D2}", pick, new Vector3(0f, 1.11f, 0f),
                           new Vector3(0.045f, 0.09f, 0.045f), iron);
            tip.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        // BoxCollider tổng
        var col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 1.05f, 0f);
        col.size   = new Vector3(3.0f, 2.1f, 0.12f);

        Finish(root, "[VoD] Iron Fence Section (3m) created. Duplicate + snap để lắp dọc viền vườn.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 2. PERRON — bậc tam cấp 5 bậc mặt tiền villa
    // ══════════════════════════════════════════════════════════════════════════
    [MenuItem("VoD/Create/Perron (Front Steps 5 bậc)")]
    public static void CreatePerron()
    {
        if (!RequireChapter1()) return;

        if (GameObject.Find("Arch_Perron") != null)
        { Debug.Log("[VoD] Arch_Perron already exists."); return; }

        var granite = M("Mat_Perron_Granite");
        var stone   = M("Mat_Wall_Stone");

        var root = new GameObject("Arch_Perron");
        root.transform.position = new Vector3(-11f, 0f, 7.5f); // mặt tiền

        // 5 bậc: mỗi bậc cao 0.2m, rộng giảm dần
        float[] widths = { 5.6f, 5.0f, 4.4f, 3.8f, 3.2f };
        for (int i = 0; i < 5; i++)
        {
            float y = i * 0.2f + 0.1f;
            float z = i * 0.22f;
            Cube($"Step_{i + 1}", root, new Vector3(0f, y, z),
                 new Vector3(widths[i], 0.2f, 0.42f), granite);
        }

        // 2 cột trụ perron (Indochine — hình chữ nhật, không tròn)
        var pillarMat = stone;
        Cube("Pillar_L", root, new Vector3(-2.2f, 1.9f, 0.9f), new Vector3(0.45f, 3.8f, 0.45f), pillarMat);
        Cube("Pillar_R", root, new Vector3( 2.2f, 1.9f, 0.9f), new Vector3(0.45f, 3.8f, 0.45f), pillarMat);

        // Mái che perron (mỏng ngang)
        Cube("Canopy", root, new Vector3(0f, 3.85f, 0.3f), new Vector3(5.2f, 0.18f, 1.5f), granite);

        var col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.5f, 0.55f);
        col.size   = new Vector3(5.6f, 1.0f, 1.1f);

        Finish(root, "[VoD] Arch_Perron created at (-11, 0, 7.5) — 5 bậc + 2 cột + mái che.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 3. HORROR PROPS — nến, mảnh gương, ván đổ (dùng để dress từng phòng)
    // ══════════════════════════════════════════════════════════════════════════
    [MenuItem("VoD/Create/Horror Props Cluster (Select room first)")]
    public static void CreateHorrorCluster()
    {
        if (!RequireChapter1()) return;

        var wood   = M("Mat_Wood_Furniture");
        var mirror = M("Mat_Mirror");
        var iron   = M("Mat_Iron_Fence");
        var decay  = M("Mat_Wall_Decay");

        // Đặt tại vị trí selected object, hoặc origin nếu không chọn
        Vector3 origin = Selection.activeGameObject != null
            ? Selection.activeGameObject.transform.position + new Vector3(2f, 0f, 0f)
            : new Vector3(-10f, 0f, -10f);

        var cluster = new GameObject("Prop_HorrorCluster");
        cluster.transform.position = origin;

        // — Cụm nến 3 cái ——————————————————————————————————
        var candleGrp = new GameObject("Candle_Group");
        candleGrp.transform.SetParent(cluster.transform, false);
        candleGrp.transform.localPosition = Vector3.zero;

        var wax = M("Mat_Cornice_White") ?? M("Mat_Wall_Interior_Cream");
        float[] cx = { 0f, 0.12f, -0.09f };
        float[] ch = { 0.25f, 0.18f, 0.30f };
        for (int i = 0; i < 3; i++)
        {
            var c = Cyl($"Candle_{i}", candleGrp,
                        new Vector3(cx[i], ch[i] * 0.5f, 0f),
                        new Vector3(0.038f, ch[i] * 0.5f, 0.038f), wax);
            // Bấc: cube tí hon
            Cube($"Wick_{i}", c, new Vector3(0f, 1.08f, 0f),
                 new Vector3(0.012f, 0.055f, 0.012f), iron);
        }
        var holderBase = Cube("Holder_Base", candleGrp, new Vector3(0f, 0.04f, 0f),
                              new Vector3(0.32f, 0.07f, 0.18f), iron);

        // — Mảnh gương vỡ đứng nghiêng ————————————————————
        var shardGrp = new GameObject("Mirror_Shard_Group");
        shardGrp.transform.SetParent(cluster.transform, false);
        shardGrp.transform.localPosition = new Vector3(0.8f, 0f, 0.3f);

        float[] sw = { 0.28f, 0.15f, 0.10f };
        float[] sh2 = { 0.45f, 0.28f, 0.18f };
        float[] sry = { 12f, -25f, 40f };
        float[] srz = { -8f, 15f, 25f };
        for (int i = 0; i < 3; i++)
        {
            var shard = Cube($"Shard_{i}", shardGrp,
                             new Vector3(i * 0.18f, sh2[i] * 0.5f, 0f),
                             new Vector3(sw[i], sh2[i], 0.012f), mirror);
            shard.transform.localRotation = Quaternion.Euler(srz[i], sry[i], 0f);
        }

        // — Ván gỗ đổ (tạo cảm giác phòng hoang phế) ——————
        var plankGrp = new GameObject("Plank_Group");
        plankGrp.transform.SetParent(cluster.transform, false);
        plankGrp.transform.localPosition = new Vector3(-0.6f, 0f, 0.5f);

        float[,] planks = {
            { 0f,   0.025f, 0f,  1.4f, 0.05f, 0.14f,  15f },
            { 0.2f, 0.055f, 0.2f,1.1f, 0.04f, 0.12f, -8f  },
            {-0.1f, 0.045f, 0.3f,0.9f, 0.04f, 0.10f,  35f  },
        };
        for (int i = 0; i < 3; i++)
        {
            var pl = Cube($"Plank_{i}", plankGrp,
                          new Vector3(planks[i,0], planks[i,1], planks[i,2]),
                          new Vector3(planks[i,3], planks[i,4], planks[i,5]),
                          wood);
            pl.transform.localRotation = Quaternion.Euler(0f, planks[i,6], 0f);
        }

        // — Giấy tờ rải rác (Quad nằm ngang) —————————————
        var paperMat = M("Mat_Wall_Interior_Cream") ?? decay;
        for (int i = 0; i < 4; i++)
        {
            var paper = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paper.name = $"Prop_Paper_{i}";
            paper.transform.SetParent(cluster.transform, false);
            paper.transform.localPosition = new Vector3(
                Random.Range(-0.5f, 0.5f), 0.005f, Random.Range(-0.4f, 0.4f));
            paper.transform.localRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            paper.transform.localScale    = new Vector3(
                Random.Range(0.12f, 0.22f), Random.Range(0.15f, 0.28f), 1f);
            Object.DestroyImmediate(paper.GetComponent<MeshCollider>());
            if (paperMat != null) paper.GetComponent<Renderer>().sharedMaterial = paperMat;
        }

        Finish(cluster, $"[VoD] Horror Props Cluster created at {origin}. Unpack và phân phối theo phòng.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 4. TƯỜNG NGĂN PHÒNG — tạo nhanh vách tường nội thất (nếu cần thêm phòng)
    // ══════════════════════════════════════════════════════════════════════════
    [MenuItem("VoD/Create/Interior Wall Segment (4×3m)")]
    public static void CreateInteriorWall()
    {
        if (!RequireChapter1()) return;

        var mat = M("Mat_Wall_Interior_Cream");

        var root = new GameObject("Wall_Int_Segment");
        root.transform.position = Selection.activeGameObject != null
            ? Selection.activeGameObject.transform.position + new Vector3(0f, 1.5f, 0f)
            : new Vector3(-15f, 1.5f, -5f);

        Cube("Wall_Body", root, Vector3.zero, new Vector3(4.0f, 3.0f, 0.22f), mat);

        // Phào chỉ trên và dưới
        var cornice = M("Mat_Cornice_White");
        Cube("Cornice_Top", root, new Vector3(0f,  1.58f, 0f), new Vector3(4.0f, 0.12f, 0.24f), cornice);
        Cube("Cornice_Bot", root, new Vector3(0f, -1.55f, 0f), new Vector3(4.0f, 0.10f, 0.24f), cornice);

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(4.0f, 3.0f, 0.22f);

        Finish(root, "[VoD] Wall_Int_Segment (4×3m) created. Duplicate + rotate để chia phòng.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ★ Chạy tất cả
    // ══════════════════════════════════════════════════════════════════════════
    [MenuItem("VoD/Create/★ Create All Missing Geo")]
    public static void CreateAll()
    {
        if (!RequireChapter1()) return;
        CreateIronFenceSection();
        CreatePerron();
        Debug.Log("[VoD] ★ All missing geo created. Horror cluster: chạy per-room khi cần.");
    }

    // ── Shared utils ───────────────────────────────────────────────────────────
    static bool RequireChapter1()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.path.Contains("Chapter1")) return true;
        Debug.LogError("[VoD] Mở Chapter1.unity trước.");
        return false;
    }

    static void Finish(GameObject go, string msg)
    {
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = go;
        Debug.Log(msg);
    }
}
