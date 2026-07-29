using UnityEditor;
using UnityEngine;

// Bake số Euler Jok đã chốt (log cuối cùng trước khi chuyển sang vật khác bằng ExamineOffsetTestHarness)
// vào field _examineRotationOffset của đúng ExamineItem tương ứng. Chỉ chạy 1 lần, xoá sau khi dùng xong.
public static class VoD_BakeExamineOffsets
{
    private static readonly (string objectName, Vector3 offset)[] Offsets = new (string, Vector3)[]
    {
        ("Prop_KeySalon",      new Vector3(87.8f, 213.7f, 33.7f)),
        ("Prop_KeySanSau",     new Vector3(90f,   180f,   0f)),
        ("Prop_KeyTiepKhach",  new Vector3(0f,    90f,    90f)),
        ("Prop_manh_giay_01",  Vector3.zero),
        ("Prop_manh_giay_02",  Vector3.zero),
        ("Prop_manh_giay_03",  Vector3.zero),
        ("Prop_manh_giay_04",  Vector3.zero),
        ("Prop_SoGhiNo",       new Vector3(0f, 0f, 180f)),
    };

    [MenuItem("VoD/Villa/Fix - Bake Examine Offset (đã chốt số)")]
    public static void Bake()
    {
        int done = 0, missing = 0;
        foreach (var (objectName, offset) in Offsets)
        {
            GameObject go = FindByNameIncludingInactive(objectName);
            if (go == null)
            {
                Debug.LogWarning($"[VoD][BakeOffset] Không tìm thấy '{objectName}' trong scene -- bỏ qua.");
                missing++;
                continue;
            }

            var examineItem = go.GetComponent<ExamineItem>();
            if (examineItem == null)
            {
                Debug.LogWarning($"[VoD][BakeOffset] '{objectName}' không có component ExamineItem -- bỏ qua.");
                missing++;
                continue;
            }

            var so = new SerializedObject(examineItem);
            var prop = so.FindProperty("_examineRotationOffset");
            if (prop == null)
            {
                Debug.LogWarning($"[VoD][BakeOffset] '{objectName}': không tìm thấy field '_examineRotationOffset'.");
                missing++;
                continue;
            }

            prop.vector3Value = offset;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(examineItem);
            Debug.Log($"[VoD][BakeOffset] '{objectName}' -- đã set offset = {offset}");
            done++;
        }

        Debug.Log($"[VoD][BakeOffset] XONG -- {done} item đã set, {missing} item bỏ qua. Nhớ Ctrl+S lưu scene.");
    }

    private static GameObject FindByNameIncludingInactive(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in all)
        {
            if (t.hideFlags != HideFlags.None) continue;
            if (t.name != name) continue;
            if (!t.gameObject.scene.IsValid()) continue;
            return t.gameObject;
        }
        return null;
    }
}
