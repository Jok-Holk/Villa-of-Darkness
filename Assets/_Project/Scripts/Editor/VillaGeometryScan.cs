using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/// <summary>
/// Quét hình học THẬT của tòa nhà và xuất báo cáo ra file để phân tích.
/// Không đoán hằng số — đọc bounds thực từ MeshRenderer.
/// Menu: VoD > Diagnostic > Scan Building Geometry
/// </summary>
public static class VillaGeometryScan
{
    const string OUT = "Assets/_Project/_geometry_report.txt";

    [MenuItem("VoD/Diagnostic/Scan Building Geometry")]
    public static void Scan()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== VILLA GEOMETRY SCAN ===");
        sb.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine();

        // ── 1. Root objects ────────────────────────────────────────────
        sb.AppendLine("--- ROOT OBJECTS (top-level) ---");
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var r in roots.OrderBy(g => g.name))
        {
            var b = GetHierarchyBounds(r);
            sb.AppendLine($"  {r.name,-28} children={r.transform.childCount,-4} " +
                          (b.HasValue ? FmtBounds(b.Value) : "(no renderers)"));
        }
        sb.AppendLine();

        // ── 2. Overall building bounds (all renderers) ─────────────────
        var all = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        sb.AppendLine($"--- TOTAL RENDERERS: {all.Length} ---");
        if (all.Length > 0)
        {
            var total = all[0].bounds;
            foreach (var r in all) total.Encapsulate(r.bounds);
            sb.AppendLine($"  WORLD BOUNDS: {FmtBounds(total)}");
            sb.AppendLine($"  min=({total.min.x:F1},{total.min.y:F1},{total.min.z:F1}) " +
                          $"max=({total.max.x:F1},{total.max.y:F1},{total.max.z:F1})");
        }
        sb.AppendLine();

        // ── 3. Floor planes (objects named Floor*) — detect Y levels ────
        sb.AppendLine("--- FLOOR OBJECTS (Y levels) ---");
        var floors = all.Where(r => r.gameObject.name.ToLower().Contains("floor")).ToList();
        var yGroups = floors.GroupBy(r => Mathf.Round(r.bounds.center.y * 2f) / 2f)
                            .OrderBy(g => g.Key);
        foreach (var g in yGroups)
        {
            var xs = g.SelectMany(r => new[] { r.bounds.min.x, r.bounds.max.x });
            var zs = g.SelectMany(r => new[] { r.bounds.min.z, r.bounds.max.z });
            sb.AppendLine($"  Y≈{g.Key,-7:F1} count={g.Count(),-3} " +
                          $"X[{xs.Min():F1}..{xs.Max():F1}] Z[{zs.Min():F1}..{zs.Max():F1}]");
        }
        sb.AppendLine();

        // ── 4. Wall objects — detect facade planes ─────────────────────
        sb.AppendLine("--- WALL / STRUCTURE OBJECTS ---");
        var walls = all.Where(r => {
            var n = r.gameObject.name.ToLower();
            return n.Contains("wall") || n.Contains("column") || n.Contains("structural") || n.Contains("pillar");
        }).ToList();
        sb.AppendLine($"  count={walls.Count}");
        foreach (var w in walls.OrderBy(r => r.bounds.center.z).Take(40))
            sb.AppendLine($"    {w.gameObject.name,-24} {FmtBounds(w.bounds)}");
        sb.AppendLine();

        // ── 5. EXISTING WINDOWS — current placement ────────────────────
        sb.AppendLine("--- EXISTING WINDOW OBJECTS ---");
        var wins = FindByNameContains("win").Concat(FindByNameContains("jalousie"))
                   .Concat(FindByNameContains("window")).Distinct().ToList();
        sb.AppendLine($"  count={wins.Count}");
        // Measure REAL rendered world size of the first actual window
        var sample = wins.FirstOrDefault(g => g.name.StartsWith("Win_"));
        if (sample != null)
        {
            var rends = sample.GetComponentsInChildren<MeshRenderer>();
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                sb.AppendLine($"  *** SAMPLE WINDOW '{sample.name}' RENDERED WORLD SIZE = " +
                              $"({b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2})  (target ~1.2w x 2.4h) ***");
            }
        }
        foreach (var w in wins.OrderBy(g => g.name).Take(6))
        {
            var p = w.transform.position;
            sb.AppendLine($"    {w.name,-26} pos=({p.x:F1},{p.y:F1},{p.z:F1}) " +
                          $"rotY={w.transform.eulerAngles.y:F0} scale={w.transform.localScale}");
        }
        sb.AppendLine();

        // ── 6. DOORS ───────────────────────────────────────────────────
        sb.AppendLine("--- EXISTING DOOR OBJECTS ---");
        var doors = FindByNameContains("door").ToList();
        foreach (var d in doors.OrderBy(g => g.name))
        {
            var p = d.transform.position;
            sb.AppendLine($"    {d.name,-26} pos=({p.x:F1},{p.y:F1},{p.z:F1}) rotY={d.transform.eulerAngles.y:F0}");
        }
        sb.AppendLine();

        // ── 7. "Sample" / leftover model-source objects ────────────────
        sb.AppendLine("--- POSSIBLE SAMPLE / SOURCE OBJECTS (Arch_*, named exactly like asset) ---");
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.transform.parent != null) continue;
            var n = go.name;
            if (n.StartsWith("Arch_") || n.StartsWith("Prop_") || n == "Arch_Window_Jalousie")
            {
                var p = go.transform.position;
                sb.AppendLine($"    {n,-30} pos=({p.x:F1},{p.y:F1},{p.z:F1}) <ROOT-LEVEL, likely leftover>");
            }
        }
        sb.AppendLine();

        // ── 8. EXTERIOR FACADE PLANES (precise) ────────────────────────
        sb.AppendLine("--- EXTERIOR FACADE ANALYSIS ---");
        // Only consider walls belonging to the house core (exclude fence at Z<3, kitchen wing X>55)
        var coreWalls = walls.Where(w => {
            var c = w.bounds.center;
            return c.z > 3f && c.x < 55f && c.x > 5f && c.y > 31f && c.y < 56f;
        }).ToList();

        // Front facade = walls with smallest Z, thin in Z (size.z < 1)
        var thinZ = coreWalls.Where(w => w.bounds.size.z < 1f).ToList();
        if (thinZ.Count > 0)
        {
            float frontZ = thinZ.Min(w => w.bounds.center.z);
            float backZ  = thinZ.Max(w => w.bounds.center.z);
            var frontWalls = thinZ.Where(w => Mathf.Abs(w.bounds.center.z - frontZ) < 1.5f).ToList();
            sb.AppendLine($"  FRONT facade Z ≈ {frontZ:F2}  (walls: {frontWalls.Count})");
            // report X coverage of front GF walls (Y 33-38) to find entrance/window gaps
            var frontGF = frontWalls.Where(w => w.bounds.center.y > 33f && w.bounds.center.y < 39f
                                            && w.bounds.size.x > 1f).OrderBy(w => w.bounds.min.x).ToList();
            sb.AppendLine("    Front GF solid wall X-spans (gaps = openings):");
            foreach (var w in frontGF)
                sb.AppendLine($"      X[{w.bounds.min.x:F1} .. {w.bounds.max.x:F1}]  {w.gameObject.name}");
            sb.AppendLine($"  BACK facade Z ≈ {backZ:F2}");
        }
        // Left/right = walls thin in X
        var thinX = coreWalls.Where(w => w.bounds.size.x < 1f).ToList();
        if (thinX.Count > 0)
        {
            float leftX  = thinX.Min(w => w.bounds.center.x);
            float rightX = thinX.Max(w => w.bounds.center.x);
            sb.AppendLine($"  LEFT facade  X ≈ {leftX:F2}  (walls thin-in-X: {thinX.Count})");
            sb.AppendLine($"  RIGHT facade X ≈ {rightX:F2}");
        }
        // Building footprint from GroundFloor
        var gf = GameObject.Find("GroundFloor");
        if (gf) {
            var b = GetHierarchyBounds(gf);
            if (b.HasValue) sb.AppendLine($"  FOOTPRINT (GroundFloor): X[{b.Value.min.x:F1}..{b.Value.max.x:F1}] " +
                                          $"Z[{b.Value.min.z:F1}..{b.Value.max.z:F1}] centerX={b.Value.center.x:F1} centerZ={b.Value.center.z:F1}");
        }
        sb.AppendLine();

        // ── 8b. -X FRONT FACADE OPENINGS (tìm cửa vào thật) ────────────────
        sb.AppendLine("--- -X FRONT FACADE (X≈10.7) GF WALL Z-SPANS (gaps = openings) ---");
        var frontXWalls = all.Where(r => {
            var c = r.bounds.center;
            return Mathf.Abs(c.x - 10.7f) < 2.5f && c.y > 33f && c.y < 39f
                   && r.bounds.size.z > 1f && r.bounds.size.x < 2f
                   && r.gameObject.name.ToLower().Contains("wall");
        }).OrderBy(r => r.bounds.min.z).ToList();
        foreach (var w in frontXWalls)
            sb.AppendLine($"    Z[{w.bounds.min.z:F1} .. {w.bounds.max.z:F1}]  {w.gameObject.name}");
        sb.AppendLine();

        // ── 9. SITE / EXTERIOR INVENTORY ───────────────────────────────────
        sb.AppendLine("--- SITE / EXTERIOR INVENTORY (vs front facade Z=7.78) ---");
        foreach (var gname in new[] { "_Exterior_Decor", "_Exterior_Arch", "_Perron", "StonePath", "FrontYard" })
        {
            var grp = GameObject.Find(gname);
            if (grp == null) { sb.AppendLine($"  {gname}: (không có)"); continue; }
            sb.AppendLine($"  {gname}: {grp.transform.childCount} con");
            foreach (Transform c in grp.transform)
            {
                var p = c.position;
                string where = p.z < 7.78f ? "SÂN TRƯỚC" : (p.z > 47.6f ? "sau nhà" : "TRONG/DƯỚI NHÀ ⚠");
                sb.AppendLine($"      {c.name,-20} ({p.x:F1},{p.y:F1},{p.z:F1})  [{where}]");
            }
        }
        // Property front edge (fence posts at very low Z)
        var fence = walls.Where(w => w.bounds.center.z < 3f && w.bounds.size.y > 3f).ToList();
        if (fence.Count > 0)
        {
            float fz = fence.Average(w => w.bounds.center.z);
            sb.AppendLine($"  HÀNG RÀO/MÉP ĐẤT trước: Z≈{fz:F2} ({fence.Count} cọc) → sân sâu ≈ {7.78f - fz:F1}m");
        }
        sb.AppendLine();

        File.WriteAllText(OUT, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"[VoD] Geometry report written to {OUT}\n\n{sb}");
    }

    [MenuItem("VoD/Diagnostic/View Front Facade")]
    public static void ViewFront()
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) { Debug.LogWarning("No SceneView"); return; }
        sv.pivot = new Vector3(31.7f, 40f, 7.5f);
        sv.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up); // nhìn theo +Z (vào mặt tiền)
        sv.size = 34f;
        sv.orthographic = false;
        sv.Repaint();
    }

    [MenuItem("VoD/Diagnostic/View Right Side")]
    public static void ViewRight()
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        sv.pivot = new Vector3(52.8f, 40f, 27.6f);
        sv.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up); // nhìn theo -X (vào cạnh phải)
        sv.size = 34f;
        sv.orthographic = false;
        sv.Repaint();
    }

    [MenuItem("VoD/Diagnostic/View Real Front (-X)")]
    public static void ViewFrontX()
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        sv.pivot = new Vector3(10.7f, 40f, 27.6f);
        sv.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up); // nhìn theo +X (vào mặt tiền -X)
        sv.size = 30f;
        sv.orthographic = false;
        sv.Repaint();
    }

    [MenuItem("VoD/Diagnostic/View Top (Site Plan)")]
    public static void ViewTop()
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        sv.pivot = new Vector3(31.7f, 33f, 27.6f);
        sv.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward); // nhìn thẳng xuống, +Z lên trên
        sv.size = 38f;
        sv.orthographic = true;
        sv.Repaint();
    }

    static IEnumerable<GameObject> FindByNameContains(string token)
    {
        token = token.ToLower();
        return Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                     .Where(g => g.name.ToLower().Contains(token));
    }

    static Bounds? GetHierarchyBounds(GameObject root)
    {
        var rends = root.GetComponentsInChildren<MeshRenderer>();
        if (rends.Length == 0) return null;
        var b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        return b;
    }

    static string FmtBounds(Bounds b) =>
        $"center=({b.center.x:F1},{b.center.y:F1},{b.center.z:F1}) size=({b.size.x:F1},{b.size.y:F1},{b.size.z:F1})";
}
