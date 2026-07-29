using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// Dò toàn bộ scene tìm vật thể có nhiều tris nhất -- ghi ra file .txt ở gốc
/// project (thay vì chỉ Debug.Log, dễ bị cắt/timeout khi query nhiều qua MCP).
public static class VoD_ScanHighTrisObjects
{
    private const string ReportPath = "high_tris_objects_report.txt";
    private const int TopN = 60;

    [MenuItem("VoD/Optimize/Scan High-Tris Objects (Report To File)")]
    public static void ScanHighTris()
    {
        var entries = new System.Collections.Generic.List<(string path, int tris, bool active, GameObject go)>();

        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mf.sharedMesh == null) continue;
            int tris = mf.sharedMesh.triangles.Length / 3;
            entries.Add((GetPath(mf.transform), tris, mf.gameObject.activeInHierarchy, mf.gameObject));
        }
        foreach (var smr in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (smr.sharedMesh == null) continue;
            int tris = smr.sharedMesh.triangles.Length / 3;
            entries.Add((GetPath(smr.transform), tris, smr.gameObject.activeInHierarchy, smr.gameObject));
        }

        long totalTris = entries.Sum(e => (long)e.tris);
        int totalObjects = entries.Count;
        int activeCount = entries.Count(e => e.active);
        long activeTris = entries.Where(e => e.active).Sum(e => (long)e.tris);

        var top = entries.OrderByDescending(e => e.tris).Take(TopN).ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== VoD High-Tris Objects Report ===");
        sb.AppendLine($"Tổng {totalObjects} object có mesh, tổng {totalTris:N0} tris (trong đó {activeCount} object đang ACTIVE, {activeTris:N0} tris active).");
        sb.AppendLine($"Top {top.Count} theo tris (kể cả inactive, đánh dấu [inactive]):");
        sb.AppendLine();

        int rank = 1;
        foreach (var e in top)
        {
            string tag = e.active ? "" : " [inactive]";
            sb.AppendLine($"{rank,3}. {e.tris,9:N0} tris{tag}  -- {e.path}");
            rank++;
        }

        var fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ReportPath);
        File.WriteAllText(fullPath, sb.ToString());

        Debug.Log($"[VoD] Đã ghi report ra '{ReportPath}' (ở gốc project) -- {totalObjects} object, {totalTris:N0} tris tổng ({activeTris:N0} tris đang active). Top 5 nặng nhất: " +
                  string.Join(" | ", top.Take(5).Select(e => $"{e.path} ({e.tris:N0})")));
    }

    private static string GetPath(Transform t)
    {
        var path = t.name;
        var p = t.parent;
        while (p != null) { path = p.name + "/" + path; p = p.parent; }
        return path;
    }
}
