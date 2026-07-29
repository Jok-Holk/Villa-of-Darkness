using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Cửa sổ riêng bật/tắt từng phần UI để xem layout -- thay vì mò trong Hierarchy (nhiều object dễ rối,
// theo đúng yêu cầu Jok). Mỗi dòng hiện tên dễ hiểu + trạng thái thật (Bật/Tắt) + 1 nút bấm để đảo trạng
// thái ngay -- không cần tìm đúng object con nằm sâu bao nhiêu cấp.
public class VoD_UIPreviewWindow : EditorWindow
{
    private static readonly (string label, string path)[] Targets =
    {
        ("Màn hình đen (ScreenFader)",        "ScreenFader_Canvas"),
        ("Examine -- màn soi 3D vật phẩm",     "ExamineStageUI/ExamineStageCanvas"),
        ("HUD -- 2 vạch Thể lực/Đèn pin",      "HudMetersUI/HudMetersUI_Canvas"),
        ("Gợi ý phím tắt (Tutorial Hint)",     "TutorialHintUI/TutorialHintUI_Canvas/HintRoot"),
        ("Chấm ngắm + [E] tương tác",          "InteractPrompt/PromptRoot"),
        ("Đọc nhật ký",                        "DiaryReaderPanel"),
        ("Túi đồ (Inventory)",                 "InventoryPanel"),
    };

    [MenuItem("VoD/Villa/Xem Nhanh UI (bật/tắt từng phần)")]
    public static void Open()
    {
        var win = GetWindow<VoD_UIPreviewWindow>("VoD - Xem Nhanh UI");
        win.minSize = new Vector2(360, 260);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox("Bấm nút để bật/tắt xem layout. NHỚ xem ở tab GAME (không phải Scene) để thấy đúng hình dạng thật.", MessageType.Info);
        EditorGUILayout.Space(6);

        foreach (var (label, path) in Targets)
        {
            GameObject go = FindByPathIncludingInactive(path);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (go == null)
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(230));
                EditorGUILayout.LabelField("Không tìm thấy trong scene", EditorStyles.miniLabel);
            }
            else
            {
                // SỬA: 1 vài object (VD ScreenFader_Canvas) dùng CanvasGroup.alpha để ẩn/hiện THẬT (không
                // phải SetActive) -- mặc định alpha=0 dù GameObject đang BẬT, bật SetActive thôi sẽ vẫn
                // trong suốt, không thấy gì. Kiểm tra CanvasGroup nếu có, hiện trạng thái + nút bấm theo
                // đúng độ TRONG SUỐT THẬT chứ không chỉ theo activeSelf.
                var canvasGroup = go.GetComponent<CanvasGroup>();
                // SỬA: activeSelf CHỈ xét đúng object này -- nếu 1 tổ tiên (VD "ExamineStageUI" cha) đang
                // tắt thì con "ExamineStageCanvas" dù activeSelf=true vẫn KHÔNG hiện được gì (Unity yêu cầu
                // TOÀN BỘ chuỗi cha phải bật). Dùng activeInHierarchy để phản ánh đúng "có thật sự hiện
                // được hay không", không chỉ mỗi trạng thái của riêng object này.
                bool isOn = go.activeInHierarchy && (canvasGroup == null || canvasGroup.alpha > 0.5f);
                bool parentBlocking = go.activeSelf && !go.activeInHierarchy;

                var color = GUI.color;
                GUI.color = isOn ? new Color(0.6f, 1f, 0.6f) : Color.white;
                EditorGUILayout.LabelField(label, GUILayout.Width(230));
                GUI.color = color;

                string statusText = isOn ? "Đang BẬT" : (parentBlocking ? "Tắt (do CHA đang tắt)" : "Đang tắt");
                if (canvasGroup != null) statusText += $" (alpha={canvasGroup.alpha:F1})";
                EditorGUILayout.LabelField(statusText, GUILayout.Width(160));

                if (GUILayout.Button(isOn ? "Tắt" : "Bật", GUILayout.Width(50)))
                {
                    go.SetActive(!isOn);
                    if (canvasGroup != null) canvasGroup.alpha = isOn ? 0f : 1f;
                    EditorUtility.SetDirty(go);

                    // Bật object này KHÔNG có nghĩa gì nếu TỔ TIÊN của nó vẫn đang tắt -- bật luôn cả chuỗi
                    // cha lên trên tới gốc, đúng lỗi Jok phát hiện ("ExamineStageUI" cha tắt, con
                    // "ExamineStageCanvas" bật vô ích).
                    if (!isOn) ForceActivateAllAncestors(go.transform);

                    // XOÁ 2026-07-29 (Jok phát hiện -- "tắt bật lại vẫn lỗi với mấy UI cũ không xoá à"):
                    // ForceActivateAllChildren() từng thêm cho case "BlackImage" của ScreenFader -- nhưng
                    // ScreenFader ĐÃ tự sửa trong chính Awake() của nó rồi (xem ScreenFader.cs), nên dòng này
                    // hoá ra THỪA cho case đó, mà lại ÉP BẬT LẠI mọi con cố ý ẩn của các hệ khác: Inventory
                    // (UseButton mỗi dòng, DetailIcon lúc chưa chọn, Hotbar cũ), Diary (3 nút cũ đã ẩn),
                    // Examine (ReadOverlay). Mỗi hệ tự quản lý đúng con của mình qua OnEnable() riêng rồi --
                    // không cần (và không nên) ép bật đại trà ở đây nữa.
                }

                if (GUILayout.Button("Chọn", GUILayout.Width(50)))
                {
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Tắt HẾT (dọn sạch trước khi lưu scene)"))
        {
            foreach (var (_, path) in Targets)
            {
                GameObject go = FindByPathIncludingInactive(path);
                if (go == null) continue;
                var canvasGroup = go.GetComponent<CanvasGroup>();
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                if (go.activeSelf) go.SetActive(false);
                EditorUtility.SetDirty(go);
            }
        }
    }

    private static void ForceActivateAllAncestors(Transform t)
    {
        Transform parent = t.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(true);
                EditorUtility.SetDirty(parent.gameObject);
            }
            parent = parent.parent;
        }
    }


    // Hỗ trợ path dạng "Cha/Con" -- tìm object cha theo tên (kể cả đang tắt), rồi tìm con theo path con.
    private static GameObject FindByPathIncludingInactive(string path)
    {
        string[] parts = path.Split('/');
        GameObject root = FindByNameIncludingInactive(parts[0]);
        if (root == null) return null;
        if (parts.Length == 1) return root;

        Transform current = root.transform;
        for (int i = 1; i < parts.Length; i++)
        {
            current = current.Find(parts[i]);
            if (current == null) return null;
        }
        return current.gameObject;
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
