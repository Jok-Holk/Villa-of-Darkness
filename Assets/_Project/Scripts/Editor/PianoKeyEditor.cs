#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor cho PianoKey — hiển thị _note dưới dạng dropdown
/// thay vì gõ tay, tránh nhập sai tên note.
/// File này đặt trong thư mục Editor (Assets/Editor/) hoặc bất kỳ thư mục nào.
/// </summary>
[CustomEditor(typeof(PianoKey))]
public class PianoKeyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PianoKey key = (PianoKey)target;

        serializedObject.Update();

        // Hiển thị field _noteDefinition
        SerializedProperty defProp = serializedObject.FindProperty("_noteDefinition");
        EditorGUILayout.PropertyField(defProp, new GUIContent("Note Definition"));

        // Lấy NoteDefinition để build dropdown
        PianoNoteDefinition def = defProp.objectReferenceValue as PianoNoteDefinition;

        if (def != null && def.notes != null && def.notes.Length > 0)
        {
            SerializedProperty noteProp = serializedObject.FindProperty("_note");
            string currentNote = noteProp.stringValue;

            // Tìm index hiện tại trong list
            int currentIndex = System.Array.IndexOf(def.notes, currentNote);
            if (currentIndex < 0) currentIndex = 0;

            // Hiện dropdown
            int newIndex = EditorGUILayout.Popup("Note", currentIndex, def.notes);
            noteProp.stringValue = def.notes[newIndex];
        }
        else
        {
            // Nếu chưa gán NoteDefinition thì vẫn cho gõ tay
            SerializedProperty noteProp = serializedObject.FindProperty("_note");
            EditorGUILayout.PropertyField(noteProp, new GUIContent("Note (gán Note Definition để dùng dropdown)"));
        }

        // Hiển thị field _piano
        SerializedProperty pianoProp = serializedObject.FindProperty("_piano");
        EditorGUILayout.PropertyField(pianoProp, new GUIContent("Piano"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif