using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

// Đọc nhật ký nhiều trang -- HOÀN TOÀN KHÁC ExamineItem (xoay 3D): chỉ mở được từ trong Inventory (đúng
// yêu cầu Jok "khi nhặt rồi vào inventory thì mới có kiểu tương tác đọc"), hiện slideshow PNG từng trang,
// Next/Prev tự do (không ép tốc độ đọc). Bắt buộc đọc tới trang CUỐI mới coi là "đã đọc xong" -- lúc đó
// mới tự chạy lời thoại phản ứng (3 câu ngắn, hoang mang -- đã viết sẵn trong story bible Phần IX) + bắn
// thêm UnityEvent để Jok tự gắn SFX khác qua Inspector, không cần sửa code.
//
// SỬA 2026-07-29 (Jok phát hiện): trước đây build UI (tiêu đề/counter/2 góc hint) trong Awake() -- Awake()
// CHỈ chạy lúc Play, khác hẳn ExamineStageUI/HudMetersUI/TutorialHintUI/InteractPromptUI đều dùng
// [ExecuteAlways] để xem được ngay trong Editor không cần bấm Play. Đổi sang ĐÚNG pattern đó cho nhất quán.
[ExecuteAlways]
public class DiaryReaderUI : MonoBehaviour
{
    // Singleton theo đúng pattern DialogueUI/InventoryUI/AudioManager... -- cho phép trigger zone (VD
    // DiaryReactionCutsceneTrigger) check HasFinishedReading từ bên ngoài mà không cần kéo tay reference.
    public static DiaryReaderUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _pageImage;

    [Tooltip("Hiện 'trang hiện tại / tổng số trang' -- để trống thì BuildVisuals() tự tạo lúc OnEnable()")]
    [SerializeField] private TextMeshProUGUI _pageCounterText;

    [Tooltip("Các trang PNG theo đúng thứ tự thời gian (8-3 -> 22-6 -> 14-9 -> 30-10)")]
    [SerializeField] private Sprite[] _pages;

    [Tooltip("Tiếng lật trang -- Jok tự tìm/gán sau")]
    [SerializeField] private AudioClip _pageFlipSfx;

    [Tooltip("Lời thoại phản ứng tự chạy ngay khi đọc tới trang cuối (không cần wire qua UnityEvent)")]
    [SerializeField] private DialogueAsset _reactionDialogue;

    [Tooltip("Thêm hook cho SFX/hiệu ứng khác lúc đọc xong -- Jok tự gắn qua Inspector nếu cần, không bắt buộc")]
    public UnityEvent OnFinishedReading;

    private int _currentPage;
    private TextMeshProUGUI _cornerNavText; // "A / D lật trang" -- cần ref riêng để tô màu động theo trang
    public bool HasFinishedReading { get; private set; }
    public bool IsOpen => _panel != null && _panel.activeSelf;

    private void Update()
    {
        // Edit Mode chỉ để xem layout tĩnh (tiêu đề/counter/2 góc hint) -- A/D lật trang + Chuột phải thoát
        // cần input thật, chỉ chạy lúc Play, giống đúng convention Examine/HUD/TutorialHint.
        if (!Application.isPlaying) return;
        if (_panel == null || !_panel.activeSelf) return;

        // Chuột Phải để thoát -- khớp đúng convention ExamineItem đang dùng khi mở từ Inventory.
        if (Input.GetMouseButtonDown(1)) { Close(); return; }

        // SỬA (Jok yêu cầu): A/D lật trang thay cho 2 nút bấm trái/phải trên UI (đã bỏ khỏi thiết kế).
        if (Input.GetKeyDown(KeyCode.D)) NextPage();
        if (Input.GetKeyDown(KeyCode.A)) PrevPage();
    }

