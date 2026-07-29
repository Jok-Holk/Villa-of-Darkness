using UnityEditor;
using UnityEngine;

// Hiện thêm dòng "Đang lưu (PlayerPrefs)" đọc trực tiếp CheckpointManager.CurrentStage -- Force Stage chỉ
// là Ô LỆNH ÉP (input), không tự phản ánh tiến độ thật đang lưu, nên cần hiển thị riêng để biết ngay vừa
// test xong đang ở stage mấy mà không cần đoán. Kèm nút bấm nhanh cho tiện.
[CustomEditor(typeof(CheckpointDebugTool))]
[CanEditMultipleObjects]
public class CheckpointDebugToolEditor : Editor
{
    private SerializedProperty _forceStageProp;

    private void OnEnable()
    {
        _forceStageProp = serializedObject.FindProperty("forceStage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trạng thái PlayerPrefs thật", EditorStyles.boldLabel);

        bool hasCheckpoint = CheckpointManager.HasCheckpoint;
        int currentStage = CheckpointManager.CurrentStage;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("Đang lưu (PlayerPrefs)", currentStage);
            EditorGUILayout.Toggle("Has Checkpoint", hasCheckpoint);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button($"Đồng bộ Force Stage = {currentStage}"))
            {
                _forceStageProp.intValue = currentStage;
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Xoá checkpoint ngay (Clear)"))
            {
                CheckpointManager.Clear();
            }
        }

        EditorGUILayout.HelpBox(
            "\"Đang lưu (PlayerPrefs)\" là tiến độ THẬT hiện có (VD vừa test xong intro thì đây sẽ tự thành 1). " +
            "\"Force Stage\" chỉ là lệnh ép cho lần Play TIẾP THEO, không tự đồng bộ theo dòng trên -- bấm nút " +
            "\"Đồng bộ\" nếu muốn Force Stage khớp đúng tiến độ thật đang có.",
            MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }
}
