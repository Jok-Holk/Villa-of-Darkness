using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

// 1 TOOL DUY NHẤT quét TOÀN BỘ UI trong game để đối chiếu với mockup -- không chỉ vị trí (RectTransform)
// mà cả font (tên font asset + size + style), màu, sprite, layout (GridLayoutGroup/LayoutElement):
//   1) Canvas dựng sẵn trong scene (Inventory, InteractPrompt, DialoguePanel, DiaryReaderPanel...) -- quét
//      được cả lúc Edit Mode lẫn Play Mode.
//   2) ExamineStageUI/HudMetersUI/TutorialHintUI -- CHỈ tồn tại lúc đang Play (tự dựng bằng code lúc
//      Awake()), tool tự nhận biết Application.isPlaying, có thì quét thêm, không có thì báo rõ lý do
//      thay vì im lặng bỏ qua.
// PauseMenu/SettingPanel/DeathScreen chỉ liệt kê 1 dòng (ngoài phạm vi sửa, theo đúng phạm vi Jok đã chốt).
public static class VoD_ScanAllUI
{
    private static readonly string[] SkipDeepScan = { "PauseMenu", "SettingPanel", "DeathScreen" };
    private const int MaxDepth = 4;

    [MenuItem("VoD/Villa/Diagnose - Scan TOÀN BỘ UI (Canvas + Examine + HUD)")]
    public static void Scan()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========== VoD SCAN TOÀN BỘ UI ==========");
        sb.AppendLine($"Application.isPlaying = {Application.isPlaying}");
        sb.AppendLine("*** NHẮC: Canvas Screen Space - Overlay CHỈ hiện đúng hình dạng ở tab GAME, KHÔNG PHẢI tab Scene ***");
        sb.AppendLine("*** Tab Scene luôn vẽ Canvas thành 1 tấm phẳng nghiêng lơ lửng trong không gian 3D -- đó là hành vi CHUẨN của Unity, không phải lỗi. Muốn xem đúng: chuyển sang tab Game (cạnh tab Scene phía trên khung nhìn). ***\n");

        // ── 1) CANVAS DỰNG SẴN TRONG SCENE ──────────────────────────────────────
        GameObject canvasGO = FindByNameIncludingInactive("Canvas");
        if (canvasGO == null)
        {
            sb.AppendLine("[Canvas] KHÔNG TÌM THẤY.");
        }
        else
        {
            sb.AppendLine($"########## CANVAS (scene) -- {canvasGO.transform.childCount} con trực tiếp ##########\n");
            foreach (Transform child in canvasGO.transform)
            {
                if (System.Array.IndexOf(SkipDeepScan, child.name) >= 0)
                {
                    sb.AppendLine($"[{child.name}] active={child.gameObject.activeSelf} -- BỎ QUA (ngoài phạm vi sửa lần này)\n");
                    continue;
                }
                sb.AppendLine($"---------- {child.name} (active={child.gameObject.activeSelf}) ----------");
                DumpNode(sb, child, 0, true);
                sb.AppendLine();
            }

            // Đào sâu riêng Slot_0 -- vòng quét chính dừng ở MaxDepth, không đủ thấy bên trong 1 slot.
            Transform slot0 = FindChildByName(canvasGO.transform, "Slot_0");
            if (slot0 != null)
            {
                sb.AppendLine("========== ĐÀO SÂU RIÊNG: Slot_0 (không giới hạn depth) ==========");
                DumpNode(sb, slot0, 0, false);
            }
        }

        // ── 2) UI TỰ DỰNG LÚC PLAY (Examine/HUD/Tutorial Hint) ──────────────────
        sb.AppendLine("\n========== UI TỰ DỰNG BẰNG CODE LÚC PLAY ==========");
        if (!Application.isPlaying)
        {
            sb.AppendLine("CHƯA BẤM PLAY -- ExamineStageUI/HudMetersUI/TutorialHintUI chỉ được tạo ra lúc game đang chạy thật, không tồn tại ở Edit Mode. Bấm Play rồi chạy lại tool này để quét thêm phần này (phần Canvas ở trên vẫn quét được bình thường dù không Play).");
        }
        else
        {
            ScanRuntimeObject(sb, "ExamineStageUI", "Chưa từng bấm Examine (V) lần nào trong phiên Play này -- bấm V xem 1 vật rồi chạy lại tool NGAY LÚC ĐANG XEM (đừng thoát ra) mới thấy nội dung thật.");
            ScanRuntimeObject(sb, "HudMetersUI", "LẠ -- lẽ ra phải tự tạo ngay từ đầu Play, kiểm tra Console có lỗi không.");
            ScanRuntimeObject(sb, "TutorialHintUI", "Chưa được tạo (bình thường nếu chưa gặp tình huống nào kích hoạt hint).");
        }

