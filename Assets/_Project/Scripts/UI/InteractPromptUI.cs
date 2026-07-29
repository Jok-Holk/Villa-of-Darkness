using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 2 lớp tách biệt, KHÔNG bật/tắt chung 1 cụm như bản cũ:
//   - defaultDot: chấm tròn trắng nhỏ, LUÔN LUÔN hiện (crosshair mặc định).
//   - promptRoot: cụm "[E] Tên vật" (key + label), CHỈ hiện khi đang ngắm trúng vật tương tác được.
//
// SỬA 2026-07-28 (Jok phát hiện qua ảnh Game thật -- "cái E hiện tại không khớp design 1 tí nào"): PromptRoot
// trước giờ CHỈ có 2 dòng chữ trần trụi (KeyLabel/NameLabel xếp DỌC, không nền) -- không hề có pill/badge bo
// tròn như bảng thiết kế. Sprite tròn thật hẹn giao cho Phúc chưa từng thấy được gắn vào. Giờ tự dựng nền
// pill + badge tròn bằng CODE (giống VoDUISpriteUtil đã dùng cho TutorialHintUI) -- không phụ thuộc ai gán
// tay sprite ngoài nữa, và đổi bố cục từ xếp dọc sang 1 HÀNG NGANG (badge trái + tên phải) đúng mockup.
// [ExecuteAlways] để Jok xem layout qua VoD_UIPreviewWindow mà không cần bấm Play.
[ExecuteAlways]
public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Tooltip("Chấm tròn trắng mặc định — luôn hiện, không tắt kể cả khi không ngắm trúng gì.")]
    [SerializeField] private GameObject defaultDot;

    [Tooltip("Cụm '[E] Tên vật' — chỉ hiện khi ngắm trúng vật tương tác được.")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private TextMeshProUGUI nameLabel; // để trống nếu vật không có IInteractableLabel
    [SerializeField] private string defaultKeyText = "E";

    private void OnEnable()
    {
        if (Instance != null && Instance != this && Application.isPlaying) { Destroy(gameObject); return; }
        Instance = this;

        // BUG THẬT (Jok phát hiện -- "không đổi gì cả"): trước đây có cờ _visualsBuilt CHỈ gọi BuildVisuals()
        // 1 LẦN DUY NHẤT -- cờ này đã lưu true vào scene từ lần chạy lỗi trước, nên mọi lần sửa code sau đó
        // KHÔNG BAO GIỜ được áp lại dù bản thân BuildVisuals() đã sửa thành idempotent (get-or-create). Giờ
        // gọi lại MỖI LẦN OnEnable -- an toàn vì toàn bộ bên trong đều get-or-create, không tạo trùng gì cả.
        BuildVisuals();

        if (defaultDot != null) defaultDot.SetActive(true);

        if (Application.isPlaying) Hide();
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    // Bọc KeyLabel vào badge tròn vàng + thêm nền pill sau lưng cả cụm, đổi layout dọc -> ngang. KeyLabel/
    // NameLabel vẫn GIỮ NGUYÊN reference (chỉ reparent/resize) -- không phá field đã gán sẵn trong Inspector.
    //
    // SỬA 2026-07-28 (Jok phát hiện): bản đầu có guard "đã có KeyBadge thì return" -- lần sau sửa code
    // (childControlWidth, vị trí pill...) chạy lại KHÔNG áp được gì cả vì KeyBadge cũ đã tồn tại từ trước,
    // phải xoá tay mới thấy fix mới. Giờ luôn GET-OR-CREATE từng phần và áp lại toàn bộ settings, không early-
    // return -- sửa code xong chỉ cần recompile/vào lại Play là tự nâng cấp, không cần dọn tay gì cả.
    private void BuildVisuals()
    {
        if (promptRoot == null || keyLabel == null || nameLabel == null) return;
        var rootT = promptRoot.transform;

        var notoFont = VoDFontUtil.FindNotoSansFont();

        // Nền pill -- gắn THẲNG lên chính PromptRoot (không phải con riêng) để Image tự phủ đúng đúng kích
        // thước cuối cùng do ContentSizeFitter tính ra bên dưới, giống pattern TutorialHintUI đã dùng.
        var bg = promptRoot.GetComponent<Image>();
        if (bg == null) bg = promptRoot.AddComponent<Image>();
        // Alpha 0.85 -> 0.94 (Jok yêu cầu) -- cảnh horror tối rất nhiều, viền pill dễ "tan" vào nền tối phía
        // sau nếu để hở quá nhiều, dù chữ/badge vẫn đọc được. Đục hơn cho khung rõ hình dạng hơn.
        bg.color = new Color(0.04f, 0.035f, 0.03f, 0.94f);
        // Sprite nguồn to hơn (128,128,48 thay vì 64,64,24) khớp tỉ lệ x2 -- giữ nguyên TỈ LỆ bo góc (radius
        // vẫn = 3/8 cạnh), không thì góc bo sẽ trông "phẳng" hơn hẳn khi pill phóng to gấp đôi.
        bg.sprite = VoDUISpriteUtil.CreateRoundedSprite(128, 128, 48);
        bg.type = Image.Type.Sliced;

        var hlayout = promptRoot.GetComponent<HorizontalLayoutGroup>();
        if (hlayout == null) hlayout = promptRoot.AddComponent<HorizontalLayoutGroup>();
        hlayout.childAlignment = TextAnchor.MiddleCenter;
        // SỬA (Jok yêu cầu -- "E và hint sẽ rất nhỏ khi gameplay thật, tăng size gấp 2"): nhân đôi toàn bộ
        // kích thước (font/badge/padding/spacing) -- giữ nguyên tỉ lệ thiết kế, chỉ to hơn cho dễ đọc từ xa.
        hlayout.spacing = 20;
        hlayout.padding = new RectOffset(16, 40, 12, 12);
        hlayout.childForceExpandWidth = false;
        hlayout.childForceExpandHeight = false;
        // BUG THẬT (Jok phát hiện -- "dày quá không dài ra"): thiếu 2 dòng này thì LayoutGroup KHÔNG resize
        // con theo LayoutElement.preferredWidth/Height -- KeyBadge giữ nguyên sizeDelta mặc định (100x100)
        // của 1 RectTransform mới tạo thay vì co về đúng 40x40, ra hình khối vuông to đùng thay vì badge tròn
        // nhỏ gọn. Đúng lỗi y hệt đã ghi chú trong HudMetersUI.cs bản đầu (childControlWidth/Height).
        hlayout.childControlWidth = true;
        hlayout.childControlHeight = true;

        var fitter = promptRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = promptRoot.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // BUG THẬT (Jok phát hiện -- "chung hàng với tâm tròn"): PromptRoot vốn neo ĐÚNG GIỮA MÀN HÌNH
        // (anchoredPosition 0,0) -- y hệt vị trí "Dot" (chấm crosshair luôn hiện, cũng neo giữa màn hình) --
        // nên pill mới đè thẳng lên chấm. Dời pill xuống DƯỚI chấm 1 khoảng, giống đúng bố cục thiết kế
        // (chấm nhỏ ở trên, pill tên vật ở dưới, tách bạch rõ). Chỉnh lại số này thẳng trong Inspector nếu
        // muốn xê dịch thêm.
        var rootRt = promptRoot.GetComponent<RectTransform>();
        // SỬA (Jok yêu cầu -- "lui xuống khỏi tâm 1 ít nữa"): 70 -> 110, xa hẳn khỏi chấm crosshair hơn nữa.
        rootRt.anchoredPosition = new Vector2(0f, -110f);

        // Badge phím tròn vàng -- KeyLabel (chữ "E") reparent vào trong làm con. GET-OR-CREATE: tìm lại đúng
        // "KeyBadge" cũ nếu đã dựng từ trước thay vì tạo chồng thêm 1 cái mới.
        var keyBadgeT = rootT.Find("KeyBadge");
        GameObject keyBadgeGO = keyBadgeT != null ? keyBadgeT.gameObject : new GameObject("KeyBadge", typeof(RectTransform));
        keyBadgeGO.transform.SetParent(rootT, false);
        keyBadgeGO.transform.SetAsFirstSibling();
        var keyBadgeImg = keyBadgeGO.GetComponent<Image>();
        if (keyBadgeImg == null) keyBadgeImg = keyBadgeGO.AddComponent<Image>();
        keyBadgeImg.color = new Color(0.79f, 0.635f, 0.36f, 1f);
        keyBadgeImg.sprite = VoDUISpriteUtil.CreateRoundedSprite(128, 128, 64); // 2x nguồn, radius vẫn = nửa cạnh = tròn khi vuông
        keyBadgeImg.type = Image.Type.Sliced;

        keyLabel.transform.SetParent(keyBadgeGO.transform, false);
        var keyRt = keyLabel.rectTransform;
        keyRt.anchorMin = Vector2.zero; keyRt.anchorMax = Vector2.one;
        keyRt.offsetMin = Vector2.zero; keyRt.offsetMax = Vector2.zero;
        keyLabel.enableAutoSizing = false;
        keyLabel.fontSize = 40;
        keyLabel.fontStyle = FontStyles.Bold;
        keyLabel.alignment = TextAlignmentOptions.Center;
        keyLabel.color = new Color(0.1f, 0.08f, 0.04f, 1f);
        if (notoFont != null) keyLabel.font = notoFont;
        if (string.IsNullOrEmpty(keyLabel.text)) keyLabel.text = defaultKeyText;

        // SỬA (Jok phát hiện -- "height hơi lạ", ra hình bầu dục méo): bản trước lồng thêm 1 lớp
        // HorizontalLayoutGroup+ContentSizeFitter NGAY TRONG KeyBadge để nó tự co giãn -- nhưng PromptRoot
        // (cha) CŨNG có childControlWidth/Height=true, nên 2 hệ layout (cha ép size xuống, con tự tính size
        // theo ContentSizeFitter riêng) TRANH NHAU set cùng 1 RectTransform trong cùng 1 lượt rebuild, ra kích
        // thước sai/méo tuỳ hệ nào chạy sau. Bỏ hẳn lớp lồng -- tính preferredWidth 1 LẦN DUY NHẤT bằng
        // GetPreferredValues() của chính TMP (biết chắc font/size hiện tại), gán thẳng vào LayoutElement --
        // chỉ 1 nguồn set kích thước duy nhất (PromptRoot), không còn xung đột.
        var keyBadgeLE = keyBadgeGO.GetComponent<LayoutElement>();
        if (keyBadgeLE == null) keyBadgeLE = keyBadgeGO.AddComponent<LayoutElement>();
        float keyTextWidth = keyLabel.GetPreferredValues(keyLabel.text).x;
        keyBadgeLE.preferredWidth = Mathf.Max(80f, keyTextWidth + 48f);
        keyBadgeLE.preferredHeight = 80f;

        // NameLabel -- giờ nằm NGANG bên phải badge (trước đây xếp dòng dưới, anchor (0.5,0.6) đo theo bố
        // cục dọc cũ). Reset về anchor 1 điểm chuẩn (0, 0.5) -- HorizontalLayoutGroup tự set anchoredPosition
        // theo đúng quy ước này, giữ nguyên anchor lệch cũ dễ tính sai vị trí hàng ngang.
        nameLabel.transform.SetParent(rootT, false);
        nameLabel.transform.SetSiblingIndex(1);
        var nameRt = nameLabel.rectTransform;
        nameRt.anchorMin = new Vector2(0f, 0.5f);
        nameRt.anchorMax = new Vector2(0f, 0.5f);
        nameRt.pivot     = new Vector2(0f, 0.5f);
        nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
        nameLabel.enableAutoSizing = false;
        nameLabel.fontSize = 40;
        if (notoFont != null) nameLabel.font = notoFont;
        nameLabel.color = new Color(0.92f, 0.89f, 0.84f, 1f);
        // Chữ mẫu placeholder -- trước đây để trống hẳn, preview qua VoD_UIPreviewWindow (không gọi Show())
        // sẽ không thấy chữ nào cả dù pill/badge đã đúng vị trí. Show() lúc Play vẫn ghi đè đúng tên vật thật.
        if (string.IsNullOrEmpty(nameLabel.text)) nameLabel.text = "Tên vật";
        var nameLE = nameLabel.GetComponent<LayoutElement>();
        if (nameLE == null) nameLE = nameLabel.gameObject.AddComponent<LayoutElement>();
        nameLE.preferredHeight = 80;
        nameLE.minWidth = 120;
    }

    /// <summary>label = null hoặc rỗng thì chỉ hiện phím, không hiện tên vật (vật không implement IInteractableLabel).</summary>
    public void Show(string label = null)
    {
        if (promptRoot != null) promptRoot.SetActive(true);
        if (keyLabel != null) keyLabel.text = defaultKeyText;
        if (nameLabel != null) nameLabel.text = string.IsNullOrEmpty(label) ? "" : label;
    }

    public void Hide()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    /// <summary>Ẩn/hiện luôn cả chấm crosshair mặc định -- dùng khi mở UI toàn màn hình (Inventory...) không
    /// cần ngắm bắn gì cả, để lại chấm giữa hình trên UI đó trông thừa/rối mắt.</summary>
    public void SetDotVisible(bool visible)
    {
        if (defaultDot != null) defaultDot.SetActive(visible);
        if (!visible) Hide();
    }
}
