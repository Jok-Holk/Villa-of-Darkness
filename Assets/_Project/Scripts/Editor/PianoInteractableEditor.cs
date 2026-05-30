#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor cho PianoInteractable — hiển thị _correctSequence
/// dưới dạng list dropdown thay vì gõ tay string.
/// Đặt file này vào Assets/Editor/
/// </summary>
[CustomEditor(typeof(PianoInteractable))]
public class PianoInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PianoInteractable piano = (PianoInteractable)target;
        serializedObject.Update();

        // Note Definition
        SerializedProperty defProp = serializedObject.FindProperty("_noteDefinition");
        EditorGUILayout.PropertyField(defProp, new GUIContent("Note Definition"));
        PianoNoteDefinition def = defProp.objectReferenceValue as PianoNoteDefinition;

        // Correct Sequence — dùng dropdown nếu có NoteDefinition
        SerializedProperty seqProp = serializedObject.FindProperty("_correctSequence");
        EditorGUILayout.LabelField("Correct Sequence", EditorStyles.boldLabel);

        if (def != null && def.notes != null && def.notes.Length > 0)
        {
            seqProp.arraySize = EditorGUILayout.IntField("Size", seqProp.arraySize);
            for (int i = 0; i < seqProp.arraySize; i++)
            {
                SerializedProperty elem = seqProp.GetArrayElementAtIndex(i);
                string current = elem.stringValue;
                int currentIdx = System.Array.IndexOf(def.notes, current);
                if (currentIdx < 0) currentIdx = 0;
                int newIdx = EditorGUILayout.Popup($"Element {i}", currentIdx, def.notes);
                elem.stringValue = def.notes[newIdx];
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Gán Note Definition để dùng dropdown chọn note.", MessageType.Info);
            EditorGUILayout.PropertyField(seqProp, new GUIContent("Correct Sequence (gõ tay)"), true);
        }

        // Sound
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_correctNoteClip"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_wrongNoteClip"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_sequenceCompleteClip"));

        // Ghost Spawn
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ghost Spawn", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_spawnManager"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_ghostPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_ghostSpawnPoint"));

        // Camera Zoom  ← MỚI
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera Zoom", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_cameraZoomTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_zoomSpeed"));

        // Events
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSequenceComplete"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnterPianoMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExitPianoMode"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif