// XOA SAU KHI CHAY XONG
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class AddHierarchySeparators
{
    [MenuItem("VoD/Add Hierarchy Separators + Fix NavMesh")]
    static void Run()
    {
        var scene = SceneManager.GetSceneByName("Chapter1");

        // Xóa _NavMesh sai vị trí
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == "_NavMesh") { Object.DestroyImmediate(r); break; }

        // Tạo separators và chèn trước từng anchor
        // Format: ("= TÊN SEPARATOR", "object name anchor đứng sau")
        var separators = new (string sep, string anchor)[]
        {
            ("= ── CAMERAS & LIGHTING ──",  "Main Camera"),
            ("= ── STRUCTURE & FLOORS ──",  "StructuralCore"),
            ("= ── EXTERIOR ──",            "FrontYard"),
            ("= ── ROOMS ──",               "Room_Kitchen"),
            ("= ── GAMEPLAY ──",            "PlayerSpawn"),
            ("= ── AI & GHOST ──",          "_AI"),
            ("= ── AUDIO ──",               "_Audio"),
            ("= ── UI ──",                  "_UI"),
        };

        foreach (var (sep, anchor) in separators)
        {
            // Bỏ qua nếu separator đã tồn tại
            bool exists = false;
            foreach (var r in scene.GetRootGameObjects())
                if (r.name == sep) { exists = true; break; }
            if (exists) continue;

            // Tìm anchor
            int anchorIdx = -1;
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                if (roots[i].name == anchor) { anchorIdx = i; break; }
            if (anchorIdx < 0) continue;

            var go = new GameObject(sep);
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetSiblingIndex(anchorIdx);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VoD] Hierarchy separators added. _NavMesh removed.");
    }
}
