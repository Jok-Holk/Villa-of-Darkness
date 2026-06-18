using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class VoD_CreateFountain
{
    // Đài phun nước đá 2 tầng — kiến trúc Đông Dương
    // Vị trí: sân trước villa, Z≈-12, X≈14 (gần giếng)
    [MenuItem("VoD/Create Stone Fountain")]
    public static void Create()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.path.Contains("Chapter1"))
        {
            Debug.LogError("[VoD] Mở Chapter1.unity trước.");
            return;
        }

        // ── Lấy hoặc tạo group ───────────────────────────────────────────────
        var extArch = GameObject.Find("_Exterior_Arch");

        var root = new GameObject("Prop_Fountain_Stone");
        root.transform.position = new Vector3(14.4f, 0f, -14f);
        if (extArch != null)
            root.transform.SetParent(extArch.transform, true);

        var mat = LoadOrCreateStoneMat();

        // ── Tầng dưới: bồn đá lớn ─────────────────────────────────────────
        // Rim (vành tròn): cylinder to, thấp
        var rimLow = MakeCylinder("Basin_Lower_Rim",  root, Vector3.zero,
            new Vector3(2.5f, 0.15f, 2.5f), mat);
        // Đáy bồn (nước đứng): cylinder nhỏ hơn, thấp
        var innerLow = MakeCylinder("Basin_Lower_Inner", root, new Vector3(0, 0.13f, 0),
            new Vector3(2.1f, 0.04f, 2.1f), mat);

        // ── Trụ giữa ─────────────────────────────────────────────────────────
        var pedestal = MakeCylinder("Pedestal", root, new Vector3(0, 0.15f, 0),
            new Vector3(0.35f, 0.55f, 0.35f), mat);

        // ── Tầng trên: bồn nhỏ ───────────────────────────────────────────────
        var rimHigh = MakeCylinder("Basin_Upper_Rim",  root, new Vector3(0, 0.70f, 0),
            new Vector3(1.2f, 0.12f, 1.2f), mat);
        var innerHigh = MakeCylinder("Basin_Upper_Inner", root, new Vector3(0, 0.81f, 0),
            new Vector3(0.95f, 0.03f, 0.95f), mat);

        // ── Đỉnh: đầu vòi nhỏ ────────────────────────────────────────────────
        var top = MakeCylinder("Spout_Top", root, new Vector3(0, 0.84f, 0),
            new Vector3(0.08f, 0.12f, 0.08f), mat);

        // ── Mặt nước (Quad phẳng) ─────────────────────────────────────────────
        var water = GameObject.CreatePrimitive(PrimitiveType.Quad);
        water.name = "Water_Surface";
        water.transform.SetParent(root.transform, false);
        water.transform.localPosition = new Vector3(0, 0.165f, 0);
        water.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        water.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        Object.DestroyImmediate(water.GetComponent<MeshCollider>());

        var waterMat = LoadOrCreateWaterMat();
        water.GetComponent<Renderer>().sharedMaterial = waterMat;

        // ── Collider tổng ─────────────────────────────────────────────────────
        var col = root.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.4f, 0);
        col.radius = 1.25f;
        col.height = 0.9f;

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = root;

        Debug.Log("[VoD] Stone Fountain created at (14.4, 0, -14). Assign stone texture trong Inspector nếu cần.");
    }

    static GameObject MakeCylinder(string n, GameObject parent, Vector3 localPos,
                                    Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = n;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static Material LoadOrCreateStoneMat()
    {
        // Thử load từ PolyHaven mossy_cobblestone (đã tải)
        const string texBase = "Assets/_Project/Textures/PolyHaven/Ground/mossy_cobblestone";
        var diff   = AssetDatabase.LoadAssetAtPath<Texture2D>(texBase + "_diff_2k.jpg");
        var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(texBase + "_nor_gl_2k.jpg");
        var arm    = AssetDatabase.LoadAssetAtPath<Texture2D>(texBase + "_arm_2k.jpg");

        var matPath = "Assets/_Project/Materials/Mat_Stone_Fountain.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null) return existing;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = "Mat_Stone_Fountain";
        if (diff   != null) mat.SetTexture("_BaseMap",    diff);
        if (normal != null) mat.SetTexture("_BumpMap",    normal);
        if (arm    != null) mat.SetTexture("_MetallicGlossMap", arm);
        mat.SetFloat("_Smoothness", 0.15f);
        mat.SetFloat("_Metallic",   0.0f);

        // Tiling lớn hơn vì bồn to
        mat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));

        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    static Material LoadOrCreateWaterMat()
    {
        var matPath = "Assets/_Project/Materials/Mat_Water_Fountain.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null) return existing;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = "Mat_Water_Fountain";
        // Nước đen tù đọng — không phải nước trong
        mat.SetColor("_BaseColor", new Color(0.08f, 0.10f, 0.10f, 0.85f));
        mat.SetFloat("_Smoothness", 0.92f);
        mat.SetFloat("_Metallic",   0.0f);
        // Transparent
        mat.SetFloat("_Surface", 1f); // Transparent
        mat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite",    0);
        mat.renderQueue = 3000;

        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        return mat;
    }
}
