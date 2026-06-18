using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using System.Linq;

public static class VoD_OptimizePerf
{
    // ── Menu items ───────────────────────────────────────────────────────────
    [MenuItem("VoD/Optimize/1 – GPU Instancing on PH Materials")]
    public static void EnableGPUInstancing()
    {
        int count = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[]{"Assets/_Project/Materials"}))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            if (!mat.enableInstancing)
            {
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[VoD] GPU Instancing enabled on {count} materials.");
    }

    [MenuItem("VoD/Optimize/2 – LOD Groups for PH Trees")]
    public static void AddLODGroupsToPHTrees()
    {
        // Tìm tất cả PH tree objects trong scene
        string[] bigTrees = { "PH_Jacaranda", "PH_FirTree_01", "PH_IslandTree_01",
                               "PH_IslandTree_02", "PH_IslandTree_03" };
        int count = 0;
        foreach (var name in bigTrees)
        {
            var go = GameObject.Find(name);
            if (go == null) continue;
            if (go.GetComponent<LODGroup>() != null) continue;

            var lodGroup = go.AddComponent<LODGroup>();
            var renderers = go.GetComponentsInChildren<Renderer>();

            // LOD0 = full, LOD1 = still full (không có simplified mesh — chỉ set crossfade)
            var lods = new LOD[]
            {
                new LOD(0.15f, renderers),   // visible đầy đủ khi > 15% screen height
                new LOD(0.05f, new Renderer[0]), // culled khi < 5%
            };
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
            EditorUtility.SetDirty(go);
            count++;
        }
        Debug.Log($"[VoD] LOD Groups added to {count} PH trees.");
    }

    [MenuItem("VoD/Optimize/3 – URP Low Quality Asset (px1)")]
    public static void CreateLowQualityURPAsset()
    {
        // Thử nhiều cách tìm URP asset (project dùng per-quality-level pipeline)
        var currentPipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (currentPipeline == null)
            currentPipeline = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (currentPipeline == null)
        {
            // Fallback: tìm PC_RPAsset hoặc bất kỳ URP asset nào trong project
            foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                currentPipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(p);
                if (currentPipeline != null) break;
            }
        }
        if (currentPipeline == null)
        {
            Debug.LogError("[VoD] Không tìm thấy URP Asset nào trong project.");
            return;
        }

        // Tạo bản sao low-quality
        const string outPath = "Assets/_Project/Settings/URP_LowQuality.asset";
        if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(outPath) != null)
        {
            Debug.Log("[VoD] URP_LowQuality.asset đã tồn tại.");
            return;
        }

        var lowAsset = Object.Instantiate(currentPipeline);
        lowAsset.name = "URP_LowQuality";

        // Giảm shadow
        lowAsset.shadowDistance          = 25f;
        lowAsset.shadowCascadeCount      = 1;     // 1 cascade thay vì 4
        lowAsset.renderScale             = 0.75f;  // render 75% → composite lên full (px ~0.75x)
        lowAsset.msaaSampleCount         = 1;      // tắt MSAA
        lowAsset.mainLightShadowmapResolution = 512;
        lowAsset.supportsHDR             = false;

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Settings"))
            AssetDatabase.CreateFolder("Assets/_Project", "Settings");

        AssetDatabase.CreateAsset(lowAsset, outPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[VoD] URP_LowQuality.asset tạo xong tại {outPath}.");
        Debug.Log("[VoD] Thêm Quality Level 'Low' trong Edit → Project Settings → Quality → Add Quality Level → assign URP_LowQuality làm Render Pipeline Asset.");
    }

    [MenuItem("VoD/Optimize/4 – Flag Static Renderers for Batching")]
    public static void FlagStaticBatching()
    {
        int count = 0;
        var scene = EditorSceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var go = r.gameObject;
                if (go.GetComponent<Rigidbody>() != null) continue; // skip dynamic
                if (go.GetComponent<Animator>()  != null) continue; // skip animated

                var flags = GameObjectUtility.GetStaticEditorFlags(go);
                if ((flags & StaticEditorFlags.BatchingStatic) == 0)
                {
                    GameObjectUtility.SetStaticEditorFlags(go,
                        flags | StaticEditorFlags.BatchingStatic);
                    EditorUtility.SetDirty(go);
                    count++;
                }
            }
        }
        Debug.Log($"[VoD] Batching Static flagged on {count} renderers.");
    }

    [MenuItem("VoD/Optimize/5 – Remove Mesh Colliders on PH Props")]
    public static void RemoveMeshCollidersOnProps()
    {
        // PH models import với MeshCollider phức tạp — dùng BoxCollider đơn giản hơn
        int removed = 0, replaced = 0;
        var scene = EditorSceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mc in root.GetComponentsInChildren<MeshCollider>(true))
            {
                var go = mc.gameObject;
                if (!go.name.StartsWith("PH_")) continue;

                var mesh = mc.sharedMesh;
                bool isComplex = mesh != null && mesh.vertexCount > 256;
                if (!isComplex) continue;

                // Thay bằng BoxCollider bao ngoài
                var bounds = mc.bounds;
                Object.DestroyImmediate(mc);
                var box = go.AddComponent<BoxCollider>();
                box.center = go.transform.InverseTransformPoint(bounds.center);
                box.size   = go.transform.InverseTransformVector(bounds.size);
                EditorUtility.SetDirty(go);
                removed++; replaced++;
            }
        }
        Debug.Log($"[VoD] {removed} complex MeshColliders replaced with BoxColliders on PH props.");
    }

    // Chạy tất cả 5 bước 1 lần
    [MenuItem("VoD/Optimize/★ Run All Optimizations")]
    public static void RunAll()
    {
        EnableGPUInstancing();
        AddLODGroupsToPHTrees();
        FlagStaticBatching();
        RemoveMeshCollidersOnProps();
        // NOTE: URP Low Quality asset cần tạo thủ công trong Quality Settings
        Debug.Log("[VoD] ★ Optimization pass complete. Nhớ tạo URP_LowQuality qua menu item 3.");
    }
}
