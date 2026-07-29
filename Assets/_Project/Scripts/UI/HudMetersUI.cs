using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Polish UI tối giản (Jok duyệt qua mockup) -- 2 vạch mảnh Thể lực/Đèn pin góc dưới trái, thay 2 hệ thống
// HUD cũ (StaminaBreathUI: thanh "thở" scale, FlashlightBatteryUI: đổi màu icon) -- ĐÃ XOÁ 2 script đó,
// đây là script DUY NHẤT quản lý cả 2 vạch.
//
// SỬA 2026-07-28: Bản đầu dùng VerticalLayoutGroup/HorizontalLayoutGroup lồng nhau, quên set
// childControlWidth/Height=true ở 1 tầng -- Unity mặc định KHÔNG kiểm soát kích thước con nếu thiếu cờ đó,
// khiến chữ nhãn/% rơi về RectTransform mặc định (100x100 giữa màn hình) chồng lấn lung tung. Viết lại
// bằng TOẠ ĐỘ NEO CỐ ĐỊNH tính tay trực tiếp cho từng phần tử -- không phụ thuộc LayoutGroup nữa, chắc
// chắn đúng vị trí mọi lúc, dễ đọc/sửa hơn hẳn so với đoán xem LayoutGroup tự tính ra sao.
//
// SỬA 2026-07-28 (2): Trước đây tạo GameObject bằng code CHỈ lúc Play (Instance getter) -- không xem được
// ở Edit Mode dù chỉ là xem layout tĩnh, khác hẳn FlashlightController (đã dùng [ExecuteAlways] sẵn trong
// project). Giờ theo ĐÚNG pattern đó: [ExecuteAlways] + build UI ngay khi object được ĐẶT SẴN TRONG SCENE
// (qua VoD_EmbedRuntimeUIInScene.cs, chạy 1 lần) -- bật/tắt GameObject này trong Hierarchy là xem layout
// ngay, không cần bấm Play.
[ExecuteAlways]
public class HudMetersUI : MonoBehaviour
{
    private static HudMetersUI _instance;
    public static HudMetersUI Instance
    {
        get
        {
            // Ưu tiên tìm object đã có sẵn trong scene (đặt qua VoD_EmbedRuntimeUIInScene.cs) -- chỉ tạo
            // mới bằng code nếu vì lý do gì đó chưa từng đặt (fallback, không phải đường chính).
            if (_instance == null) _instance = FindFirstObjectByType<HudMetersUI>(FindObjectsInactive.Include);
            if (_instance == null)
            {
                var go = new GameObject("HudMetersUI");
                _instance = go.AddComponent<HudMetersUI>();
            }
            return _instance;
        }
    }

    // SỬA 2026-07-28: Số cũ (27/14/28/26/220) đoán bừa không qua tính toán gì -- giờ tính lại đúng từ
    // mockup đã duyệt (khung tham chiếu 1180px: left=24 bottom=22 width=200 gap=10 track=4 label font=10),
    // scale lên đúng CanvasScaler thật 1920px (hệ số 1920/1180 = 1.627).
    // SỬA 2026-07-28 (Jok yêu cầu): chữ vẫn nhỏ quá dù đã tính đúng tỉ lệ mockup -- nhân thêm x3 (giống
    // cách đã áp cho Examine), tăng khung/khoảng cách tương ứng cho khỏi chật/đè chữ.
    private const float SmoothSpeed = 8f;
    private const float RowHeight = 70f;   // đủ chỗ cho chữ x3 + track dày hơn
    // SỬA (Jok yêu cầu -- "muốn spacing ra, cách nhau 1 khoảng thuận mắt hơn"): 34 -> 60, 2 hàng Thể lực/Đèn
    // pin đứng sát nhau quá, tăng khoảng cách rõ rệt hơn hẳn thay vì chỉnh nhích từng chút.
    private const float RowGap    = 60f;
    private const float PanelX    = 39f;   // mockup 24px * 1.627
    private const float PanelY    = 36f;   // mockup 22px * 1.627
    private const float PanelWidth = 460f; // rộng hơn cho chữ to hơn không bị cắt