    // OnEnable() thay vì Awake() -- chạy được ở CẢ Edit Mode lẫn Play Mode nhờ [ExecuteAlways], và chạy lại
    // mỗi lần Jok bật GameObject này lên trong Hierarchy/Preview Window để xem thử.
    private void OnEnable()
    {
        Instance = this;

        // SỬA (Jok yêu cầu -- "xoá 3 nút này được không"): PrevButton/NextButton/CloseButton đã xoá hẳn khỏi
        // scene, thay bằng A/D + góc hint "THOÁT/Chuột phải". Field/listener liên quan cũng dọn sạch luôn,
        // không giữ lại tham chiếu tới object không còn tồn tại.
        BuildVisuals();

        // AN TOÀN: phòng Jok bật "_panel" lên xem layout ở Edit Mode rồi quên tắt lại trước khi bấm Play --
        // ép về ẩn mỗi lần vào Play thật, chỉ Open() mới được bật lại, không phụ thuộc trạng thái Jok để lại
        // lúc preview. Giữ nguyên trạng thái Jok để ở Edit Mode (không ép tắt) để Preview Window dùng được.
        if (Application.isPlaying && _panel != null) _panel.SetActive(false);
    }

    // Mockup đã duyệt (artifact "Màn Đọc Nhật Ký") mới chỉ áp phần CHỨC NĂNG (A/D, counter) -- còn thiếu
    // toàn bộ phần HÌNH: tiêu đề "NHẬT KÝ", 2 cụm hint góc dưới (điều hướng A/D + thoát Chuột phải). Tự dựng
    // bằng code (GET-OR-CREATE, giống ExamineStageUI/InteractPromptUI) ngay trên _panel có sẵn -- không đụng
    // tới _pageImage/background gốc Phúc đã dựng tay, chỉ THÊM lớp trang trí xung quanh.
    private void BuildVisuals()
    {
        if (_panel == null) return;
        var notoFont = VoDFontUtil.FindNotoSansFont();
        var panelT = _panel.transform;

        // XÓA object hỏng từ lần build lỗi trước (TextMeshProUGUI + Image chung 1 GameObject -- Unity không
        // cho phép, AddComponent<Image> luôn trả null) -- không dọn thì rác này nằm lại trong scene mãi mãi.
        var stale = panelT.Find("PageCounterText");
        if (stale != null)
        {
            if (Application.isPlaying) Destroy(stale.gameObject);
            else DestroyImmediate(stale.gameObject);
        }

        // SỬA (Jok phát hiện -- NullReferenceException ở counter chặn đứng CẢ 2 góc hint phía sau không bao
        // giờ chạy tới): 4 phần tử (tiêu đề/counter/2 góc hint) ĐỘC LẬP với nhau, không phần nào phụ thuộc
        // phần nào -- bọc riêng try-catch cho từng phần để 1 chỗ lỗi KHÔNG kéo sập toàn bộ các phần còn lại.
        try { BuildTitle(panelT, notoFont); } catch (System.Exception e) { Debug.LogException(e); }
        try { BuildCounter(panelT, notoFont); } catch (System.Exception e) { Debug.LogException(e); }
        try
        {
            _cornerNavText = BuildCornerHint(panelT, "CornerNav",  TextAlignmentOptions.BottomLeft,  new Vector2(0f, 0f), new Vector2(39f, 39f),
                "LẬT TRANG", "A / D", notoFont); // ghi đè ngay bởi UpdateNavHintColors() bên dưới, chỉ là placeholder ban đầu
            BuildCornerHint(panelT, "CornerExit", TextAlignmentOptions.BottomRight, new Vector2(1f, 0f), new Vector2(-39f, 39f),
                "THOÁT", "Chuột phải", notoFont);
        }
        catch (System.Exception e) { Debug.LogException(e); }

        UpdateNavHintColors(); // đặt sẵn màu placeholder (Edit Mode) -- RefreshPage() sẽ tính lại đúng lúc Play

        // SỬA (Jok yêu cầu -- "giảm size khu vực png lại 1 ít để chữ nhật ký nhìn được"): trang PNG hiện quá
        // sát tiêu đề "NHẬT KÝ" ở trên. Không biết chính xác anchor/sizeDelta gốc Phúc đặt tay (không đoán mù
        // số liệu) -- co lại bằng localScale quanh tâm, chừa khoảng trống mọi phía kể cả phía trên, không cần
        // biết số liệu gốc. Set thẳng (không cộng dồn) nên gọi lại nhiều lần vẫn an toàn, không co nhỏ dần mãi.
        if (_pageImage != null) _pageImage.rectTransform.localScale = Vector3.one * 0.85f;
    }

