using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Jok yêu cầu -- "tạo script riêng để xoá clear preview này đi, làm trống nó thôi": InventoryPanel trong
// scene vẫn còn dính dữ liệu preview cũ từ hồi [ExecuteAlways] còn bật (tên/mô tả "Sổ ghi nợ", pill "Sử
// dụng"/"Xem kỹ" bị vỡ chữ "89 dụng"...) -- giờ OnEnable() chỉ chạy lúc Play thật nên KHÔNG còn tự refresh
// trong Editor để dọn lại. Script riêng này KHÔNG động tới InventoryUI.cs, chỉ trực tiếp làm TRỐNG mọi thứ
// trong scene về đúng trạng thái "chưa chọn gì cả" -- Play thật sẽ tự Refresh() đè lên bằng data thật ngay.
public static class VoD_ClearInventoryPreview
{
    private static readonly Color DimBg   = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    private static readonly Color DimText = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    [MenuItem("VoD/Villa/Fix - Làm Trống Preview InventoryPanel (clear sạch)")]
    public static void ClearAll()
    {
        GameObject panel = FindByNameIncludingInactive("InventoryPanel");
        if (panel == null) { Debug.LogError("[VoD][ClearInvPreview] Không tìm thấy 'InventoryPanel' trong scene."); return; }

        // -- DetailsPanel: tên/eyebrow/mô tả rỗng, icon lớn ẩn --
        SetText(panel.transform, "DetailsPanel/ItemNameText", "");
        SetText(panel.transform, "DetailsPanel/CategoryText", "");
        SetText(panel.transform, "DetailsPanel/ItemDescText", "Chọn 1 vật phẩm để xem chi tiết");

        var detailIcon = FindDeepChild(panel.transform, "DetailIcon");
        if (detailIcon != null) detailIcon.gameObject.SetActive(false);

        // -- 2 pill "E Sử dụng"/"V Xem kỹ": về đúng chữ gốc + màu mờ (chưa chọn gì) --
        ResetPill(panel.transform, "DetailsPanel/ActionRow/UsePill", "Sử dụng");
        ResetPill(panel.transform, "DetailsPanel/ActionRow/ViewPill", "Xem kỹ");

        // -- W/S/Tab: về màu mờ mặc định --
        SetColor(panel.transform, "DetailsPanel/FootHintRow/MoveGroup/Tag_W/Text", DimText);
        SetColor(panel.transform, "DetailsPanel/FootHintRow/MoveGroup/Tag_S/Text", DimText);

        // -- Từng dòng trong list: rỗng hết, icon xám, highlight/stripe tắt --
        var grid = FindDeepChild(panel.transform, "Grid");
        if (grid != null)
        {
            foreach (Transform slot in grid)
            {
                SetText(slot, "Label", "");
                SetText(slot, "Status", "");

                var icon = slot.Find("Icon")?.GetComponent<Image>();
                if (icon != null) { icon.sprite = null; icon.color = new Color(0.267f, 0.267f, 0.267f, 1f); }

                var highlight = slot.Find("RowHighlight")?.GetComponent<Image>();
                if (highlight != null) highlight.color = new Color(0f, 0f, 0f, 0f);

                var stripe = slot.Find("Stripe")?.GetComponent<Image>();
                if (stripe != null) stripe.color = new Color(0.15f, 0.15f, 0.15f, 1f);

                var useBtn = slot.Find("UseButton");
                if (useBtn != null) useBtn.gameObject.SetActive(false);
            }
        }

        EditorUtility.SetDirty(panel);
        Debug.Log("[VoD][ClearInvPreview] Đã làm trống toàn bộ preview InventoryPanel. Play thật sẽ Refresh() đè bằng data thật ngay khi mở Tab.");
    }

    private static void ResetPill(Transform root, string pillPath, string label)
    {
        var labelT = root.Find(pillPath + "/Label");
        var labelText = labelT != null ? labelT.GetComponent<TMP_Text>() : null;
        if (labelText != null) { labelText.text = label; labelText.color = DimText; }

        var bgT = root.Find(pillPath);
        var bgImg = bgT != null ? bgT.GetComponent<Image>() : null;
        if (bgImg != null) bgImg.color = DimBg;
    }

    private static void SetText(Transform root, string path, string value)
    {
        var t = root.Find(path);
        var text = t != null ? t.GetComponent<TMP_Text>() : null;
        if (text != null) text.text = value;
    }

    private static void SetColor(Transform root, string path, Color color)
    {
        var t = root.Find(path);
        var text = t != null ? t.GetComponent<TMP_Text>() : null;
        if (text != null) text.color = color;
    }

    private static Transform FindDeepChild(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) return child;
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
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