    // SỬA 2026-07-28 (Jok chốt lại cụ thể): 2 thanh dùng 2 bộ màu RIÊNG, không chung công thức.
    //   - Thể lực: xanh nước biển đậm (đầy) --gradient mượt--> đỏ (cạn), giống cảnh báo dần đều.
    //   - Đèn pin: sáng/trắng (đầy) -> vàng SÁNG (giữa) -> đỏ (cạn), nhảy bậc rõ ràng.
    private static readonly Color StaminaFullColor  = new Color(0.14f, 0.28f, 0.52f, 1f); // xanh nước biển đậm
    private static readonly Color BatteryFullColor  = new Color(0.92f, 0.93f, 0.88f, 1f); // sáng/gần trắng
    private static readonly Color BatteryWarningColor = new Color(0.95f, 0.78f, 0.15f, 1f); // vàng sáng, rực hơn hẳn tông vàng gold trầm cũ
    private static readonly Color DangerFill   = new Color(0.82f, 0.345f, 0.29f, 1f);
    // SỬA 2026-07-28 (Jok yêu cầu): HUD là UI phủ lên trên, KHÔNG bị tối theo ánh sáng cảnh 3D -- nhưng nếu
    // màu CHỮ tự nó cũng tối/trầm (0.55 cũ) thì vẫn dễ chìm khi nền cảnh thật sự tối. Sáng hẳn lên cho chắc
    // ăn về độ tương phản trong bóng tối, kể cả lúc scene sáng như grass ở ảnh test cũng không bị chói.
    private static readonly Color LabelColor   = new Color(0.85f, 0.83f, 0.78f, 1f);

    private CanvasGroup _canvasGroup;
    private Image _staminaFill;
    private Image _batteryFill;
    private TextMeshProUGUI _staminaPct;
    private TextMeshProUGUI _batteryPct;

    private PlayerController _player;
    private FlashlightController _flashlight;
    private float _staminaDisplayed = 1f;
    private float _batteryDisplayed = 1f;

    private bool _built;

    // OnEnable() thay vì Awake() -- chạy được ở CẢ Edit Mode lẫn Play Mode nhờ [ExecuteAlways], và chạy
    // lại mỗi lần Jok bật GameObject này lên trong Hierarchy để xem thử (không chỉ 1 lần duy nhất).
    private void OnEnable()
    {
        if (_instance != null && _instance != this && Application.isPlaying) { Destroy(gameObject); return; }
        _instance = this;
        // KHÔNG DontDestroyOnLoad -- HUD này LUÔN hiện mặc định (khác TutorialHintUI/ExamineStageUI chỉ
        // hiện khi được gọi), nếu sống xuyên scene sẽ kẹt lại đè lên MainMenu lúc PauseMenuUI.GoToMainMenu()
        // đổi scene.
        if (!_built) { BuildUI(); _built = true; }

        if (Application.isPlaying)
        {
            _player = PlayerController.Instance;
            _flashlight = FindFirstObjectByType<FlashlightController>();

            // BUG THẬT (Jok phát hiện): HUD phải LUÔN hiện mặc định (khác Examine/TutorialHint chỉ hiện lúc
            // Show()), ẩn/hiện CHỈ qua CanvasGroup.alpha (SetVisible()) -- KHÔNG qua SetActive của Canvas.
            // Nhưng Jok từng tắt "HudMetersUI_Canvas" để dọn Hierarchy lúc edit/preview, và UIBootstrap CỐ
            // Ý không ép bật con cấp sâu (đúng cho Examine/TutorialHint, sai cho HUD) -- Canvas kẹt luôn ở
            // trạng thái tắt suốt cả ván chơi, y hệt bug ScreenFader/BlackImage trước đây. Tự đảm bảo ở đây,
            // giống ScreenFader.Awake() tự bật lại con của chính nó.
            var hudCanvas = transform.Find("HudMetersUI_Canvas");
            if (hudCanvas != null && !hudCanvas.gameObject.activeSelf)
                hudCanvas.gameObject.SetActive(true);
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }
    }

    private void OnDisable()
    {
        if (_instance == this) _instance = null;
    }