    // Tiêu đề "NHẬT KÝ" -- eyebrow trên đầu, giống style "ĐANG XEM" của ExamineStageUI. Không có ngày tháng
    // động kèm theo vì nội dung đó đã nằm sẵn trong chính ảnh PNG từng trang, không có dữ liệu riêng để hiển
    // thị lại ở đây.
    // SỬA (Jok yêu cầu -- x3 cỡ chữ giống Examine): 26 -> 78, box cao hơn cho đủ chỗ.
    private static void BuildTitle(Transform panelT, TMP_FontAsset notoFont)
    {
        var titleT = panelT.Find("DiaryTitle");
        GameObject titleGO = titleT != null ? titleT.gameObject : new GameObject("DiaryTitle", typeof(RectTransform));
        titleGO.transform.SetParent(panelT, false);
        var title = titleGO.GetComponent<TextMeshProUGUI>();
        if (title == null) title = titleGO.AddComponent<TextMeshProUGUI>();
        title.enableAutoSizing = false;
        title.fontSize = 78;
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 8f;
        title.alignment = TextAlignmentOptions.Top;
        title.color = new Color(0.8f, 0.71f, 0.53f, 1f);
        if (notoFont != null) title.font = notoFont;
        title.text = "NHẬT KÝ";
        var titleRt = titleGO.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot     = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(900f, 130f);
        titleRt.anchoredPosition = new Vector2(0f, -39f);
    }

    // Counter "X / Y" -- góc trên phải, giữ đúng vị trí Jok đã duyệt ở mockup.
    // SỬA (Jok phát hiện -- Image+TMP chung 1 GameObject KHÔNG được, cùng là Graphic): tách "PageCounterPill"
    // (nền, layout) làm CHA, "PageCounterLabel" (chữ) làm CON riêng -- giống đúng pattern KeyBadge/KeyText đã
    // dùng ở TutorialHintUI/InteractPromptUI, không lặp lại lỗi cũ.
    // SỬA (Jok yêu cầu -- x3 cỡ chữ): 20 -> 60.
    private void BuildCounter(Transform panelT, TMP_FontAsset notoFont)
    {
        var pillT = panelT.Find("PageCounterPill");
        GameObject pillGO = pillT != null ? pillT.gameObject : new GameObject("PageCounterPill", typeof(RectTransform));
        pillGO.transform.SetParent(panelT, false);

        var bg = pillGO.GetComponent<Image>();
        if (bg == null) bg = pillGO.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.035f, 0.03f, 0.94f);
        bg.sprite = VoDUISpriteUtil.CreateRoundedSprite(64, 64, 24);
        bg.type = Image.Type.Sliced;

        var layout = pillGO.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = pillGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.padding = new RectOffset(28, 28, 12, 12);
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = pillGO.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = pillGO.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var pillRt = pillGO.GetComponent<RectTransform>();
        pillRt.anchorMin = new Vector2(1f, 1f);
        pillRt.anchorMax = new Vector2(1f, 1f);
        pillRt.pivot     = new Vector2(1f, 1f);
        pillRt.anchoredPosition = new Vector2(-39f, -39f);

        var labelT = pillGO.transform.Find("PageCounterLabel");
        GameObject labelGO = labelT != null ? labelT.gameObject : new GameObject("PageCounterLabel", typeof(RectTransform));
        labelGO.transform.SetParent(pillGO.transform, false);
        _pageCounterText = labelGO.GetComponent<TextMeshProUGUI>();
        if (_pageCounterText == null) _pageCounterText = labelGO.AddComponent<TextMeshProUGUI>();
        _pageCounterText.enableAutoSizing = false;
        _pageCounterText.fontSize = 60;
        _pageCounterText.fontStyle = FontStyles.Bold;
        _pageCounterText.alignment = TextAlignmentOptions.MidlineRight;
        _pageCounterText.color = new Color(0.85f, 0.83f, 0.78f, 1f);
        if (notoFont != null) _pageCounterText.font = notoFont;
        if (string.IsNullOrEmpty(_pageCounterText.text)) _pageCounterText.text = "1 / 1"; // placeholder cho preview Edit Mode
    }

    // SỬA (Jok yêu cầu -- x3 cỡ chữ): 18 -> 54, box to hơn. Trả về TMP để BuildVisuals() lưu ref riêng cho
    // CornerNav (cần tô màu A/D động theo trang, xem UpdateNavHintColors()).
    private static TextMeshProUGUI BuildCornerHint(Transform parent, string goName, TextAlignmentOptions align, Vector2 anchor,
        Vector2 offset, string label, string value, TMP_FontAsset notoFont)
    {
        var t = parent.Find(goName);
        GameObject go = t != null ? t.gameObject : new GameObject(goName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        if (text == null) text = go.AddComponent<TextMeshProUGUI>();
        text.enableAutoSizing = false;
        text.text = $"<size=86%><color=#8a713f>{label}</color></size>\n{value}";
        text.fontSize = 54;
        text.fontStyle = FontStyles.Bold;
        if (notoFont != null) text.font = notoFont;
        text.lineSpacing = 16f;
        text.color = new Color(0.78f, 0.74f, 0.66f, 0.95f);
        text.alignment = align;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.sizeDelta = new Vector2(560f, 150f);
        rt.anchoredPosition = offset;
        return text;
    }

    // SỬA (Jok yêu cầu -- "ở 1/13 thì A sẽ tắt màu, D có màu, y trang W/S của Inventory"): trang đầu thì "A"
    // (lùi) tối đi vì không lùi được nữa, trang cuối thì "D" (tới) tối đi -- còn lại sáng vàng bình thường.
    private void UpdateNavHintColors()
    {
        if (_cornerNavText == null) return;

        bool hasPages   = _pages != null && _pages.Length > 0;
        bool canGoBack  = hasPages && _currentPage > 0;
        bool canGoFwd   = hasPages && _currentPage < _pages.Length - 1;
        // Edit Mode / chưa gán _pages -- mặc định sáng cả 2 để Jok xem layout đầy đủ, không tối om.
        if (!hasPages) { canGoBack = true; canGoFwd = true; }

        const string activeColor = "#c9a25c";
        const string dimColor    = "#5a5248";
        string aColor = canGoBack ? activeColor : dimColor;
        string dColor = canGoFwd  ? activeColor : dimColor;

        // SỬA (Jok yêu cầu -- "chữ lật trang dòng này bị thừa"): nhãn "LẬT TRANG" ở trên đã nói rõ ý nghĩa
        // rồi, dòng dưới chỉ cần "A / D" không lặp lại chữ.
        _cornerNavText.text = $"<size=86%><color=#8a713f>LẬT TRANG</color></size>\n" +
                              $"<color={aColor}>A</color> / <color={dColor}>D</color>";
    }

    public void Open()
    {
        if (_pages == null || _pages.Length == 0) return;
        _currentPage = 0;
        if (_panel != null) _panel.SetActive(true);
        RefreshPage();
    }

    public void Close()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void NextPage()
    {
        if (_currentPage >= _pages.Length - 1) return;
        _currentPage++;
        RefreshPage();
        PlayFlipSfx();

        if (_currentPage == _pages.Length - 1 && !HasFinishedReading)
        {
            HasFinishedReading = true;
            if (_reactionDialogue != null) DialogueUI.Instance?.StartDialogue(_reactionDialogue);
            OnFinishedReading?.Invoke();
        }
    }

    private void PrevPage()
    {
        if (_currentPage <= 0) return;
        _currentPage--;
        RefreshPage();
        PlayFlipSfx();
    }

    private void RefreshPage()
    {
        if (_pageImage != null && _pages != null && _currentPage < _pages.Length)
            _pageImage.sprite = _pages[_currentPage];

        // Close luôn hiện xuyên suốt (không chỉ trang cuối) -- vẫn thoả đúng yêu cầu "trang cuối phải có
        // nút thoát", chỉ đơn giản hơn là hiện sẵn từ đầu, không mất gì.

        if (_pageCounterText != null && _pages != null)
            _pageCounterText.text = $"{_currentPage + 1} / {_pages.Length}";

        // A/D tự đổi màu theo trang hiện tại (Jok yêu cầu -- "y trang W/S của Inventory").
        UpdateNavHintColors();
    }

    private void PlayFlipSfx()
    {
        if (_pageFlipSfx != null) AudioManager.Instance?.PlaySFX(_pageFlipSfx);
    }
}
