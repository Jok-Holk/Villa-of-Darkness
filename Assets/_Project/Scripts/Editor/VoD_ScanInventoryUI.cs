using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Jok báo: màn Inventory thật có 2 chỗ hint "Đóng" (1 kiểu "[TAB] Đóng" nổi giữa, 1 kiểu "TAB  Đóng" góc dưới
// phải) + 1 chấm tròn trắng nổi trôi giữa panel chi tiết -- InventoryUI.cs KHÔNG hề có field/logic nào quản
// lý mấy thứ này (grep xác nhận), nghĩa là TOÀN BỘ đều là object đặt TAY trong scene, không phải bug code.
// Scan liệt kê hết TMP_Text (kèm nội dung) + Image tròn/không rõ nguồn gốc dưới "InventoryPanel" để biết
// chính xác object nào cần xoá/gộp, không đoán mù.
public static class VoD_ScanInventoryUI
{
    [MenuItem("VoD/Villa/Scan - Liệt Kê Chi Tiết InventoryPanel (hint trùng + chấm lạ)")]
    public static void Scan()
    {
        GameObject panel = FindByNameIncludingInactive("InventoryPanel");
        if (panel == null) { Debug.LogError("[VoD][ScanInventory] Không tìm thấy 'InventoryPanel' trong scene."); return; }

        var invUI = panel.GetComponent<InventoryUI>();
        if (invUI != null)
        {
            Debug.Log("[VoD][ScanInventory] === Field trong InventoryUI đang trỏ vào object nào (SerializedObject) ===");
            var so = new SerializedObject(invUI);
            LogFieldTarget(so, "_itemNameText", panel.transform);
            LogFieldTarget(so, "_itemDescText", panel.transform);
            LogFieldTarget(so, "_actionHintText", panel.transform);
            LogFieldTarget(so, "_moveHintText", panel.transform);
        }

        Debug.Log($"[VoD][ScanInventory] === Toàn bộ TMP_Text dưới InventoryPanel (kể cả đang tắt) ===");
        var texts = panel.GetComponentsInChildren<TMP_Text>(true);
        foreach (var t in texts)
        {
            string path = GetPath(t.transform, panel.transform);
            string preview = t.text.Length > 40 ? t.text.Substring(0, 40) + "..." : t.text;
            Debug.Log($"  '{path}' -- active={t.gameObject.activeInHierarchy} -- text=\"{preview}\"");
        }

        Debug.Log($"[VoD][ScanInventory] === Toàn bộ Image KHÔNG có sprite (khả nghi hình tròn/vuông trơn) ===");
        var images = panel.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.sprite != null) continue;
            string path = GetPath(img.transform, panel.transform);
            var rt = img.GetComponent<RectTransform>();
            Debug.Log($"  '{path}' -- active={img.gameObject.activeInHierarchy} -- sizeDelta={rt.sizeDelta} -- color={img.color}");
        }

        Debug.Log("[VoD][ScanInventory] === RectTransform thật (anchor/offset/size) -- không đoán mù nữa ===");
        LogRect(panel.transform, "DetailsPanel", panel.transform);
        LogRect(panel.transform, "DetailsPanel/ActionRow", panel.transform);
        LogRect(panel.transform, "DetailsPanel/FootHintRow", panel.transform);
        LogRect(panel.transform, "DetailsPanel/ItemNameText", panel.transform);
        LogRect(panel.transform, "DetailsPanel/ItemDescText", panel.transform);
        LogRect(panel.transform, "ListScrollView", panel.transform);
        LogRect(panel.transform, "ListScrollView/Viewport", panel.transform);
        LogRect(panel.transform, "ListScrollView/Viewport/Grid", panel.transform);
        LogRect(panel.transform, "ListScrollView/Scrollbar", panel.transform);
        LogRect(panel.transform, "ListScrollView/Scrollbar/SlidingArea", panel.transform);
        LogRect(panel.transform, "ListScrollView/Scrollbar/SlidingArea/Handle", panel.transform);

        var gridT = FindDeepChild(panel.transform, "Grid");
        var gridLayout = gridT != null ? gridT.GetComponent<GridLayoutGroup>() : null;
        if (gridLayout != null)
            Debug.Log($"[VoD][ScanInventory] GridLayoutGroup trên 'Grid': cellSize={gridLayout.cellSize}, spacing={gridLayout.spacing}, " +
                      $"padding=({gridLayout.padding.left},{gridLayout.padding.right},{gridLayout.padding.top},{gridLayout.padding.bottom}), " +
                      $"constraint={gridLayout.constraint}, childAlignment={gridLayout.childAlignment}");
        else if (gridT != null)
            Debug.Log("[VoD][ScanInventory] 'Grid' không có GridLayoutGroup (có thể dùng layout khác hoặc đặt tay từng dòng).");

        // THÊM (Jok báo -- "sao giờ tên item bị mất, chỉ thấy chữ trạng thái"): scan chi tiết Label/Status
        // của dòng đầu tiên (Slot_0) -- text/color/sibling index/RectTransform thật, không đoán mù nguyên nhân.
        Debug.Log("[VoD][ScanInventory] === Chi tiết Label/Status dòng Slot_0 (nghi tên item không hiện) ===");
        var slot0 = FindDeepChild(panel.transform, "Slot_0");
        if (slot0 != null)
        {
            var slot0Label = slot0.Find("Label")?.GetComponent<TMP_Text>();
            var slot0Status = slot0.Find("Status")?.GetComponent<TMP_Text>();
            if (slot0Label != null)
                Debug.Log($"  Label -- text=\"{slot0Label.text}\" color={slot0Label.color} fontSize={slot0Label.fontSize} " +
                          $"siblingIndex={slot0Label.transform.GetSiblingIndex()} active={slot0Label.gameObject.activeInHierarchy}");
            else
                Debug.LogWarning("  Label: KHÔNG tìm thấy dưới Slot_0");
            if (slot0Status != null)
                Debug.Log($"  Status -- text=\"{slot0Status.text}\" color={slot0Status.color} fontSize={slot0Status.fontSize} " +
                          $"siblingIndex={slot0Status.transform.GetSiblingIndex()} active={slot0Status.gameObject.activeInHierarchy}");
            else
                Debug.LogWarning("  Status: KHÔNG tìm thấy dưới Slot_0");
            LogRect(slot0, "Label", slot0);
            LogRect(slot0, "Status", slot0);
            LogRect(slot0, "Icon", slot0);

            // THÊM (Jok báo -- "vùng chọn stripe xanh + nền vàng chưa phủ hết chiều CAO row"): scan RectTransform
            // thật của Slot_0 (row gốc), RowHighlight, Stripe -- so sánh rect(h) của Slot_0 với 2 cái kia để
            // biết chính xác cái nào không phủ đủ, không đoán mù.
            LogRect(panel.transform, "ListScrollView/Viewport/Grid/Slot_0", panel.transform);
            LogRect(slot0, "RowHighlight", slot0);
            LogRect(slot0, "Stripe", slot0);
        }
        else
        {
            Debug.LogWarning("[VoD][ScanInventory] Không tìm thấy 'Slot_0' để scan Label/Status.");
        }

        // THÊM (Jok yêu cầu -- "thanh nhỏ dưới đó y cũng phải cách title ra"): scan vị trí thật của Title +
        // TitleDivider để biết chính xác baseline trước khi chỉnh Y, không đoán mù.
        Debug.Log("[VoD][ScanInventory] === RectTransform Title/TitleDivider ===");
        LogRect(panel.transform, "Title", panel.transform);
        LogRect(panel.transform, "TitleDivider", panel.transform);

        var scrollRect = panel.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            Debug.Log($"[VoD][ScanInventory] ScrollRect trên '{GetPath(scrollRect.transform, panel.transform)}': " +
                      $"horizontal={scrollRect.horizontal}, vertical={scrollRect.vertical}, " +
                      $"viewport={(scrollRect.viewport != null ? scrollRect.viewport.name : "NULL")}, " +
                      $"content={(scrollRect.content != null ? scrollRect.content.name : "NULL")}, " +
                      $"horizontalScrollbar={(scrollRect.horizontalScrollbar != null ? GetPath(scrollRect.horizontalScrollbar.transform, panel.transform) : "không gán")}, " +
                      $"verticalScrollbar={(scrollRect.verticalScrollbar != null ? GetPath(scrollRect.verticalScrollbar.transform, panel.transform) : "không gán")}");
        }

        Debug.Log("[VoD][ScanInventory] XONG -- đối chiếu list trên để biết đúng object nào là hint trùng/chấm lạ cần xoá.");
    }

    private static void LogRect(Transform root, string relativePath, Transform panelRoot)
    {
        Transform t = root;
        foreach (var part in relativePath.Split('/'))
        {
            t = t.Find(part);
            if (t == null) { Debug.LogWarning($"  '{relativePath}': KHÔNG tìm thấy (dừng ở '{part}')"); return; }
        }
        var rt = t.GetComponent<RectTransform>();
        if (rt == null) { Debug.LogWarning($"  '{relativePath}': không có RectTransform"); return; }

        Debug.Log($"  '{relativePath}' -- anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} pivot={rt.pivot} " +
                  $"anchoredPos={rt.anchoredPosition} sizeDelta={rt.sizeDelta} offsetMin={rt.offsetMin} offsetMax={rt.offsetMax} " +
                  $"rect(w,h)={rt.rect.width:F1},{rt.rect.height:F1} worldPos={rt.position}");
    }

    // Đã xác nhận qua scan: "HintText" (chữ "[TAB] Đóng" nổi giữa panel) là object RỜI, đứng trực tiếp dưới
    // InventoryPanel (không nằm trong Hotbar) -- trùng lặp hoàn toàn với "Hotbar/CloseText" ("TAB Đóng") đã
    // đúng vị trí góc dưới. Xoá bản thừa này, giữ lại đúng 1 bản trong Hotbar.
    [MenuItem("VoD/Villa/Fix - Xoá Hint 'Đóng' Trùng Trong InventoryPanel")]
    public static void DeleteDuplicateCloseHint()
    {
        GameObject panel = FindByNameIncludingInactive("InventoryPanel");
        if (panel == null) { Debug.LogError("[VoD][ScanInventory] Không tìm thấy 'InventoryPanel' trong scene."); return; }

        var stray = panel.transform.Find("HintText");
        if (stray == null) { Debug.Log("[VoD][ScanInventory] Không thấy 'HintText' -- có thể đã xoá rồi."); return; }

        Undo.DestroyObjectImmediate(stray.gameObject);
        Debug.Log("[VoD][ScanInventory] Đã xoá 'InventoryPanel/HintText' (bản trùng) -- giữ lại 'Hotbar/CloseText'.");
    }

    // THÊM (Jok yêu cầu -- "lên script xoá hết preview riêng luôn"): sau khi bỏ [ExecuteAlways], panel không
    // còn tự refresh khi bật/tắt trong Edit Mode -- dữ liệu preview cũ (RefreshPreviewOnly() set lúc trước)
    // cứ đứng yên/đóng băng trong scene. Không ảnh hưởng gameplay thật (Play → Open() vẫn Refresh() data thật),
    // nhưng nhìn trong Editor dễ nhầm là bug. Dọn sạch 1 lần cho scene về trạng thái rỗng đúng nghĩa.
    [MenuItem("VoD/Villa/Fix - Xoá Preview Data InventoryPanel")]
    public static void ClearPreviewData()
    {
        GameObject panel = FindByNameIncludingInactive("InventoryPanel");
        if (panel == null) { Debug.LogError("[VoD][ClearPreview] Không tìm thấy 'InventoryPanel' trong scene."); return; }

        SetText(panel.transform, "DetailsPanel/ItemNameText", "");
        SetText(panel.transform, "DetailsPanel/CategoryText", "");
        SetText(panel.transform, "DetailsPanel/ItemDescText", "Chọn 1 vật phẩm để xem chi tiết");

        var detailIcon = FindDeepChild(panel.transform, "DetailIcon");
        if (detailIcon != null) detailIcon.gameObject.SetActive(false);

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
        Debug.Log("[VoD][ClearPreview] Đã xoá sạch dữ liệu preview -- scene giờ về đúng trạng thái rỗng. Play thật sẽ tự Refresh() lại data thật, không cần làm gì thêm.");
    }

    // THÊM (Jok yêu cầu -- "tăng height RowHighlight/Stripe xuống phía dưới"): code trong InventoryUI.cs đã
    // sửa đúng, nhưng KHÔNG còn [ExecuteAlways] nên OnEnable()/CacheSlots() không tự chạy trong Editor nữa --
    // Jok chỉnh code xong không thấy gì đổi vì code chưa có dịp chạy. Tool này áp trực tiếp fix đó vào TOÀN
    // BỘ slot đang có trong scene, không cần Play.
    [MenuItem("VoD/Villa/Fix - Tăng Height RowHighlight+Stripe (tràn 20px xuống dưới)")]
    public static void ExtendRowHighlightAndStripe()
    {
        GameObject panel = FindByNameIncludingInactive("InventoryPanel");
        if (panel == null) { Debug.LogError("[VoD][ExtendHighlight] Không tìm thấy 'InventoryPanel' trong scene."); return; }

        var grid = FindDeepChild(panel.transform, "Grid");
        if (grid == null) { Debug.LogError("[VoD][ExtendHighlight] Không tìm thấy 'Grid'."); return; }

        int count = 0;
        foreach (Transform slot in grid)
        {
            var highlightRt = slot.Find("RowHighlight") as RectTransform;
            if (highlightRt != null)
            {
                highlightRt.offsetMin = new Vector2(0f, -20f);
                highlightRt.offsetMax = new Vector2(0f, 0f);
                count++;
            }

            var stripeRt = slot.Find("Stripe") as RectTransform;
            if (stripeRt != null)
            {
                stripeRt.offsetMin = new Vector2(stripeRt.offsetMin.x, -20f);
                stripeRt.offsetMax = new Vector2(stripeRt.offsetMax.x, 0f);
            }
        }

        EditorUtility.SetDirty(panel);
        Debug.Log($"[VoD][ExtendHighlight] Đã tràn RowHighlight+Stripe thêm 20px xuống dưới cho {count} dòng.");
    }

    private static void SetText(Transform root, string path, string value)
    {
        var t = root.Find(path);
        var text = t != null ? t.GetComponent<TMP_Text>() : null;
        if (text != null) text.text = value;
    }

    private static void LogFieldTarget(SerializedObject so, string fieldName, Transform panelRoot)
    {
        var prop = so.FindProperty(fieldName);
        if (prop == null) { Debug.LogWarning($"  {fieldName}: (không tìm thấy field trong script)"); return; }

        var obj = prop.objectReferenceValue;
        if (obj == null) { Debug.LogWarning($"  {fieldName}: ĐANG TRỐNG (null) -- code không bao giờ chạm được object nào cả."); return; }

        Component comp = obj as Component;
        Transform targetT = comp != null ? comp.transform : null;
        string path = targetT != null ? GetPath(targetT, panelRoot) : obj.name;
        Debug.Log($"  {fieldName} -> '{path}'");
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

    private static string GetPath(Transform t, Transform root)
    {
        string path = t.name;
        while (t.parent != null && t.parent != root)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
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
