using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// VoD / Organize Materials — di chuyển .mat vào subfolders theo nhóm:
///   Architecture/  ← tường, mái, sàn, phào chỉ, đá granit
///   Nature/        ← vườn, cỏ, nước, thân cây
///   Props/         ← sắt, gỗ, kính, gương
///   VFX/           ← PSX post-process, mirror RT
/// Dùng AssetDatabase.MoveAsset nên GUID được bảo toàn — scene refs không hỏng.
/// </summary>
public static class VoD_OrganizeMaterials
{
    const string MAT_ROOT = "Assets/_Project/Materials";

    // name (không đuôi .mat) → subfolder
    static readonly Dictionary<string, string> FolderMap = new()
    {
        // ── Architecture ──────────────────────────────────────────────────────
        { "Mat_Wall_Exterior_Ochre",   "Architecture" },
        { "Mat_Wall_Interior_Cream",   "Architecture" },
        { "Mat_Wall_Stone",            "Architecture" },
        { "Mat_Wall_Decay",            "Architecture" },
        { "Mat_Wall_Ochre",            "Architecture" },
        { "Mat_Roof_TerraCotta",       "Architecture" },
        { "Mat_Floor_CementTile",      "Architecture" },
        { "Mat_Floor_Courtyard",       "Architecture" },
        { "Mat_Floor_Teak",            "Architecture" },
        { "Mat_Cornice_White",         "Architecture" },
        { "Mat_Perron_Granite",        "Architecture" },
        // ── Nature ───────────────────────────────────────────────────────────
        { "Mat_Garden_Grass",          "Nature" },
        { "Mat_Garden_Hedge",          "Nature" },
        { "Mat_Garden_FlowerR",        "Nature" },
        { "Mat_Garden_FlowerW",        "Nature" },
        { "Mat_Garden_FlowerY",        "Nature" },
        { "Mat_Garden_Gravel",         "Nature" },
        { "Mat_Garden_Water",          "Nature" },
        { "Mat_Palm_Trunk",            "Nature" },
        { "Mat_Stone_Fountain",        "Nature" },
        { "Mat_Water_Fountain",        "Nature" },
        // ── Props ─────────────────────────────────────────────────────────────
        { "Mat_Iron_Fence",            "Props" },
        { "Mat_Iron_Railing",          "Props" },
        { "Mat_Metal_Railing",         "Props" },
        { "Mat_Jalousie_Green",        "Props" },
        { "Mat_Wood_Furniture",        "Props" },
        { "Mat_Wood_Teak",             "Props" },
        { "M_MirrorDisplay",           "Props" },
        // ── VFX ───────────────────────────────────────────────────────────────
        { "Mat_PSX_PostProcess",       "VFX" },
        { "Mat_Mirror",                "VFX" },
    };

    [MenuItem("VoD/Organize/★ Reorganize Materials into Subfolders")]
    public static void Run()
    {
        // Tạo subfolders nếu chưa có
        foreach (var sub in new[] { "Architecture", "Nature", "Props", "VFX" })
        {
            string fullPath = $"{MAT_ROOT}/{sub}";
            if (!AssetDatabase.IsValidFolder(fullPath))
                AssetDatabase.CreateFolder(MAT_ROOT, sub);
        }

        AssetDatabase.StartAssetEditing();
        int moved = 0, skipped = 0, notFound = 0;
        var log = new System.Text.StringBuilder("[VoD] Material reorganization:\n");

        try
        {
            foreach (var kv in FolderMap)
            {
                string name   = kv.Key;
                string sub    = kv.Value;
                string srcPath = $"{MAT_ROOT}/{name}.mat";
                string dstPath = $"{MAT_ROOT}/{sub}/{name}.mat";

                bool srcExists = AssetDatabase.LoadAssetAtPath<Material>(srcPath) != null;
                if (!srcExists)
                {
                    if (AssetDatabase.LoadAssetAtPath<Material>(dstPath) != null)
                    { log.AppendLine($"  SKIP (already in {sub}/): {name}"); skipped++; }
                    else
                    { log.AppendLine($"  NOT FOUND: {name}"); notFound++; }
                    continue;
                }

                var err = AssetDatabase.MoveAsset(srcPath, dstPath);
                if (string.IsNullOrEmpty(err))
                { log.AppendLine($"  → {sub}/{name}"); moved++; }
                else
                { log.AppendLine($"  ERROR {name}: {err}"); }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        log.AppendLine($"\nKết quả: {moved} moved, {skipped} already placed, {notFound} not found.");
        Debug.Log(log.ToString());

        if (moved > 0)
            Debug.Log("[VoD] Xong. VoD_FinalPatch sẽ tự tìm material trong subfolders qua FindAssets fallback.");
    }

    [MenuItem("VoD/Organize/List Materials (check tên trước khi di chuyển)")]
    public static void ListMaterials()
    {
        var guids = AssetDatabase.FindAssets("t:Material", new[] { MAT_ROOT });
        var sb    = new System.Text.StringBuilder($"[VoD] {guids.Length} materials in {MAT_ROOT}:\n");
        foreach (var g in guids)
            sb.AppendLine("  " + AssetDatabase.GUIDToAssetPath(g));
        Debug.Log(sb.ToString());
    }
}
