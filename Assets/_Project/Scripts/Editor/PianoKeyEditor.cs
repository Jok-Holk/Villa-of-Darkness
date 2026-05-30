#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PianoKey))]
public class PianoKeyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Note Definition ──────────────────────────────────────────────────
        SerializedProperty defProp = serializedObject.FindProperty("_noteDefinition");
        EditorGUILayout.PropertyField(defProp, new GUIContent("Note Definition"));

        PianoNoteDefinition def = defProp.objectReferenceValue as PianoNoteDefinition;

        // ── Note (dropdown hoặc gõ tay) ──────────────────────────────────────
        SerializedProperty noteProp = serializedObject.FindProperty("_note");
        if (def != null && def.notes != null && def.notes.Length > 0)
        {
            string currentNote = noteProp.stringValue;
            int currentIndex   = System.Array.IndexOf(def.notes, currentNote);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex         = EditorGUILayout.Popup("Note", currentIndex, def.notes);
            noteProp.stringValue = def.notes[newIndex];
        }
        else
        {
            EditorGUILayout.PropertyField(noteProp,
                new GUIContent("Note (gán Note Definition để dùng dropdown)"));
        }

        // ── KeyCode ───────────────────────────────────────────────────────────
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("_keyCode"),
            new GUIContent("Key Code", "Phím bàn phím để nhấn note này khi đang trong piano mode"));

        // ── Piano reference ───────────────────────────────────────────────────
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("_piano"),
            new GUIContent("Piano"));

        // ── Animation fields ──────────────────────────────────────────────────
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_pressDepth"),
            new GUIContent("Press Depth", "Phím nhún xuống bao nhiêu đơn vị theo trục Y"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_pressSpeed"),
            new GUIContent("Press Speed", "Tốc độ nhún xuống"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_returnSpeed"),
            new GUIContent("Return Speed", "Tốc độ trả lên"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif