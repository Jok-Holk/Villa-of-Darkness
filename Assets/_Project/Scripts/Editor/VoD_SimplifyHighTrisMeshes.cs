using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;

/// Giảm tris hàng loạt cho mesh nặng trong scene bằng UnityMeshSimplifier --
/// gom theo MESH GỐC (không theo từng GameObject) vì nhiều object (vd cụm
/// DryBranch_*) DÙNG CHUNG 1 mesh nguồn, giảm 1 lần là áp dụng cho mọi instance
/// cùng lúc. Lưu mesh đã giảm thành asset MỚI (không đè lên mesh import gốc từ
/// FBX), gán lại cho mọi MeshFilter đang tham chiếu mesh đó. Có ghi manifest
/// (gốc -> đã giảm) để revert lại đúng khi 1 mesh bị giảm quá tay/méo hình.
public static class VoD_SimplifyHighTrisMeshes
{
    private const string OutputFolder = "Assets/_Project/Models/_Simplified";
    private const string ManifestPath = OutputFolder + "/_manifest.json";

    [MenuItem("VoD/Optimize/Enable Read-Write On High-Tris Meshes (Run Before Simplify)")]
    public static void EnableReadWriteOnHighTrisMeshes()
    {
        var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var uniqueMeshes = filters.Where(mf => mf.sharedMesh != null).Select(mf => mf.sharedMesh).Distinct();

        var pathsToFix = new HashSet<string>();
        foreach (var mesh in uniqueMeshes)
        {
            int tris = mesh.triangles.Length / 3;
            if (mesh.isReadable) continue;
            if (GetQualityForTriCount(tris) <= 0f) continue;

            var path = AssetDatabase.GetAssetPath(mesh);
            if (!string.IsNullOrEmpty(path)) pathsToFix.Add(path);
        }

        int fixedCount = 0;
        foreach (var path in pathsToFix)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null || importer.isReadable) continue;
            importer.isReadable = true;
            importer.SaveAndReimport();
            fixedCount++;
        }

        Debug.Log($"[VoD] Đã bật Read/Write Enabled + reimport cho {fixedCount} file model. Chạy lại 'Simplify High-Tris Meshes' để xử lý nốt {pathsToFix.Count} mesh này.");
    }

    [MenuItem("VoD/Optimize/Simplify High-Tris Meshes (Auto Quality By Size)")]
    public static void SimplifyHighTrisMeshes()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Models", "_Simplified");
        }

        var manifest = LoadManifest();

        var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var byMesh = new Dictionary<Mesh, List<MeshFilter>>();
        foreach (var mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            // Bỏ qua mesh ĐÃ nằm trong _Simplified (đã giảm ở lần chạy trước rồi, khỏi giảm chồng).
            var curPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
            if (curPath != null && curPath.StartsWith(OutputFolder)) continue;

            if (!byMesh.TryGetValue(mf.sharedMesh, out var list))
            {
                list = new List<MeshFilter>();
                byMesh[mf.sharedMesh] = list;
            }
            list.Add(mf);
        }

        long trisBefore = 0, trisAfter = 0;
        int meshesProcessed = 0, instancesAffected = 0, skippedTooSmall = 0, skippedError = 0;

        foreach (var kv in byMesh.OrderByDescending(k => k.Key.triangles.Length))
        {
            var mesh = kv.Key;
            var owners = kv.Value;
            int tris = mesh.triangles.Length / 3;

            float quality = GetQualityForTriCount(tris);
            if (quality <= 0f) { skippedTooSmall++; continue; }

            Mesh newMesh;
            try
            {
                var simplifier = new MeshSimplifier();
                simplifier.Initialize(mesh);
                simplifier.SimplifyMesh(quality);
                newMesh = simplifier.ToMesh();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VoD] Lỗi khi giảm tris mesh '{mesh.name}': {e.Message} -- bỏ qua, giữ nguyên mesh gốc.");
                skippedError++;
                continue;
            }
            newMesh.name = mesh.name + "_Simplified";

            string safeName = MakeSafeFileName(newMesh.name);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{OutputFolder}/{safeName}.asset");
            AssetDatabase.CreateAsset(newMesh, assetPath);

            // Ghi vào manifest: đường dẫn mesh gốc (FBX) + tên mesh gốc -> path mesh đã giảm,
            // để có thể revert đúng nếu sau này thấy mesh bị giảm quá tay/méo hình.
            string origAssetPath = AssetDatabase.GetAssetPath(mesh);
            manifest[assetPath] = new ManifestEntry { originalAssetPath = origAssetPath, originalMeshName = mesh.name };

            int newTris = newMesh.triangles.Length / 3;
            trisBefore += (long)tris * owners.Count;
            trisAfter += (long)newTris * owners.Count;
            meshesProcessed++;

            foreach (var mf in owners)
            {
                Undo.RecordObject(mf, "VoD Simplify Mesh");
                mf.sharedMesh = newMesh;
                EditorUtility.SetDirty(mf);
                instancesAffected++;
            }

            Debug.Log($"[VoD] {mesh.name}: {tris:N0} -> {newTris:N0} tris (quality {quality:P0}), áp dụng cho {owners.Count} object.");
        }

        SaveManifest(manifest);
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"[VoD] XONG: {meshesProcessed} mesh gốc đã giảm tris, áp dụng cho {instancesAffected} object trong scene. " +
                  $"Tổng tris (tính theo số instance): {trisBefore:N0} -> {trisAfter:N0} (giảm {trisBefore - trisAfter:N0}, {(trisBefore > 0 ? 100.0 * (trisBefore - trisAfter) / trisBefore : 0):F1}%). " +
                  $"Bỏ qua {skippedTooSmall} mesh nhỏ + {skippedError} mesh lỗi khi giảm. Mesh mới lưu ở '{OutputFolder}/'. " +
                  "Nếu thấy vật thể nào bị méo/mất hình dạng: chọn object đó trong Hierarchy rồi chạy 'VoD/Optimize/Revert Simplified Mesh On Selected'. Ctrl+S sau khi kiểm tra Scene ổn.");
    }

    // Chọn 1 hoặc nhiều object trong Hierarchy bị giảm tris quá tay (méo hình như
    // sofa/ghế bọc nệm) rồi chạy cái này để trả lại mesh gốc -- ưu tiên đọc từ
    // manifest (chính xác 100%), nếu không có (mesh bị giảm từ trước khi có
    // manifest) thì fallback tìm theo tên (bỏ hậu tố "_Simplified").
    [MenuItem("VoD/Optimize/Revert Simplified Mesh On Selected")]
    public static void RevertSimplifiedMeshOnSelected()
    {
        var manifest = LoadManifest();
        int reverted = 0, failed = 0;

        // Tìm MeshFilter cả ở CON của object chọn (không chỉ đúng object được
        // chọn) -- object FBX import thường root chỉ có Transform, mesh nằm ở
        // node con, chọn root nên GetComponent thẳng bị bỏ sót im lặng.
        var allFilters = Selection.gameObjects
            .SelectMany(go => go.GetComponentsInChildren<MeshFilter>(true))
            .Distinct();

        foreach (var mf in allFilters)
        {
            var go = mf.gameObject;
            if (mf.sharedMesh == null) continue;

            string curPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
            if (string.IsNullOrEmpty(curPath) || !curPath.StartsWith(OutputFolder))
            {
                Debug.LogWarning($"[VoD] '{go.name}': mesh hiện tại không phải bản đã giảm tris (không nằm trong '{OutputFolder}') -- bỏ qua.");
                continue;
            }

            Mesh original = null;
            if (manifest.TryGetValue(curPath, out var entry) && !string.IsNullOrEmpty(entry.originalAssetPath))
            {
                var loaded = AssetDatabase.LoadAllAssetsAtPath(entry.originalAssetPath).OfType<Mesh>();
                original = loaded.FirstOrDefault(m => m.name == entry.originalMeshName) ?? loaded.FirstOrDefault();
            }

            if (original == null)
            {
                // Fallback: AssetDatabase.FindAssets với tên KHÔNG khớp mesh nằm
                // lồng trong FBX (search text không index tên sub-asset) -- test
                // thực tế trên sofa_big/sofa_small cho thấy fallback cũ luôn ra 0
                // kết quả. Dùng "t:Mesh" liệt kê hết rồi tự so tên chính xác trong
                // code (chậm hơn nhưng đáng tin cậy -- chỉ chạy khi cần revert tay).
                string wantName = mf.sharedMesh.name.EndsWith("_Simplified")
                    ? mf.sharedMesh.name.Substring(0, mf.sharedMesh.name.Length - "_Simplified".Length)
                    : mf.sharedMesh.name;
                original = FindMeshByExactNameSlow(wantName);
            }

            if (original == null)
            {
                Debug.LogError($"[VoD] '{go.name}': không tìm được mesh gốc để revert -- báo tên object này, mình tìm tay.");
                failed++;
                continue;
            }

            Undo.RecordObject(mf, "VoD Revert Simplified Mesh");
            mf.sharedMesh = original;
            EditorUtility.SetDirty(mf);
            reverted++;
            Debug.Log($"[VoD] '{go.name}': đã trả về mesh gốc '{original.name}' ({original.triangles.Length / 3:N0} tris).");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log($"[VoD] Revert xong: {reverted} object trả về mesh gốc, {failed} object không tìm được (báo tên cụ thể để xử lý tay). Ctrl+S.");
    }

    private static Mesh FindMeshByExactNameSlow(string exactName)
    {
        var guids = AssetDatabase.FindAssets("t:Mesh");
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (p.StartsWith(OutputFolder)) continue;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(p))
            {
                if (obj is Mesh m && m.name == exactName) return m;
            }
        }
        return null;
    }

    // Ngưỡng + độ giữ chi tiết theo số tris gốc. GIẢM BỚT mức mạnh tay sau khi
    // sofa/ghế bọc nệm bị méo hình ở quality thấp (mesh cong/nhăn nhiều tam giác
    // nhỏ dễ vỡ dạng khi giảm sâu) -- ưu tiên an toàn hơn, vẫn còn giảm đáng kể.
    private static float GetQualityForTriCount(int tris)
    {
        if (tris > 15000) return 0.35f;
        if (tris > 8000) return 0.45f;
        if (tris > 4000) return 0.55f;
        if (tris > 1500) return 0.65f;
        return 0f;
    }

    private static string MakeSafeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name;
    }

    [System.Serializable]
    private class ManifestEntry
    {
        public string originalAssetPath;
        public string originalMeshName;
    }

    [System.Serializable]
    private class ManifestFile
    {
        public List<string> simplifiedPaths = new List<string>();
        public List<ManifestEntry> entries = new List<ManifestEntry>();
    }

    private static Dictionary<string, ManifestEntry> LoadManifest()
    {
        var dict = new Dictionary<string, ManifestEntry>();
        if (!File.Exists(ManifestPath)) return dict;
        try
        {
            var json = File.ReadAllText(ManifestPath);
            var file = JsonUtility.FromJson<ManifestFile>(json);
            if (file?.simplifiedPaths != null)
            {
                for (int i = 0; i < file.simplifiedPaths.Count && i < file.entries.Count; i++)
                    dict[file.simplifiedPaths[i]] = file.entries[i];
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[VoD] Không đọc được manifest cũ ({e.Message}), bắt đầu manifest mới.");
        }
        return dict;
    }

    private static void SaveManifest(Dictionary<string, ManifestEntry> dict)
    {
        var file = new ManifestFile();
        foreach (var kv in dict)
        {
            file.simplifiedPaths.Add(kv.Key);
            file.entries.Add(kv.Value);
        }
        File.WriteAllText(ManifestPath, JsonUtility.ToJson(file, true));
        AssetDatabase.ImportAsset(ManifestPath);
    }
}