    private void BuildUI()
    {
        // Đã dựng rồi (VD OnEnable chạy lại sau domain reload) -- không dựng chồng lần 2.
        if (transform.Find("HudMetersUI_Canvas") != null) return;

        var canvasGO = new GameObject("HudMetersUI_Canvas");
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // dưới Tutorial Hint (500)/Examine (500)/Death (1000), trên HUD nền
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var rootGO = new GameObject("MetersRoot", typeof(RectTransform));
        rootGO.transform.SetParent(canvasGO.transform, false);
        var rootRt = rootGO.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0f, 0f);
        rootRt.anchorMax = new Vector2(0f, 0f);
        rootRt.pivot     = new Vector2(0f, 0f);
        rootRt.anchoredPosition = new Vector2(PanelX, PanelY);
        rootRt.sizeDelta = new Vector2(PanelWidth, RowHeight * 2f + RowGap);

        _canvasGroup = rootGO.AddComponent<CanvasGroup>();

        // Hàng dưới = Đèn pin (y=0), hàng trên = Thể lực (y=RowHeight+RowGap) -- toạ độ neo tay, không LayoutGroup.
        BuildMeterRow(rootGO.transform, 0f,                 "ĐÈN PIN", out _batteryFill, out _batteryPct);
        BuildMeterRow(rootGO.transform, RowHeight + RowGap, "THỂ LỰC", out _staminaFill, out _staminaPct);
    }

    private void BuildMeterRow(Transform parent, float rowY, string label, out Image fill, out TextMeshProUGUI pctText)
    {
        var rowGO = new GameObject($"Meter_{label}", typeof(RectTransform));
        rowGO.transform.SetParent(parent, false);
        var rowRt = rowGO.GetComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0f, 0f);
        rowRt.anchorMax = new Vector2(0f, 0f);
        rowRt.pivot     = new Vector2(0f, 0f);
        rowRt.anchoredPosition = new Vector2(0f, rowY);
        rowRt.sizeDelta = new Vector2(PanelWidth, RowHeight);

        // Nhãn -- neo trái, cố định trong hàng.
        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(rowGO.transform, false);
        var labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.enableAutoSizing = false; // đề phòng project có preset TMP mặc định bật Auto Size
        labelText.text = label;
        labelText.fontSize = 42; // x3 gốc scale mockup (16 * ~2.6, làm tròn cho dễ đọc)
        labelText.fontStyle = FontStyles.Bold;
        labelText.characterSpacing = 8f; // rộng hơn hẳn (trước 3) -- kiểu letter-spacing thoáng của web
        var notoFont = VoDFontUtil.FindNotoSansFont();
        if (notoFont != null) labelText.font = notoFont;
        labelText.color = LabelColor;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        var labelRt = labelText.rectTransform;
        labelRt.anchorMin = new Vector2(0f, 1f);
        labelRt.anchorMax = new Vector2(0f, 1f);
        labelRt.pivot     = new Vector2(0f, 1f);
        // SỬA (Jok yêu cầu -- "chữ và số 100% cao hơn 1 tí so với thanh"): trước đây y=0 nghĩa là đáy chữ
        // chạm THẲNG đỉnh track (70-50=20, track cao 0->20 -- chạm khít, 0 khoảng hở). Nhích lên +14 để có
        // khoảng thở rõ ràng giữa chữ và thanh -- RowGap đã tăng lên 60 nên vẫn dư chỗ, không đè hàng trên.
        labelRt.anchoredPosition = new Vector2(0f, 14f);
        labelRt.sizeDelta = new Vector2(280f, 50f);

        // % -- neo phải, cùng hàng ngang với nhãn.
        var pctGO = new GameObject("Pct", typeof(RectTransform));
        pctGO.transform.SetParent(rowGO.transform, false);
        pctText = pctGO.AddComponent<TextMeshProUGUI>();
        pctText.enableAutoSizing = false; // đề phòng project có preset TMP mặc định bật Auto Size
        pctText.text = "100%";
        pctText.fontSize = 42;
        pctText.fontStyle = FontStyles.Bold;
        pctText.characterSpacing = 2f;
        if (notoFont != null) pctText.font = notoFont;
        pctText.color = LabelColor;
        pctText.alignment = TextAlignmentOptions.MidlineRight;
        var pctRt = pctText.rectTransform;
        pctRt.anchorMin = new Vector2(1f, 1f);
        pctRt.anchorMax = new Vector2(1f, 1f);
        pctRt.pivot     = new Vector2(1f, 1f);
        pctRt.anchoredPosition = new Vector2(0f, 14f); // cùng khoảng thở +14 với Label
        pctRt.sizeDelta = new Vector2(120f, 50f);

        // Track (nền mờ, kéo giãn hết chiều rộng hàng) -- đặt SÁT ĐÁY hàng, dưới dòng chữ.
        var trackGO = new GameObject("Track", typeof(RectTransform));
        trackGO.transform.SetParent(rowGO.transform, false);
        var trackImg = trackGO.AddComponent<Image>();
        trackImg.color = new Color(1f, 1f, 1f, 0.08f);
        var trackRt = trackImg.rectTransform;
        trackRt.anchorMin = new Vector2(0f, 0f);
        trackRt.anchorMax = new Vector2(1f, 0f);
        trackRt.pivot     = new Vector2(0.5f, 0f);
        trackRt.anchoredPosition = Vector2.zero;
        // SỬA (Jok yêu cầu -- "thanh cao lên luôn"): 14 -> 20, dày hơn hẳn cho cân đối với khoảng cách hàng
        // mới rộng ra -- width = 0 vì anchorMin/Max.x kéo giãn theo cha rồi.
        trackRt.sizeDelta = new Vector2(0f, 20f);

        // Fill -- con của Track, co giãn theo % thật qua Image.Type.Filled (KHÔNG dùng scale/LayoutGroup).
        var fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(trackGO.transform, false);
        fill = fillGO.AddComponent<Image>();
        fill.color = BatteryFullColor; // màu tạm lúc dựng -- Update() lúc Play sẽ tự ghi đè đúng theo từng thanh ngay khung hình đầu
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        var fillRt = fill.rectTransform;
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        // Edit Mode -- không có _player/_flashlight thật đang chạy (Stamina01/BatteryLevel01 chỉ có ý
        // nghĩa lúc Play), giữ nguyên mặc định 100% tĩnh cho Jok xem layout, không cố tính toán gì thêm.
        if (!Application.isPlaying) return;

        if (_player != null)
        {
            float target = _player.Stamina01;
            _staminaDisplayed = Mathf.Lerp(_staminaDisplayed, target, Time.deltaTime * SmoothSpeed);
            ApplyStaminaMeter(_staminaFill, _staminaPct, _staminaDisplayed, target);
        }

        if (_flashlight != null)
        {
            float target = _flashlight.BatteryLevel01;
            _batteryDisplayed = Mathf.Lerp(_batteryDisplayed, target, Time.deltaTime * SmoothSpeed);
            ApplyBatteryMeter(_batteryFill, _batteryPct, _batteryDisplayed, target);
        }
    }

    // Thể lực -- Jok yêu cầu: xanh nước biển đậm lúc đầy, ĐỎ lúc cạn, chuyển màu MƯỢT (gradient liên tục)
    // thay vì nhảy bậc như đèn pin -- đúng cảm giác "tụt dần đều" của thể lực.
    private void ApplyStaminaMeter(Image fill, TextMeshProUGUI pctText, float smoothed, float raw)
    {
        if (fill == null) return;
        fill.fillAmount = smoothed;
        fill.color = Color.Lerp(DangerFill, StaminaFullColor, raw);
        if (pctText != null) pctText.text = Mathf.RoundToInt(raw * 100f) + "%";
    }

    // Đèn pin -- Jok yêu cầu: sáng (light, gần trắng) lúc đầy, vàng SÁNG lúc giữa chừng, đỏ lúc cạn --
    // NHẢY BẬC rõ ràng (không mượt) để khớp đúng cảm giác đèn pin thật nhấp nháy/yếu dần theo từng mốc pin
    // (giống cách FlashlightController.cs đã chia mốc rõ ràng, không lerp liên tục).
    private void ApplyBatteryMeter(Image fill, TextMeshProUGUI pctText, float smoothed, float raw)
    {
        if (fill == null) return;
        fill.fillAmount = smoothed;
        fill.color = raw <= 0.15f ? DangerFill : (raw <= 0.3f ? BatteryWarningColor : BatteryFullColor);
        if (pctText != null) pctText.text = Mathf.RoundToInt(raw * 100f) + "%";
    }

    /// <summary>Ẩn/hiện cả 2 vạch cùng lúc -- gọi từ Inventory/Examine giống InteractPromptUI.SetDotVisible.</summary>
    public void SetVisible(bool visible)
    {
        if (_canvasGroup != null) _canvasGroup.alpha = visible ? 1f : 0f;
    }
}
