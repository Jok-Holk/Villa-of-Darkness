// XOA SAU KHI CHAY XONG
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;

public class SetupNavMesh
{
    [MenuItem("VoD/NavMesh - Full Setup + Bake")]
    static void RunAll()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Chapter1");

        // 1. Xóa _NavMesh cũ nếu tồn tại
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == "_NavMesh") { Object.DestroyImmediate(r); break; }

        // 2. Tạo NavMeshSurface mới — CollectObjects.All quét toàn scene
        var navRoot = new GameObject("_NavMesh");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(navRoot, scene);
        navRoot.transform.position = Vector3.zero;

        var surface = navRoot.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.agentTypeID    = 0; // Humanoid
        surface.minRegionArea  = 0.1f;

        // 3. Bake
        surface.BuildNavMesh();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VoD] NavMesh baked with CollectObjects.All");
    }
}