        Debug.Log(sb.ToString());
        Debug.Log("[VoD][ScanAll] XONG -- log dài, nếu Console cắt bớt thì mở Editor.log (Help > Open Editor Log) để lấy đủ.");
    }

    private static void ScanRuntimeObject(StringBuilder sb, string name, string notFoundReason)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            sb.AppendLine($"\n[{name}] CHƯA ĐƯỢC TẠO -- {notFoundReason}");
            return;
        }
        sb.AppendLine($"\n---------- {name} (active={go.activeSelf}) ----------");
        DumpNode(sb, go.transform, 0, false);
    }

    // includeSlotLimit: true khi quét Canvas chính (giới hạn 2 slot con giống nhau + MaxDepth để log không
    // nổ to), false khi đào sâu 1 nhánh cụ thể (Slot_0 riêng, hoặc UI runtime -- không nhiều slot lặp lại).
    private static void DumpNode(StringBuilder sb, Transform t, int depth, bool includeSlotLimit)
    {
        if (includeSlotLimit && depth > MaxDepth) { sb.AppendLine($"{Indent(depth)}... (quá sâu, dừng ở depth {MaxDepth})"); return; }

        string indent = Indent(depth);
        var rt = t.GetComponent<RectTransform>();
        string rectInfo = rt != null
            ? $"anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} pivot={rt.pivot} anchoredPos={rt.anchoredPosition} sizeDelta={rt.sizeDelta}"
            : "(không có RectTransform)";
        sb.AppendLine($"{indent}- '{t.name}' active={t.gameObject.activeSelf} | {rectInfo}");

        var canvas = t.GetComponent<Canvas>();
        if (canvas != null) sb.AppendLine($"{indent}    Canvas: renderMode={canvas.renderMode} sortingOrder={canvas.sortingOrder}");

        var cg = t.GetComponent<CanvasGroup>();
        if (cg != null) sb.AppendLine($"{indent}    CanvasGroup: alpha={cg.alpha}");

        var img = t.GetComponent<Image>();
        if (img != null)
            sb.AppendLine($"{indent}    Image: color={img.color} sprite={(img.sprite != null ? img.sprite.name : "null")} type={img.type} fillAmount={img.fillAmount}");

        var rawImg = t.GetComponent<RawImage>();
        if (rawImg != null)
            sb.AppendLine($"{indent}    RawImage: color={rawImg.color} texture={(rawImg.texture != null ? rawImg.texture.name : "null")}");

        // Font: tên font asset + size + style + màu -- Jok yêu cầu đối chiếu cả font, không chỉ vị trí.
        var tmp = t.GetComponent<TMP_Text>();
        if (tmp != null)
            sb.AppendLine($"{indent}    TMP: text='{Truncate(tmp.text, 50)}' font={(tmp.font != null ? tmp.font.name : "null")} fontSize={tmp.fontSize} style={tmp.fontStyle} color={tmp.color} alignment={tmp.alignment}");

        var btn = t.GetComponent<Button>();
        if (btn != null) sb.AppendLine($"{indent}    Button: interactable={btn.interactable}");

        var gridLayout = t.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
            sb.AppendLine($"{indent}    GridLayoutGroup: cellSize={gridLayout.cellSize} spacing={gridLayout.spacing} padding=(L{gridLayout.padding.left},R{gridLayout.padding.right},T{gridLayout.padding.top},B{gridLayout.padding.bottom}) constraint={gridLayout.constraint} constraintCount={gridLayout.constraintCount}");
        else
        {
            var layoutGroup = t.GetComponent<LayoutGroup>();
            if (layoutGroup != null) sb.AppendLine($"{indent}    LayoutGroup: {layoutGroup.GetType().Name}");
        }

        var le = t.GetComponent<LayoutElement>();
        if (le != null)
            sb.AppendLine($"{indent}    LayoutElement: preferredW={le.preferredWidth} preferredH={le.preferredHeight} flexibleW={le.flexibleWidth}");

        int childCount = t.childCount;
        int limit = childCount;
        if (includeSlotLimit)
        {
            var scrollRectParent = t.GetComponentInParent<ScrollRect>();
            if (scrollRectParent != null && scrollRectParent.content == t && childCount > 2)
            {
                limit = 2;
                sb.AppendLine($"{indent}  ({childCount} slot con giống nhau -- chỉ quét 2 slot đầu)");
            }
        }

        for (int i = 0; i < limit; i++)
            DumpNode(sb, t.GetChild(i), depth + 1, includeSlotLimit);
    }

    private static string Indent(int depth) => new string(' ', depth * 2);

    private static string Truncate(string s, int len) => string.IsNullOrEmpty(s) || s.Length <= len ? s : s.Substring(0, len) + "...";

    private static Transform FindChildByName(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
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
