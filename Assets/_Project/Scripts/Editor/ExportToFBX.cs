// XOA SAU KHI CHAY XONG
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GLTFast.Export;

public class ExportToFBX
{
    static readonly string[] Skip = {
        "Main Camera", "Directional Light",
        "_Systems", "_AI", "_Audio", "_UI", "_NavMesh",
        "PlayerSpawn", "EventSystem", "Canvas",
        "GazeTrigger", "TriggerZone", "DelayEvent",
        "Zone_Delay", "Zone_CancelDelay", "Zone_Entry",
        "Mirror_Surface", "Plane",
        "GhostCube", "Player",
    };

    [MenuItem("VoD/Export Chapter1 to GLB (Blender)")]
    static async void Run()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Chapter1");

        var toExport = scene.GetRootGameObjects()
            .Where(r => !Skip.Contains(r.name) && !r.name.StartsWith("= ──"))
            .ToArray();

        // Ngoài Assets/ để Unity không auto-import
        string dir  = Path.Combine(Path.GetDirectoryName(Application.dataPath), "_BlenderExport");
        string path = Path.Combine(dir, "Chapter1_Geometry.glb");
        Directory.CreateDirectory(dir);

        Debug.Log($"[VoD] Exporting {toExport.Length} objects to GLB...");

        var export = new GameObjectExport();
        export.AddScene(toExport, "Chapter1");
        bool success = await export.SaveToFileAndDispose(path);

        AssetDatabase.Refresh();
        if (success)
        {
            Debug.Log($"[VoD] GLB exported → {path}");
            EditorUtility.RevealInFinder(path);
        }
        else
            Debug.LogError("[VoD] GLB export failed!");
    }
}
