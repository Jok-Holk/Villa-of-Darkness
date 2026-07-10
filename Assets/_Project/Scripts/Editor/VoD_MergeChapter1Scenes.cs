using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// Bộ tool chẩn đoán/sửa lỗi scene còn dùng lại được lâu dài.
// Các nút một-lần-dùng-xong (rebuild scene, fix layout inventory qua nhiều bản, fix rig/material/audio listener,
// import PauseMenu/DeathScreen...) đã hoàn thành nhiệm vụ và bị dọn khỏi file này — xem lịch sử git nếu cần xem lại.
public static class VoD_MergeChapter1Scenes
{
    [MenuItem("VoD/Temp/Preview DeathScreen (không cần Play)")]
    public static void PreviewDeathScreenNoPlay()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }

        var so = new SerializedObject(deathUI);

        GameObject panelGO = so.FindProperty("deathScreenPanel").objectReferenceValue as GameObject;
        if (panelGO != null) panelGO.SetActive(true);

        void ActivateAndFade(string fieldName, float alpha)
        {
            var comp = so.FindProperty(fieldName).objectReferenceValue as Component;
            if (comp == null) return;
            comp.gameObject.SetActive(true);
            var g = comp.GetComponent<Graphic>();
            if (g != null) { Color c = g.color; c.a = alpha; g.color = c; }
        }

        ActivateAndFade("vignetteImage", 1f);
        ActivateAndFade("apparitionImage", 0.4f); // preview tĩnh — không random ẩn hiện như lúc Play thật

        var quoteText = so.FindProperty("quoteText").objectReferenceValue as TMP_Text;
        if (quoteText != null && string.IsNullOrEmpty(quoteText.text))
            quoteText.text = "\"Nguyễn Minh Khoa, 1979 – 2000...\""; // text mẫu, edit-mode Show() thật không chạy nên rỗng

        // Vignette cần texture để hiện — edit-mode không chạy Awake() nên chưa có texture. Sinh tạm bằng reflection.
        // Hàm giờ có thêm tham số centerYRatio — đọc đúng giá trị field "vignetteCenterY" Jok đang chỉnh trong Inspector.
        var voidMethod = typeof(DeathScreenUI).GetMethod("GenerateVignetteTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vignetteImg = so.FindProperty("vignetteImage").objectReferenceValue as RawImage;
        var centerYProp = so.FindProperty("vignetteCenterY");
        float centerY = centerYProp != null ? centerYProp.floatValue : 0.82f;
        if (vignetteImg != null && vignetteImg.texture == null && voidMethod != null)
        {
            var tex = voidMethod.Invoke(null, new object[] { 192, 108, centerY }) as Texture2D;
            vignetteImg.texture = tex;
            vignetteImg.color = Color.white;
        }
        var apparitionImg = so.FindProperty("apparitionImage").objectReferenceValue as RawImage;
        var appMethod = typeof(DeathScreenUI).GetMethod("GenerateApparitionTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (apparitionImg != null && apparitionImg.texture == null && appMethod != null)
        {
            var tex = appMethod.Invoke(null, new object[] { 96, 138 }) as Texture2D;
            apparitionImg.texture = tex;
        }

        Debug.Log("[VoD] Đã bật preview DeathScreen ở Edit Mode (không cần Play) — KHÔNG chạy được hiệu ứng động (rung sắc/chớp/pulse nút, cần Play thật). Nhớ chạy \"Stop Preview\" trước khi Save Scene để không lưu nhầm trạng thái preview.");
    }

    [MenuItem("VoD/Temp/Stop Preview DeathScreen")]
    public static void StopPreviewDeathScreen()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }

        var so = new SerializedObject(deathUI);
        GameObject panelGO = so.FindProperty("deathScreenPanel").objectReferenceValue as GameObject;
        if (panelGO != null) panelGO.SetActive(false);

        string[] fields = { "vignetteImage", "apparitionImage", "whiteFlash" };
        foreach (var f in fields)
        {
            var comp = so.FindProperty(f).objectReferenceValue as Component;
            if (comp != null) comp.gameObject.SetActive(false);
        }

        Debug.Log("[VoD] Đã tắt preview — về đúng trạng thái ẩn mặc định trước khi Save Scene.");
    }

    [MenuItem("VoD/Temp/Enter Play Mode")]
    public static void EnterPlayModeForTest() { EditorApplication.isPlaying = true; }

    [MenuItem("VoD/Temp/Exit Play Mode")]
    public static void ExitPlayModeForTest() { EditorApplication.isPlaying = false; }

    // MCP hay timeout khi đọc GameObject qua get_gameobject — thay vì query từng cái qua MCP (chậm, hay treo),
    // tool này quét 1 lần, ghi hết ra file text ở root project. Jok bấm nút này trong Unity (không qua MCP),
    // Claude đọc file bằng Read tool bình thường — nhanh và ổn định hơn nhiều so với gọi MCP lặp lại.
    [MenuItem("VoD/Temp/Scan DeathScreen Report")]
    public static void ScanDeathScreenReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══ VoD DeathScreen Diagnostic Report ═══");
        sb.AppendLine($"Screen.width x height (runtime): {Screen.width} x {Screen.height}");
        sb.AppendLine($"Screen.currentResolution: {Screen.currentResolution}");
        sb.AppendLine($"Application.isPlaying: {Application.isPlaying}");

        // Kích thước Game View thật (khác Screen.width/height khi ở Editor, không maximize) — lấy qua reflection
        // vì Unity không public API này.
        try
        {
            var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var getSizeMethod = gameViewType.GetMethod("GetSizeOfMainGameView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var size = (Vector2)getSizeMethod.Invoke(null, null);
            sb.AppendLine($"GameView.GetSizeOfMainGameView(): {size.x} x {size.y}");
        }
        catch (System.Exception e) { sb.AppendLine($"GameView size: lỗi đọc ({e.Message})"); }

        void DumpRect(string label, Component c)
        {
            if (c == null) { sb.AppendLine($"[{label}] NULL — không gán trong Inspector"); return; }
            var rt = c.GetComponent<RectTransform>();
            if (rt == null) { sb.AppendLine($"[{label}] không có RectTransform"); return; }
            var g = c.GetComponent<Graphic>();
            sb.AppendLine($"[{label}] activeSelf={c.gameObject.activeSelf} activeInHierarchy={c.gameObject.activeInHierarchy}");
            sb.AppendLine($"    anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} pivot={rt.pivot}");
            sb.AppendLine($"    sizeDelta={rt.sizeDelta} anchoredPosition={rt.anchoredPosition}");
            sb.AppendLine($"    offsetMin={rt.offsetMin} offsetMax={rt.offsetMax} rect={rt.rect}");
            sb.AppendLine($"    localScale={rt.localScale} lossyScale={rt.lossyScale}");
            if (g != null) sb.AppendLine($"    color={g.color} raycastTarget={g.raycastTarget}");
            if (c is RawImage ri) sb.AppendLine($"    texture={(ri.texture != null ? $"{ri.texture.width}x{ri.texture.height}" : "NULL")} uvRect={ri.uvRect}");
        }

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var crt = canvas.GetComponent<RectTransform>();
            sb.AppendLine($"[Canvas] renderMode={canvas.renderMode} pixelRect={canvas.pixelRect} scaleFactor={canvas.scaleFactor}");
            sb.AppendLine($"    rect={crt.rect} sizeDelta={crt.sizeDelta}");
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
                sb.AppendLine($"    CanvasScaler: uiScaleMode={scaler.uiScaleMode} referenceResolution={scaler.referenceResolution} matchWidthOrHeight={scaler.matchWidthOrHeight}");
        }

        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        DumpRect("DeathScreen (root)", deathUI);

        var so = new SerializedObject(deathUI);
        string[] fieldsToScan = {
            "deathScreenPanel", "glitchNoise", "scanlineStrip",
            "mastheadText", "headlineText", "subText", "quoteText",
            "vignetteImage", "apparitionImage", "whiteFlash",
            "headlineGhostR", "headlineGhostC"
        };
        foreach (var f in fieldsToScan)
        {
            var prop = so.FindProperty(f);
            var obj = prop != null ? prop.objectReferenceValue : null;
            Component comp = obj as Component;
            if (comp == null && obj is GameObject go) comp = go.transform;
            DumpRect(f, comp);
        }

        string path = System.IO.Path.Combine(Application.dataPath, "../VoD_DeathScreenReport.txt");
        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"[VoD] Đã ghi report ra: {path}\n\n{sb}");
    }

    // Root cause tìm được từ report: DeathScreenPanel.localScale = 0.81 (cruft, không phải 1) khiến toàn bộ
    // con của nó (Masthead/Headline/SubText/Quote/GlitchNoise/ScanlineStrip) bị co 81%, để lại viền trống quanh
    // màn hình dù RectTransform stretch full đúng. Vignette/Apparition/WhiteFlash không bị vì chúng là con của
    // Canvas trực tiếp, không phải con của DeathScreenPanel.
    [MenuItem("VoD/Temp/Fix DeathScreenPanel Scale To 1")]
    public static void FixDeathScreenPanelScale()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }

        var so = new SerializedObject(deathUI);
        var panelGO = so.FindProperty("deathScreenPanel").objectReferenceValue as GameObject;
        if (panelGO == null) { Debug.LogError("[VoD] deathScreenPanel field rỗng."); return; }

        Vector3 before = panelGO.transform.localScale;
        panelGO.transform.localScale = Vector3.one;
        EditorUtility.SetDirty(panelGO);
        Debug.Log($"[VoD] DeathScreenPanel.localScale: {before} → {panelGO.transform.localScale}. Nhớ Save Scene.");
    }

    // Import font AmaticSC-Bold.ttf (Jok tải về D:\...\Downloads\Amatic_SC_extracted) vào project, tạo TMP Font Asset
    // theo đúng cách đã xác nhận ăn (Merienda trước đó) — CreateAsset trực tiếp thay vì CopySerialized (CopySerialized
    // làm hỏng glyph-rect mapping, "nát bét" — bài học cũ). Rồi gán font mới cho toàn bộ text/button của DeathScreen.
    [MenuItem("VoD/Temp/Import AmaticSC Font And Apply To DeathScreen")]
    public static void ImportAmaticFontAndApply()
    {
        string sourcePath = @"C:\Users\Admin\Downloads\Amatic_SC_extracted\AmaticSC-Bold.ttf";
        if (!System.IO.File.Exists(sourcePath))
        {
            Debug.LogError($"[VoD] Không tìm thấy file font ở {sourcePath}. Kiểm tra lại đường dẫn giải nén.");
            return;
        }

        string destDir = "Assets/_Project/Fonts/AmaticSC";
        if (!AssetDatabase.IsValidFolder(destDir))
            AssetDatabase.CreateFolder("Assets/_Project/Fonts", "AmaticSC");

        string destTtf = destDir + "/AmaticSC-Bold.ttf";
        System.IO.File.Copy(sourcePath, destTtf, true);
        AssetDatabase.ImportAsset(destTtf, ImportAssetOptions.ForceUpdate);

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(destTtf);
        if (sourceFont == null) { Debug.LogError("[VoD] Import .ttf thất bại — không load được Font asset."); return; }

        string fontAssetPath = destDir + "/AmaticSC-Bold SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        if (existing != null) AssetDatabase.DeleteAsset(fontAssetPath);

        var newFontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048,
            AtlasPopulationMode.Dynamic, true);
        AssetDatabase.CreateAsset(newFontAsset, fontAssetPath);
        if (newFontAsset.atlasTextures != null && newFontAsset.atlasTextures.Length > 0)
            AssetDatabase.AddObjectToAsset(newFontAsset.atlasTextures[0], newFontAsset);
        AssetDatabase.AddObjectToAsset(newFontAsset.material, newFontAsset);
        AssetDatabase.SaveAssets();

        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        int changed = 0;
        void ApplyFont(string fieldName)
        {
            var prop = so.FindProperty(fieldName);
            var t = prop?.objectReferenceValue as TMP_Text;
            if (t == null) return;
            t.font = newFontAsset;
            EditorUtility.SetDirty(t);
            changed++;
        }

        ApplyFont("mastheadText");
        ApplyFont("headlineText");
        ApplyFont("subText");
        ApplyFont("quoteText");
        ApplyFont("headlineGhostR");
        ApplyFont("headlineGhostC");

        void ApplyFontToButton(string fieldName)
        {
            var prop = so.FindProperty(fieldName);
            var btn = prop?.objectReferenceValue as Button;
            if (btn == null) return;
            var t = btn.GetComponentInChildren<TMP_Text>(true);
            if (t == null) return;
            t.font = newFontAsset;
            EditorUtility.SetDirty(t);
            changed++;
        }

        ApplyFontToButton("retryButton");
        ApplyFontToButton("menuButton");

        Debug.Log($"[VoD] Đã tạo font asset '{fontAssetPath}' và gán cho {changed} TMP_Text/Button trong DeathScreen. Nhớ Save Scene.");
    }

    // Report cho thấy thứ tự sibling hiện tại: Masthead...ButtonRow rồi mới tới GlitchNoise/ScanlineStrip cuối cùng
    // — nghĩa là lớp nhiễu đang VẼ ĐÈ LÊN chữ/nút (child sau cùng vẽ trên cùng trong uGUI). Đổi lại: đưa
    // GlitchNoise + ScanlineStrip lên làm 2 con đầu tiên để chữ/nút luôn nổi trên lớp pixel.
    [MenuItem("VoD/Temp/Reorder DeathScreen - Text Above Glitch")]
    public static void ReorderTextAboveGlitch()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        var glitchNoise = so.FindProperty("glitchNoise").objectReferenceValue as Component;
        var scanline = so.FindProperty("scanlineStrip").objectReferenceValue as Component;
        if (glitchNoise == null || scanline == null) { Debug.LogError("[VoD] Thiếu glitchNoise/scanlineStrip."); return; }

        glitchNoise.transform.SetSiblingIndex(0);
        scanline.transform.SetSiblingIndex(1);
        EditorUtility.SetDirty(glitchNoise.gameObject);
        EditorUtility.SetDirty(scanline.gameObject);

        Debug.Log("[VoD] Đã đưa GlitchNoise + ScanlineStrip xuống dưới cùng (sibling 0,1) — chữ/nút giờ vẽ đè lên trên. Nhớ Save Scene.");
    }

    // Vấn đề design Jok chỉ ra: Masthead/Headline/SubText đang dùng CHUNG 1 màu kem-vàng #E9DCC0 — không có
    // phân cấp, mắt không biết nhìn đâu trước. Quote (tên + năm nạn nhân — dòng đáng lẽ là cú đấm cảm xúc nhất)
    // lại là màu xám mờ nhất, ngược hoàn toàn với vai trò của nó. Fix: Masthead/SubText mờ xuống (chỉ là info phụ),
    // Headline sáng nhất giữ nguyên vai trò tiêu đề chính, Quote đổi sang đỏ máu để trở thành điểm nhấn thật.
    [MenuItem("VoD/Temp/Fix DeathScreen Text Color Hierarchy")]
    public static void FixTextColorHierarchy()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        void SetColor(string fieldName, Color c)
        {
            var t = so.FindProperty(fieldName)?.objectReferenceValue as TMP_Text;
            if (t == null) { Debug.LogWarning($"[VoD] Thiếu field {fieldName}"); return; }
            t.color = c;
            EditorUtility.SetDirty(t);
        }

        SetColor("mastheadText", new Color(0.55f, 0.51f, 0.44f)); // #8C8270 — mờ xuống, chỉ là chrome/context
        SetColor("headlineText", new Color(0.95f, 0.90f, 0.79f)); // #F2E6C9 — sáng nhất, tiêu đề chính
        SetColor("subText",      new Color(0.60f, 0.57f, 0.50f)); // #9A9280 — mờ, info phụ
        SetColor("quoteText",    new Color(0.70f, 0.22f, 0.17f)); // #B33A2C — đỏ máu, điểm nhấn cảm xúc

        Debug.Log("[VoD] Đã sửa phân cấp màu: Masthead/SubText mờ, Headline sáng, Quote đỏ máu làm điểm nhấn. Nhớ Save Scene.");
    }

    // Feedback Jok: Quote (tên nạn nhân) vẫn chìm dù đã đỏ — do size quá nhỏ (42) so với Headline (112).
    // 2 button quá nhỏ/thấp, "lọt khỏm". Tăng cả 2 lên, đồng thời nâng ButtonRow lên một chút cho đỡ dồn sát đáy.
    [MenuItem("VoD/Temp/Boost Quote Size And Button Scale")]
    public static void BoostQuoteAndButtons()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        var quoteText = so.FindProperty("quoteText")?.objectReferenceValue as TMP_Text;
        if (quoteText != null)
        {
            quoteText.fontSize = 62f;                 // 42 → 62, tăng hẳn để không bị chìm cạnh Headline (112)
            quoteText.fontStyle = FontStyles.Bold;
            quoteText.characterSpacing = 2f;           // giảm bớt spacing cũ (4) vì size đã to hơn nhiều
            EditorUtility.SetDirty(quoteText);
        }
        else Debug.LogWarning("[VoD] Thiếu quoteText.");

        var retryButton = so.FindProperty("retryButton")?.objectReferenceValue as Button;
        var menuButton  = so.FindProperty("menuButton")?.objectReferenceValue as Button;

        void BoostButton(Button btn)
        {
            if (btn == null) return;
            var rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300f, 96f);
            var txt = btn.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) { txt.fontSize = 34f; EditorUtility.SetDirty(txt); }
            EditorUtility.SetDirty(rt);
        }

        BoostButton(retryButton);
        BoostButton(menuButton);

        // ButtonRow là cha trực tiếp của 2 nút — lấy qua transform.parent thay vì field riêng (không có field).
        if (retryButton != null)
        {
            var buttonRow = retryButton.transform.parent;
            var rowRt = buttonRow.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(700f, 110f);
            rowRt.anchoredPosition = new Vector2(rowRt.anchoredPosition.x, -420f); // -460 → -420, nâng lên khỏi đáy
            var hlg = buttonRow.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) hlg.spacing = 40f;
            EditorUtility.SetDirty(buttonRow.gameObject);
        }

        Debug.Log("[VoD] Đã tăng size Quote (62, bold) + 2 button (300x96, font 34) + nâng ButtonRow lên -420. Nhớ Save Scene.");
    }

    // Feedback tiếp: SubText quá nhỏ, và từ SubText trở xuống (SubText/Quote/ButtonRow) đang dồn quá thấp.
    // Nâng cả khối lên +100 nhưng GIỮ NGUYÊN khoảng cách tương đối giữa 3 phần tử (đã tune trước đó khớp mockup).
    [MenuItem("VoD/Temp/Raise Lower Block And Enlarge SubText")]
    public static void RaiseLowerBlockAndEnlargeSubText()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        var subText   = so.FindProperty("subText")?.objectReferenceValue as TMP_Text;
        var quoteText = so.FindProperty("quoteText")?.objectReferenceValue as TMP_Text;
        var retryButton = so.FindProperty("retryButton")?.objectReferenceValue as Button;

        const float delta = 100f;

        if (subText != null)
        {
            subText.fontSize = 38f; // 30 → 38
            var rt = subText.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + delta);
            EditorUtility.SetDirty(subText);
            EditorUtility.SetDirty(rt);
        }
        else Debug.LogWarning("[VoD] Thiếu subText.");

        if (quoteText != null)
        {
            var rt = quoteText.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + delta);
            EditorUtility.SetDirty(rt);
        }
        else Debug.LogWarning("[VoD] Thiếu quoteText.");

        if (retryButton != null)
        {
            var buttonRow = retryButton.transform.parent;
            var rowRt = buttonRow.GetComponent<RectTransform>();
            rowRt.anchoredPosition = new Vector2(rowRt.anchoredPosition.x, rowRt.anchoredPosition.y + delta);
            EditorUtility.SetDirty(buttonRow.gameObject);
        }
        else Debug.LogWarning("[VoD] Thiếu retryButton (để tìm ButtonRow qua parent).");

        Debug.Log("[VoD] Đã nâng SubText/Quote/ButtonRow lên +100, SubText fontSize 30→38. Nhớ Save Scene.");
    }

    // Feedback: SubText + Quote màu quá mờ (#9A9280 / #B33A2C trước đó), lúc màn hình nhấp nháy (glitch/flash)
    // 2 dòng này bị lọt/chìm hẳn. Sáng lên: SubText sáng thêm nhiều hơn (đang mờ nhất trong 4 dòng),
    // Quote sáng/đậm thêm một chút (đã có màu đỏ riêng biệt rồi nên chỉ cần tăng nhẹ).
    [MenuItem("VoD/Temp/Brighten SubText And Quote Colors")]
    public static void BrightenSubTextAndQuoteColors()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        var subText   = so.FindProperty("subText")?.objectReferenceValue as TMP_Text;
        var quoteText = so.FindProperty("quoteText")?.objectReferenceValue as TMP_Text;

        if (subText != null)
        {
            subText.color = new Color(0.80f, 0.76f, 0.66f); // #CCC2A8 — sáng hẳn lên, tránh chìm khi nhấp nháy
            EditorUtility.SetDirty(subText);
        }
        else Debug.LogWarning("[VoD] Thiếu subText.");

        if (quoteText != null)
        {
            quoteText.color = new Color(0.85f, 0.30f, 0.22f); // #D94D38 — đỏ sáng/đậm hơn 1 nấc so với #B33A2C cũ
            EditorUtility.SetDirty(quoteText);
        }
        else Debug.LogWarning("[VoD] Thiếu quoteText.");

        Debug.Log("[VoD] Đã sáng màu SubText (#CCC2A8) và Quote (#D94D38). Nhớ Save Scene.");
    }

    // Report xác nhận +100 trước đã áp dụng đúng (subText -157→-57, quote -264→-164) nhưng vẫn còn khoảng
    // trống rất lớn tới Headline (Y=209) — 266px, chiếm gần 1/4 chiều cao canvas — nên mắt thấy "không nhít".
    // Lần này nâng dứt khoát +160 nữa để khoảng trống co lại còn ~106px, rõ rệt hơn hẳn.
    [MenuItem("VoD/Temp/Raise Lower Block Again (+160)")]
    public static void RaiseLowerBlockAgain()
    {
        var deathUI = Object.FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null) { Debug.LogError("[VoD] Không tìm thấy DeathScreenUI."); return; }
        var so = new SerializedObject(deathUI);

        var subText     = so.FindProperty("subText")?.objectReferenceValue as TMP_Text;
        var quoteText   = so.FindProperty("quoteText")?.objectReferenceValue as TMP_Text;
        var retryButton = so.FindProperty("retryButton")?.objectReferenceValue as Button;

        const float delta = 160f;

        void Shift(Component c)
        {
            if (c == null) return;
            var rt = c.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + delta);
            EditorUtility.SetDirty(rt);
        }

        Shift(subText);
        Shift(quoteText);
        if (retryButton != null) Shift(retryButton.transform.parent.GetComponent<RectTransform>());

        Debug.Log("[VoD] Đã nâng thêm +160 nữa (SubText/Quote/ButtonRow). Nhớ Save Scene.");
    }

    [MenuItem("VoD/Fix/1 - Scan Duplicate Objects")]
    public static void ScanDuplicates()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        var groups = new Dictionary<string, List<GameObject>>();

        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            // "Prop_X (1)" và "Prop_X" đều gom về chung 1 nhóm
            string baseName = Regex.Replace(root.name, @"\s*\(\d+\)$", "");
            if (!groups.ContainsKey(baseName))
                groups[baseName] = new List<GameObject>();
            groups[baseName].Add(root);
        }

        var duplicateGroups = groups.Where(g => g.Value.Count > 1).ToList();

        if (duplicateGroups.Count == 0)
        {
            Debug.Log("[VoD] Không thấy object trùng tên nào ở cấp root trong scene hiện tại.");
            return;
        }

        Debug.Log($"[VoD] ═══ Tìm thấy {duplicateGroups.Count} nhóm object trùng tên — chi tiết bên dưới ═══");

        foreach (var group in duplicateGroups)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[VoD] ── Nhóm \"{group.Key}\" — {group.Value.Count} bản ──");

            foreach (GameObject go in group.Value)
            {
                sb.AppendLine($"  • \"{go.name}\" tại vị trí {go.transform.position}, active={go.activeSelf}");

                var scriptComponents = go.GetComponentsInChildren<Component>(true).Where(c => c != null && !(c is Transform)).ToList();
                sb.AppendLine($"      Tổng component (kể cả con): {scriptComponents.Count}");

                foreach (var comp in scriptComponents)
                {
                    string flag = (comp is Behaviour beh && !beh.enabled) ? " [DISABLED]" : "";
                    sb.AppendLine($"      - {comp.GetType().Name}{flag} (trên: {comp.gameObject.name})");
                }
            }

            Debug.Log(sb.ToString());
        }

        Debug.Log("[VoD] ═══ Hết — so sánh xong tự chọn bản nào giữ, bản nào xoá tay trong Hierarchy ═══");
    }

    [MenuItem("VoD/Fix/2 - Scan Missing References (All Scripts)")]
    public static void ScanMissingReferences()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        int totalMissing = 0;

        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue; // chính script bị missing (mất source code) cũng tính
                var so = new SerializedObject(mb);
                var prop = so.GetIterator();
                bool enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                    if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                    {
                        // objectReferenceInstanceIDValue != 0 nghĩa là CÓ trỏ tới 1 object nhưng object đó
                        // đã bị xoá/mất — khác với để trống (None) từ đầu.
                        Debug.LogWarning($"[VoD] MISSING REF: \"{mb.gameObject.name}\" ({mb.GetType().Name}).{prop.name} đang trỏ vào object đã bị xoá.", mb);
                        totalMissing++;
                    }
                }
            }
        }

        Debug.Log(totalMissing == 0
            ? "[VoD] Scan xong — không thấy reference nào bị mất trên toàn scene."
            : $"[VoD] Scan xong — tìm thấy {totalMissing} reference bị mất (xem các dòng cảnh báo phía trên, mỗi dòng bấm vào sẽ nhảy tới đúng object).");
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    [MenuItem("VoD/Fix/3 - Fix Selected Object's Redundant MeshCollider Lag")]
    public static void FixSelectedMeshColliderLag()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
        {
            Debug.LogError("[VoD] Chưa chọn object nào trong Hierarchy. Chọn object (VD Player hoặc GhostCube) rồi chạy lại.");
            return;
        }

        CharacterController cc = target.GetComponent<CharacterController>();
        MeshCollider mc = target.GetComponent<MeshCollider>();

        SkinnedMeshRenderer smr = target.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (smr != null && smr.sharedMesh != null)
        {
            Debug.Log($"[VoD] Mesh trên \"{target.name}\" (\"{smr.sharedMesh.name}\"): {smr.sharedMesh.triangles.Length / 3:N0} tam giác, {smr.sharedMesh.vertexCount:N0} vertex.");
        }

        if (target.transform.localScale != Vector3.one)
        {
            Debug.LogWarning($"[VoD] \"{target.name}\".localScale = {target.transform.localScale} — khác (1,1,1), có thể ảnh hưởng tính toán physics/camera. Kiểm tra có chủ đích không trước khi sửa.");
        }

        if (cc == null)
        {
            Debug.LogError($"[VoD] \"{target.name}\" không có CharacterController — KHÔNG xoá MeshCollider (có thể đang cần nó thật). Dừng, kiểm tra tay.");
            return;
        }

        if (mc == null)
        {
            Debug.Log($"[VoD] \"{target.name}\" không có MeshCollider — không cần làm gì thêm.");
            return;
        }

        Debug.Log($"[VoD] Xác nhận: \"{target.name}\" có CharacterController (đã lo va chạm di chuyển) + MeshCollider thừa (mesh {(mc.sharedMesh != null ? mc.sharedMesh.triangles.Length / 3 : 0):N0} tam giác dùng làm collision) → MeshCollider này gây \"Fast Midphase\" warning + lag.");

        Undo.DestroyObjectImmediate(mc);
        Debug.Log($"[VoD] Đã xoá MeshCollider thừa trên \"{target.name}\". CharacterController vẫn lo va chạm di chuyển bình thường.");

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    // Object không bao giờ nên prefab hoá — hệ thống/singleton/gameplay-core, không phải đồ vật rời.
    private static readonly string[] PrefabExcludeNames =
    {
        "Player", "Main Camera", "Directional Light", "Terrain", "GhostCube", "Canvas",
        "EventSystem", "GlobalVolume", "InventorySystem_GO", "Plane", "AmbientZone_Hallway"
    };

    [MenuItem("VoD/Fix/4 - Scan ALL Prefab Candidates (toàn scene)")]
    public static void ScanAllPrefabCandidates()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        var candidates = new List<GameObject>();
        CollectCandidates(activeScene.GetRootGameObjects(), candidates);

        Debug.Log($"[VoD] ═══ Tìm thấy {candidates.Count} object có thể prefab hoá (có Renderer, chưa phải prefab instance) ═══");
        foreach (var go in candidates)
        {
            bool alreadyPrefab = PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab;
            Debug.Log($"  • {GetPath(go.transform)}{(alreadyPrefab ? "  [ĐÃ LÀ PREFAB, bỏ qua khi convert]" : "")}");
        }
    }

    [MenuItem("VoD/Fix/5 - Convert ALL Safe Objects To Prefabs (toàn scene)")]
    public static void ConvertAllSafeToPrefabs()
    {
        const string prefabFolder = "Assets/_Project/Prefabs/AutoConverted";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "AutoConverted");
        }

        Scene activeScene = SceneManager.GetActiveScene();
        var candidates = new List<GameObject>();
        CollectCandidates(activeScene.GetRootGameObjects(), candidates);

        int converted = 0, skippedPrefab = 0, skippedRef = 0;

        foreach (GameObject go in candidates)
        {
            if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
            {
                skippedPrefab++;
                continue;
            }

            // Kiểm tra an toàn: script nào tham chiếu ra ngoài chính hierarchy object này không.
            bool hasSceneRef = false;
            string sceneRefDetail = "";
            foreach (MonoBehaviour mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                var so = new SerializedObject(mb);
                var prop = so.GetIterator();
                bool enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var val = prop.objectReferenceValue;
                    if (val == null || prop.propertyPath.Contains(".Array.data[")) continue;

                    bool isOutsideOwnHierarchy = val is Component c && !c.transform.IsChildOf(go.transform);
                    bool isSceneObject = val is GameObject g && g.scene.IsValid() && !g.transform.IsChildOf(go.transform);

                    if (isOutsideOwnHierarchy || isSceneObject)
                    {
                        hasSceneRef = true;
                        sceneRefDetail = $"{mb.GetType().Name}.{prop.name} → \"{val.name}\"";
                        break;
                    }
                }
                if (hasSceneRef) break;
            }

            if (hasSceneRef)
            {
                Debug.LogWarning($"[VoD] BỎ QUA \"{GetPath(go.transform)}\" — có tham chiếu ra ngoài scene ({sceneRefDetail}). Không prefab hoá liều.");
                skippedRef++;
                continue;
            }

            string safeName = go.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
            string path = $"{prefabFolder}/{safeName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.UserAction);
            Debug.Log($"[VoD] Đã prefab hoá \"{GetPath(go.transform)}\" → \"{path}\".");
            converted++;
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log($"[VoD] Xong — prefab hoá {converted} object mới. Bỏ qua {skippedPrefab} (đã là prefab sẵn) + {skippedRef} (có tham chiếu scene, xem cảnh báo phía trên).");
    }

    private static void CollectCandidates(GameObject[] roots, List<GameObject> result)
    {
        foreach (GameObject root in roots)
            CollectCandidatesRecursive(root, result, isTopLevel: true);
    }

    private static void CollectCandidatesRecursive(GameObject go, List<GameObject> result, bool isTopLevel)
    {
        if (PrefabExcludeNames.Contains(go.name)) return;
        if (go.name.StartsWith("──")) // nhóm tổ chức Hierarchy (ENVIRONMENT/FURNITURE/PROPS...) — không phải đồ vật
        {
            foreach (Transform child in go.transform)
                CollectCandidatesRecursive(child.gameObject, result, isTopLevel: true);
            return;
        }

        bool hasRenderer = go.GetComponent<MeshRenderer>() != null || go.GetComponent<SkinnedMeshRenderer>() != null;
        if (hasRenderer && isTopLevel)
        {
            result.Add(go);
            return; // không đi sâu vào con của 1 candidate đã nhận (tránh prefab lồng prefab)
        }

        // Object không có Renderer ở cấp này (VD 1 group rỗng) — vẫn duyệt tiếp xuống con tìm candidate
        if (!hasRenderer)
        {
            foreach (Transform child in go.transform)
                CollectCandidatesRecursive(child.gameObject, result, isTopLevel: true);
        }
    }

    [MenuItem("VoD/Fix/6 - Scan Chapter1 Ending Setup (kiểm tra flow kết thúc chương)")]
    public static void ScanChapter1Ending()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[VoD] ═══ Scan flow kết thúc Chapter1 ═══");

        var gameManager = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        sb.AppendLine(gameManager != null
            ? $"  • GameManager: CÓ trong scene ({GetPath(gameManager.transform)})"
            : "  • GameManager: KHÔNG có trong scene hiện tại (chỉ tồn tại nếu load từ MainMenu qua DontDestroyOnLoad).");

        var triggerZones = Object.FindObjectsByType<TriggerZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"  • TriggerZone trong scene: {triggerZones.Length}");
        foreach (var tz in triggerZones)
            sb.AppendLine($"      - {GetPath(tz.transform)}");

        var gazeTriggers = Object.FindObjectsByType<GazeTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"  • GazeTrigger trong scene: {gazeTriggers.Length}");
        foreach (var gt in gazeTriggers)
            sb.AppendLine($"      - {GetPath(gt.transform)}");

        var wellDeath = Object.FindObjectsByType<WellDeathSequence>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"  • WellDeathSequence trong scene: {wellDeath.Length}");
        foreach (var w in wellDeath)
            sb.AppendLine($"      - {GetPath(w.transform)}");

        var cutscenes = Object.FindObjectsByType<CutsceneController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"  • CutsceneController trong scene: {cutscenes.Length}");
        foreach (var cs in cutscenes)
            sb.AppendLine($"      - {GetPath(cs.transform)}");

        // Kiểm tra Build Settings còn scene nào ngoài MainMenu/Chapter1 không (Chapter2-4 đã bị xoá khỏi git).
        var buildScenes = EditorBuildSettings.scenes;
        sb.AppendLine($"  • Scene trong Build Settings ({buildScenes.Length}):");
        foreach (var s in buildScenes)
            sb.AppendLine($"      - [{(s.enabled ? "ON " : "OFF")}] {s.path}");

        sb.AppendLine("  → Không tìm thấy TriggerZone/logic nào gọi GameManager.LoadChapter(2) trong toàn bộ codebase (đã grep) — nghĩa là hiện KHÔNG có màn hình/flow kết thúc Chapter1 nào được wire, dù chơi xong nội dung vẫn có thể đi lại tự do, không có gì xảy ra.");

        Debug.Log(sb.ToString());
    }

    [MenuItem("VoD/Fix/7 - Clear External Refs + Convert 3 Props Còn Sót (Key01/Portrait/Frame)")]
    public static void ClearExternalRefsOnSkippedProps()
    {
        string[] names = { "Prop_Key01_Skeleton", "Prop_Portrait_Family", "Prop_Frame_Portrait" };
        int cleared = 0;

        foreach (string name in names)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"[VoD] Không tìm thấy \"{name}\" — bỏ qua.");
                continue;
            }

            foreach (MonoBehaviour mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                var so = new SerializedObject(mb);
                var prop = so.GetIterator();
                bool enterChildren = true;
                bool changed = false;

                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var val = prop.objectReferenceValue;
                    if (val == null || prop.propertyPath.Contains(".Array.data[")) continue;

                    bool isOutsideOwnHierarchy = val is Component c && !c.transform.IsChildOf(go.transform);
                    bool isSceneObject = val is GameObject g && g.scene.IsValid() && !g.transform.IsChildOf(go.transform);

                    if (isOutsideOwnHierarchy || isSceneObject)
                    {
                        Debug.Log($"[VoD] Xoá tham chiếu ra ngoài scene: \"{mb.gameObject.name}\" ({mb.GetType().Name}).{prop.name} (đang trỏ \"{val.name}\") — tự resolve qua Instance lúc chạy.");
                        prop.objectReferenceValue = null;
                        changed = true;
                        cleared++;
                    }
                }

                if (changed)
                {
                    Undo.RecordObject(mb, "Clear external refs before prefab-ify");
                    so.ApplyModifiedProperties();
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[VoD] Đã dọn {cleared} tham chiếu ra ngoài scene trên 3 Prop. Chạy tiếp \"5 - Convert ALL Safe Objects To Prefabs\" để prefab hoá nốt.");
    }

    [MenuItem("VoD/Fix/8 - Import PauseMenu + DeathScreen (từ TestQA)")]
    public static void ImportPauseMenuAndDeathScreen()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "Chapter1")
        {
            Debug.LogError("[VoD] Cần mở Chapter1.unity làm scene chính trước khi chạy tool này.");
            return;
        }

        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("[VoD] Không tìm thấy Canvas trong Chapter1."); return; }

        Transform oldPause = canvasGO.transform.Find("PauseMenu");
        if (oldPause != null) Undo.DestroyObjectImmediate(oldPause.gameObject);
        Transform oldDeath = canvasGO.transform.Find("DeathScreen");
        if (oldDeath != null) Undo.DestroyObjectImmediate(oldDeath.gameObject);

        // Dùng scene test của nhóm (TestQA) — không lấy từ Chapter1_backup nữa.
        string sourcePath = "Assets/_Project/Scenes/TestQA.unity";
        Scene sourceScene = EditorSceneManager.OpenScene(sourcePath, OpenSceneMode.Additive);

        GameObject sourcePause = null, sourceDeath = null;
        foreach (GameObject root in sourceScene.GetRootGameObjects())
        {
            if (sourcePause == null) sourcePause = FindDeepByName(root.transform, "PauseMenu");
            if (sourceDeath == null) sourceDeath = FindDeepByName(root.transform, "DeathScreen");
        }

        int imported = 0;
        if (sourcePause != null)
        {
            var copy = Object.Instantiate(sourcePause);
            copy.name = "PauseMenu";
            SceneManager.MoveGameObjectToScene(copy, activeScene);
            copy.transform.SetParent(canvasGO.transform, false);
            Undo.RegisterCreatedObjectUndo(copy, "Import PauseMenu");
            imported++;
        }
        else Debug.LogWarning("[VoD] Không tìm thấy GameObject \"PauseMenu\" trong TestQA.");

        if (sourceDeath != null)
        {
            var copy = Object.Instantiate(sourceDeath);
            copy.name = "DeathScreen";
            SceneManager.MoveGameObjectToScene(copy, activeScene);
            copy.transform.SetParent(canvasGO.transform, false);
            Undo.RegisterCreatedObjectUndo(copy, "Import DeathScreen");
            imported++;
        }
        else Debug.LogWarning("[VoD] Không tìm thấy GameObject \"DeathScreen\" trong TestQA.");

        EditorSceneManager.CloseScene(sourceScene, true);
        SceneManager.SetActiveScene(activeScene);

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log($"[VoD] Đã import {imported}/2 panel (PauseMenu, DeathScreen) từ TestQA vào Canvas của Chapter1 hiện tại. Cả 2 script đều tự-chứa (không tham chiếu object ngoài), nên copy trực tiếp là an toàn — không cần relink gì thêm.");
    }

    private static GameObject FindDeepByName(Transform t, string name)
    {
        if (t.name == name) return t.gameObject;
        foreach (Transform child in t)
        {
            var found = FindDeepByName(child, name);
            if (found != null) return found;
        }
        return null;
    }

}
