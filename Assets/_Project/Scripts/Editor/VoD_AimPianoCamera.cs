using UnityEngine;
using UnityEditor;

public static class VoD_AimPianoCamera
{
    [MenuItem("VoD/Fix/Aim CameraZoomTarget At Piano Keys")]
    public static void Aim()
    {
        GameObject zoomTarget = GameObject.Find("CameraZoomTarget");
        GameObject pianoBody = GameObject.Find("Piano_Body");

        if (zoomTarget == null || pianoBody == null)
        {
            Debug.LogError("[VoD] Thiếu object: CameraZoomTarget/Piano_Body.");
            return;
        }

        // Gộp bounds của TOÀN BỘ renderer con (thân đàn + mọi phím) để lấy tâm khối đàn thật,
        // không chỉ riêng hàng phím.
        var renderers = pianoBody.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError("[VoD] Piano_Body không có MeshRenderer con nào.");
            return;
        }

        Bounds wholeBounds = renderers[0].bounds;
        foreach (var r in renderers) wholeBounds.Encapsulate(r.bounds);
        Vector3 pianoCenter = wholeBounds.center;

        Vector3 dir = (pianoCenter - zoomTarget.transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

        Undo.RecordObject(zoomTarget.transform, "Aim Piano Camera");
        zoomTarget.transform.rotation = lookRot;

        EditorUtility.SetDirty(zoomTarget.transform);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[VoD] Đã xoay CameraZoomTarget nhìn thẳng vào tâm TOÀN BỘ khối đàn {pianoCenter} (size khối {wholeBounds.size}) " +
                  $"từ vị trí hiện tại {zoomTarget.transform.position}. Chạy lại menu này bất kỳ lúc nào sau khi bạn kéo lại vị trí để tự căn góc.");
    }
}
