using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public static class VoD_SetupPSX
{
    const string SHADER_PATH = "Assets/_Project/Shaders/PSX_PostProcess.shader";
    const string MAT_PATH    = "Assets/_Project/Materials/Mat_PSX_PostProcess.mat";

    [MenuItem("VoD/PSX Style/1 – Create PSX Material")]
    public static void CreateMaterial()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(SHADER_PATH);
        if (shader == null)
        {
            Debug.LogError("[VoD] Không tìm thấy PSX_PostProcess.shader tại " + SHADER_PATH);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<Material>(MAT_PATH) != null)
        {
            Debug.Log("[VoD] Mat_PSX_PostProcess.mat đã tồn tại.");
            return;
        }

        var mat = new Material(shader) { name = "Mat_PSX_PostProcess" };
        AssetDatabase.CreateAsset(mat, MAT_PATH);
        AssetDatabase.SaveAssets();
        Debug.Log("[VoD] Mat_PSX_PostProcess.mat created. Tiếp theo: thêm PSX_RendererFeature vào URP Renderer Data.");
        EditorGUIUtility.PingObject(mat);
    }

    [MenuItem("VoD/PSX Style/2 – Select URP Renderer Data (add feature manually)")]
    public static void SelectRendererData()
    {
        // Tìm UniversalRendererData trong project
        foreach (var guid in AssetDatabase.FindAssets("t:UniversalRendererData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj  = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
                Debug.Log($"[VoD] Đang chọn Renderer Data: {path}");
                Debug.Log("[VoD] Trong Inspector → Add Renderer Feature → PSX Renderer Feature → kéo Mat_PSX_PostProcess vào Material.");
                return;
            }
        }
        Debug.LogWarning("[VoD] Không tìm thấy UniversalRendererData. Tìm thủ công trong Assets/_Project/Settings/.");
    }
}
