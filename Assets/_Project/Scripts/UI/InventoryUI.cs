using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// XOÁ [ExecuteAlways] + toàn bộ code Preview/dựng UI 2026-07-29/30 (Jok yêu cầu -- "ẩn hết preview, xoá code
// liên quan tới chỉnh UI, chỉ giữ tính năng thôi"): Jok đã canh tay xong toàn bộ layout + xác nhận bug font
// đã fix. OnEnable() giờ CHỈ chạy lúc Play thật (hành vi MonoBehaviour bình thường). Đã xoá hẳn RefreshPreviewOnly()
// + mọi hàm FixColumnLayout/EnsureDetailIcon/EnsureCategoryLabel/EnsureTitleStyle/EnsureDetailTextStyle/
// EnsureActionPills/EnsureFootHints (từng chỉ dùng để DỰNG/CHỈNH layout 1 lần, không phải tính năng gameplay)
// -- các object UI đã tồn tại sẵn trong scene, không cần code dựng lại. Logic gameplay thật (ApplyActionPills,
// UpdateMoveHint, ShowItemDetails, Refresh...) không đụng tới, vẫn hoạt động bình thường.
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Để trống các field dưới thì tự lấy Instance tương ứng lúc chạy (Awake) — cho phép prefab hoá")]
    [SerializeField] private InventorySystem  _inventorySystem;

    [Header("Kéo Player vào để tắt input khi mở inventory")]
    [SerializeField] private PlayerController _playerController;

    [Header("Xem 3D item từ túi đồ")]
    [Tooltip("itemId → ExamineItem proxy. Click item → xem 3D → Chuột Phải → về lại túi đồ.")]
    [SerializeField] private ExamineItemEntry[] _examineRegistry;

    [System.Serializable]
    public struct ExamineItemEntry
    {
        public string     itemId;
        public ExamineItem examineItem;
    }

    [Header("Sử dụng — cầm tay trái")]
    [Tooltip("Kéo HandheldItemController trên Player vào đây.")]
    [SerializeField] private HandheldItemController _handheldController;

    [Header("HUD ẩn khi mở Inventory (chấm crosshair, thanh stamina, gợi ý lắc đèn pin...) -- trước đây mở Inventory không ẩn gì cả, nhìn rối vì HUD gameplay vẫn đè lên")]
    [SerializeField] private GameObject[] _hudToHideDuringInventory;

    [Header("Đọc nhật ký — UI paged-reader RIÊNG, khác hẳn Examine 3D thường (chỉ áp dụng cho đúng itemId này)")]
    [SerializeField] private string _diaryItemId = "nhat_ky_ong_do";
    [SerializeField] private DiaryReaderUI _diaryReader;

    [Header("Panel chi tiết vật phẩm (bên phải)")]
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemDescText;
    [Tooltip("Icon lớn của vật phẩm đang xem -- dùng lại ĐÚNG ItemData.icon đã có sẵn (giống icon nhỏ trong danh sách), không cần ảnh riêng khác. Để trống thì tự tạo lúc Awake().")]
    [SerializeField] private Image   _detailIconImage;
    [Tooltip("Eyebrow phân loại phía trên tên vật phẩm (VD 'VẬT PHẨM CHÍNH') -- để trống thì tự tạo lúc Awake().")]
    [SerializeField] private TMP_Text _categoryText;
    [SerializeField] private string   _defaultDescHint = "Chọn 1 vật phẩm để xem chi tiết";

    [Header("Hint 'E Sử dụng' trên thanh phím tắt dưới cùng — luôn hiện, chỉ mờ đi khi item không dùng được")]
    [SerializeField] private TMP_Text _actionHintText;
    // SỬA (Jok yêu cầu -- "E cất đi V xem chưa hề áp layout luôn"): 2 hint này giờ là 2 PILL THẬT (badge +
    // chữ, giống TutorialHintUI/InteractPromptUI) nằm trong DetailsPanel dưới mô tả -- đúng vị trí mockup,
    // không còn nằm chung 1 dòng text trần trong Hotbar dưới cùng nữa. _actionHintText cũ bị ẩn hẳn.
    private TMP_Text _usePillLabel;
    private TMP_Text _viewPillLabel;
    private Image    _usePillBg;
    private Image    _viewPillBg;

    [Header("Hint 'W/S Di chuyển' — W mờ khi đang ở dòng đầu, S mờ khi đang ở dòng cuối")]
    [SerializeField] private TMP_Text _moveHintText;
    // SỬA (Jok chỉ ra -- "WS Tab tôi đâu có yêu cầu tách khỏi 2 panel"): fetch lại đúng artifact GỐC đã
    // duyệt xác nhận -- cụm W/S/Tab nằm NGAY TRONG DetailsPanel (chỉ 1 viền mỏng phân cách phía trên), KHÔNG
    // phải 1 thanh Hotbar riêng full-width như bản cũ Thuận dựng tay. Badge cũng chỉ viền/nền mờ nhạt, không
    // phải pill vàng đặc. 2 field dưới đây thay thế hẳn _moveHintText/Hotbar.
    private TMP_Text _footWText, _footSText, _footTabText;

    [Header("Cuộn danh sách — để trống thì tự tìm ScrollRect trong con Grid")]
    [SerializeField] private ScrollRect _listScrollRect;

    // List thay vì mảng cố định -- pool slot tự PHÌNH RA (nhân bản dòng đầu tiên làm mẫu) mỗi khi số item
    // thật nhiều hơn số dòng đã dựng sẵn trong scene, không còn giới hạn cứng theo số GameObject có sẵn.
    private List<Image>         _slotIcons       = new List<Image>();
    private List<TMP_Text>      _slotLabels      = new List<TMP_Text>();
    private List<TMP_Text>      _slotStatusLabels = new List<TMP_Text>(); // "Đọc được"/"Đang cầm"/"Xem được" -- dòng nhỏ dưới tên (Jok yêu cầu khớp mockup)
    private List<Image>         _slotStripes     = new List<Image>();      // sọc màu bên trái mỗi dòng — phân loại (vàng=key, xanh=usable), dày/sáng hơn khi được chọn
    private List<RectTransform> _slotStripeRects = new List<RectTransform>();
    private List<Image>         _slotHighlights  = new List<Image>(); // tô nền cả dòng khi được chọn (Jok yêu cầu khớp mockup, không chỉ sọc mỏng)
    private List<Button>        _slotUseButtons  = new List<Button>();
    private List<TMP_Text>      _slotUseButtonLabels = new List<TMP_Text>();
    private List<Color>         _slotBaseColors  = new List<Color>();
    private bool                _slotsCached     = false;
    private Transform           _gridContent;

    private static readonly Color _iconFilledColor  = Color.white;
    private static readonly Color _iconEmptyColor   = new Color(0.267f, 0.267f, 0.267f, 1f);
    private static readonly Color _keyItemColor     = new Color(1f, 0.85f, 0f);
    private static readonly Color _usableItemColor  = new Color(0.33f, 0.53f, 0.8f);
    private static readonly Color _plainItemColor   = new Color(0.15f, 0.15f, 0.15f);
    private static readonly Color _hintDimColor     = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    // SỬA (Jok yêu cầu -- "W S có thể sang màu Vàng nếu lên xuống được"): đổi từ cream nhạt sang ĐÚNG màu
    // vàng accent dùng chung toàn UI (giống màu tô nền dòng chọn/pill "Sử dụng") để rõ ràng hơn khi khả dụng.
    private static readonly Color _hintBrightColor  = new Color(0.79f, 0.635f, 0.36f, 1f);
    private const float _stripeWidthNormal   = 4f;
    private const float _stripeWidthSelected = 9f;

    private int _selectedSlotIndex = -1;

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    private ExamineItem _activeExamine = null;
    public  bool IsExamining => _activeExamine != null && _activeExamine.IsExamining;

    private int _lastClickFrame = -1;

    public UnityEvent OnOpen  = new UnityEvent();
    public UnityEvent OnClose = new UnityEvent();

    // Lazy-resolve qua Instance nếu field chưa gán tay — object tự deactivate
    // trong Awake() nên KHÔNG dùng Start() để fallback được (Start bị hoãn
    // tới lần SetActive(true) đầu tiên), phải resolve tại điểm dùng thực tế.
    private InventorySystem        Inv      => _inventorySystem    != null ? _inventorySystem    : (_inventorySystem    = InventorySystem.Instance);
    private PlayerController       Player   => _playerController   != null ? _playerController   : (_playerController   = PlayerController.Instance);
    private HandheldItemController Handheld => _handheldController != null ? _handheldController : (_handheldController = HandheldItemController.Instance);

    // OnEnable() -- không còn [ExecuteAlways] (xem ghi chú đầu file), chỉ chạy lúc Play thật.
    private void OnEnable()
    {
        Instance = this;

        CacheSlots();

        // SỬA (Jok yêu cầu -- "E cất đi V xem chưa hề áp layout"): hint cũ nằm chung Hotbar dưới cùng, giờ
        // đã có 2 pill riêng trong DetailsPanel thay thế -- ẩn hẳn object cũ, không xoá field (an toàn nếu
        // Jok muốn khôi phục).
        if (_actionHintText != null) _actionHintText.gameObject.SetActive(false);

        // SỬA (Jok phát hiện -- artifact GỐC không có thanh Hotbar riêng full-width, W/S/Tab nằm trong
        // DetailsPanel): ẩn hẳn "Hotbar" cũ -- MoveText/CloseText của nó đã được thay bằng FootHintRow.
        var hotbar = FindDeep(transform, "Hotbar");
        if (hotbar != null) hotbar.gameObject.SetActive(false);

        // Remove trước rồi mới Add -- OnEnable() có thể chạy lại nhiều lần (Jok bật/tắt qua Preview Window),
        // += đơn thuần sẽ ĐĂNG KÝ TRÙNG, khiến 1 lần ma phát hiện gọi OnGhostSpottedPlayer() nhiều lần.
        GhostAI.OnPlayerSpotted -= OnGhostSpottedPlayer;
        GhostAI.OnPlayerSpotted += OnGhostSpottedPlayer;

        // BUG THẬT (Jok phát hiện -- icon hiện ô trắng trơn nổi lên dù chưa chọn item nào): DetailIcon vừa
        // tạo ở EnsureDetailIcon() mặc định activeSelf=true (không sprite = Unity tự vẽ ô trắng đặc). Open()
        // gọi ClearItemDetails() TRƯỚC Refresh() để ẩn icon + đặt chữ mặc định đúng, nhưng OnEnable() ở đây
        // trước giờ CHỈ gọi Refresh() -- bỏ sót bước ClearItemDetails() ban đầu này.
        ClearItemDetails();

        // Xem NGAY cấu trúc/style (icon/tên/hint) trong Editor không cần Play (Jok yêu cầu) -- nếu Inventory
        // chưa từng có item thật (Edit Mode/chưa Play lần nào trong phiên) thì chỉ thấy đúng style rỗng, đó
        // không phải bug, chỉ là chưa có DATA thật để hiện (dữ liệu túi đồ là trạng thái runtime).
        Refresh();
    }

    private void OnDisable()
    {
        GhostAI.OnPlayerSpotted -= OnGhostSpottedPlayer;
    }

    // XOÁ 2026-07-30 (Jok yêu cầu -- "xoá code liên quan tới chỉnh UI, chỉ giữ tính năng thôi"): toàn bộ
    // FixColumnLayout/EnsureDetailIcon/EnsureCategoryLabel/EnsureTitleStyle/EnsureDetailTextStyle/
    // EnsureActionPills+BuildActionPill/EnsureFootHints+BuildFootGroupLayout+BuildFootTag+BuildFootPlainLabel
    // từng ở đây -- các hàm BUILD/STYLE layout 1 lần (anchor, size, font, sprite) cho DetailsPanel/ActionRow/
    // FootHintRow, không còn được OnEnable() gọi từ khi bỏ [ExecuteAlways] (Jok đã canh tay toàn bộ layout
    // xong). Field/logic THẬT SỰ chạy lúc gameplay (ApplyActionPills(), UpdateMoveHint(), ShowItemDetails()...)
    // vẫn giữ nguyên bên dưới -- các object UI (ActionRow, FootHintRow, DetailIcon, CategoryText...) đã tồn
    // tại sẵn trong scene, không cần code dựng lại nữa.

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        GhostAI.OnPlayerSpotted -= OnGhostSpottedPlayer;
    }

    // Ma phát hiện player khi Tab đang mở — đóng ngay lập tức, không cho lấp ló né jumpscare.
    private void OnGhostSpottedPlayer()
    {
        if (_isOpen) Close();
    }

    private void CacheSlots()
    {
        // BUG THẬT: transform.Find("Grid") chỉ tìm CON TRỰC TIẾP của InventoryUI -- danh sách slot thật
        // nằm sâu hơn trong cấu trúc ScrollRect chuẩn (ScrollView > Viewport > Content), không phải con
        // trực tiếp tên "Grid". Kết quả: grid luôn null, CacheSlots() return sớm ngay từ đầu, KHÔNG BAO GIỜ
        // cache được gì cả -- toàn bộ Inventory (tên/icon/click/highlight) im lặng không hoạt động.
        // Giờ dùng thẳng ScrollRect.content (tham chiếu type-safe, không phụ thuộc đặt tên) làm nguồn chính,
        // chỉ fallback về tìm tên "Grid" (sâu, không chỉ con trực tiếp) nếu vì lý do gì đó không có ScrollRect.
        if (_listScrollRect == null)
            _listScrollRect = GetComponentInChildren<ScrollRect>(includeInactive: true);

        Transform grid = _listScrollRect != null ? _listScrollRect.content : FindDeep(transform, "Grid");
        if (grid == null)
        {
            Debug.LogError("[InventoryUI] Không tìm thấy danh sách slot (ScrollRect.content lẫn object tên 'Grid' đều không có) -- Inventory sẽ trống hoàn toàn.");
            return;
        }

        _gridContent  = grid;
        _slotsCached  = true;
        RebuildSlotCacheFromGrid();
    }

    // Đọc lại TOÀN BỘ slot đang có dưới _gridContent (kể cả những slot vừa Instantiate thêm bởi
    // EnsureSlotCount) -- gọi lại mỗi khi số dòng thay đổi, không chỉ 1 lần lúc Awake như trước.
    private void RebuildSlotCacheFromGrid()
    {
        Transform grid = _gridContent;
        int count = grid.childCount;

        _slotIcons.Clear();
        _slotLabels.Clear();
        _slotStatusLabels.Clear();
        _slotStripes.Clear();
        _slotStripeRects.Clear();
        _slotHighlights.Clear();
        _slotUseButtons.Clear();
        _slotUseButtonLabels.Clear();
        _slotBaseColors.Clear();

        for (int i = 0; i < count; i++)
        {
            Transform slot = grid.GetChild(i);
            var labelT = FixLabelFontBug(slot.Find("Label")?.GetComponent<TMP_Text>());
            _slotIcons.Add(slot.Find("Icon")?.GetComponent<Image>());
            _slotLabels.Add(labelT);
            _slotStatusLabels.Add(GetOrCreateStatusLabel(slot));
            _slotHighlights.Add(EnsureRowHighlight(slot));
            Image stripe = slot.Find("Stripe")?.GetComponent<Image>();
            _slotStripes.Add(stripe);
            var stripeRect = stripe != null ? stripe.GetComponent<RectTransform>() : null;
            ExtendStripeHeight(stripeRect);
            _slotStripeRects.Add(stripeRect);
            _slotBaseColors.Add(_plainItemColor);

            int captured = i;

            // Slot vừa Instantiate() clone nguyên cả listener của slot mẫu (nếu mẫu đã có listener) --
            // gỡ hết listener cũ trước khi gán lại theo ĐÚNG index mới, tránh 2 slot cùng trỏ 1 index cũ.
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSlotClicked(captured));
            }

            Transform useBtnTransform = slot.Find("UseButton");
            Button useBtn = useBtnTransform != null ? useBtnTransform.GetComponent<Button>() : null;
            _slotUseButtons.Add(useBtn);
            _slotUseButtonLabels.Add(useBtnTransform != null ? useBtnTransform.GetComponentInChildren<TMP_Text>(true) : null);
            if (useBtn != null)
            {
                useBtn.onClick.RemoveAllListeners();
                useBtn.onClick.AddListener(() => OnUseButtonClicked(captured));
            }
        }
    }

    // THÊM (Jok yêu cầu -- khớp mockup đã duyệt): mỗi dòng item cần 2 tầng chữ (tên đậm ở trên, trạng thái
    // mờ hơn ở dưới -- "Đọc được"/"Đang cầm"/"Xem được"...), trước đây CHỈ có 1 dòng tên duy nhất. Dồn Label
    // gốc lên NỬA TRÊN của dòng, tạo thêm "Status" ở NỬA DƯỚI -- get-or-create, an toàn gọi lại nhiều lần.
    // Nền tô cả dòng khi được chọn (Jok yêu cầu -- "cả select lên layout chưa giống hoàn toàn", mockup tô
    // nền tan/gold cả hàng chứ không chỉ sọc mỏng bên trái). Chèn làm CON ĐẦU TIÊN để render phía sau
    // Icon/Label/Stripe, mặc định trong suốt (alpha 0), chỉ hiện khi UpdateSelectionVisuals() bật lên.
    private static Image EnsureRowHighlight(Transform slot)
    {
        var t = slot.Find("RowHighlight");
        GameObject go = t != null ? t.gameObject : new GameObject("RowHighlight", typeof(RectTransform));
        go.transform.SetParent(slot, false);
        go.transform.SetAsFirstSibling();

        var img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.raycastTarget = false;
        img.color = new Color(0f, 0f, 0f, 0f);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        // SỬA (Jok yêu cầu -- "thêm height phía DƯỚI"): tràn xuống ĐÁY, không chia đôi trên/dưới.
        // 28 -> 14 -> 20 (Jok chỉnh dần) -- bản gốc (trước khi có sửa này) là 0, không tràn gì cả.
        rt.offsetMin = new Vector2(0f, -20f);
        rt.offsetMax = new Vector2(0f, 0f);

        return img;
    }

    // Stripe là object dựng tay trong scene template (không phải EnsureXxx tạo ra) -- chỉ chỉnh Y để tràn
    // đều theo đúng lý do/khoảng như RowHighlight ở trên, không đụng X/màu (đã quản lý riêng ở
    // UpdateSelectionVisuals()).
    private static void ExtendStripeHeight(RectTransform stripeRt)
    {
        if (stripeRt == null) return;
        stripeRt.offsetMin = new Vector2(stripeRt.offsetMin.x, -20f);
        stripeRt.offsetMax = new Vector2(stripeRt.offsetMax.x, 0f);
    }

    // SỬA 2026-07-29 (Jok yêu cầu -- "chỉ can thiệp phần font đó thôi, giữ size các thứ lại"): bỏ HẲN toàn
    // bộ code chỉnh anchor/size của Label/Status/Icon (từng ở đây qua nhiều vòng -- Jok đã tự canh tay xong)
    // -- Status giờ chỉ get-or-create (KHÔNG set lại vị trí/font size/màu), giữ nguyên y hệt scene hiện có.
    private static TMP_Text GetOrCreateStatusLabel(Transform slot)
    {
        var statusT = slot.Find("Status");
        GameObject statusGO = statusT != null ? statusT.gameObject : new GameObject("Status", typeof(RectTransform));
        statusGO.transform.SetParent(slot, false);

        var status = statusGO.GetComponent<TMP_Text>();
        if (status == null) status = statusGO.AddComponent<TextMeshProUGUI>();

        // SỬA (Jok yêu cầu -- "dời status y = -60 là ổn"): tên item giờ có thể xuống 2 dòng (bỏ Ellipsis) --
        // Status cần dời xuống thêm để không đè lên dòng 2 của tên. Chỉ đụng đúng Y, giữ nguyên X (stretch,
        // không quản ở đây).
        var statusRt = status.rectTransform;
        statusRt.anchoredPosition = new Vector2(statusRt.anchoredPosition.x, -58f);

        return status;
    }

    // BUG THẬT (Jok xác nhận -- "mấy chỗ kia hiện chữ OK, chắc riêng object này setting bị sai... lúc nãy
    // sửa sai làm mất chữ luôn"): 2 lần thử trước (xoá sub-mesh con, rồi ép fontSharedMaterial) ĐỀU không ăn
    // -- nghĩa là hỏng không nằm ở sub-mesh/material bên ngoài mà nằm SÂU trong chính component
    // TextMeshProUGUI này (đã bị gán lại .font qua HÀNG CHỤC lần recompile/toggle suốt session với
    // [ExecuteAlways] bật, khác hẳn pill/tag/category -- những object MỚI TẠO chỉ set font ĐÚNG 1 LẦN).
    // Fix: xoá HẲN component cũ + gắn component MỚI TINH (100% sạch, không mang theo trạng thái mesh/sub-mesh
    // hỏng) -- Jok xác nhận ĐÃ RA CHỮ. Giờ set lại font Noto ĐÚNG 1 LẦN trên component MỚI này (không phải
    // component cũ đã bị đổi qua đổi lại hàng chục lần) -- an toàn vì mỗi lần OnEnable là 1 component hoàn
    // toàn mới, không tích luỹ trạng thái qua nhiều lần đổi font như trước.
    // XOÁ maxVisibleLines/Ellipsis (Jok phát hiện -- "mấy chữ ngắn cũng bị ẩn"): tên NGẮN như "Sổ ghi nợ"
    // (thừa chỗ 1 dòng) vẫn bị cắt trụi thành "..." -- nghi do font mặc định LiberationSans SDF (atlas tĩnh)
    // không có sẵn hết dấu tiếng Việt (ố/ợ/ô/Đ...), TMP đo sai độ rộng dòng cho các ký tự thiếu glyph rồi cắt
    // sớm bất thường. Cơ chế này không đáng tin ở font hiện tại -- bỏ hẳn, chấp nhận tên dài xuống 2 dòng còn
    // hơn mất chữ vô cớ.
    private static TMP_Text FixLabelFontBug(TMP_Text label)
    {
        if (label == null) return null;

        GameObject go = label.gameObject;
        string savedText          = label.text;
        float savedFontSize       = label.fontSize;
        Color savedColor          = label.color;
        TextAlignmentOptions savedAlignment = label.alignment;
        bool savedAutoSizing      = label.enableAutoSizing;

        if (Application.isPlaying) Object.Destroy(label);
        else Object.DestroyImmediate(label);

        for (int i = go.transform.childCount - 1; i >= 0; i--)
        {
            var child = go.transform.GetChild(i);
            if (!child.name.StartsWith("TMP SubMeshUI")) continue;

            if (Application.isPlaying) Object.Destroy(child.gameObject);
            else Object.DestroyImmediate(child.gameObject);
        }

        var fresh = go.AddComponent<TextMeshProUGUI>();
        fresh.text             = savedText;
        fresh.fontSize         = savedFontSize;
        fresh.color            = savedColor;
        fresh.alignment        = savedAlignment;
        fresh.enableAutoSizing = savedAutoSizing;
        // SỬA (Jok yêu cầu -- "nếu là font mặc định thì bold nó lên"): tên item là chữ chính của dòng, cần
        // nổi bật hơn Status bên dưới.
        fresh.fontStyle        = FontStyles.Bold;

        // XÁC NHẬN LẦN 2 (Jok test -- "đổi font này là mất chữ?"): cứ gán label.font = NotoSans qua SCRIPT là
        // chữ biến mất, kể cả trên component MỚI TINH vừa tạo -- đúng bug đã ghi nhận trong memory 17 ngày
        // trước (crash/im lặng ngay tại TMP_Text.set_font khi gán qua code, không liên quan gì tới trạng thái
        // cũ tích luỹ). KHÔNG gán font qua code cho Label nữa -- để mặc định TMP (LiberationSans SDF,
        // Static-mode, đã chứng minh ổn định). Nếu Jok muốn Noto cho riêng object này, gán TAY qua Inspector
        // (không phải lúc nào script chạy cũng bị, nhưng script thì luôn bị).

        return fresh;
    }

    // Pool slot tự phình ra -- nhân bản dòng ĐẦU TIÊN (đã dựng sẵn tay, đủ Icon/Label/Stripe/UseButton)
    // làm mẫu, thêm đúng số dòng còn thiếu, rồi cache lại toàn bộ. KHÔNG bao giờ thu nhỏ lại (item bỏ ra
    // khỏi túi đồ chỉ để trống dòng, không xoá GameObject -- tránh phải Instantiate lại liên tục).
    private void EnsureSlotCount(int needed)
    {
        if (_gridContent == null || needed <= _slotIcons.Count) return;
        if (_gridContent.childCount == 0)
        {
            Debug.LogError("[InventoryUI] Không có dòng slot mẫu nào trong Content để nhân bản thêm -- cần ít nhất 1 dòng dựng sẵn trong scene.");
            return;
        }

        Transform template = _gridContent.GetChild(0);
        int toAdd = needed - _gridContent.childCount;
        for (int i = 0; i < toAdd; i++)
        {
            Transform clone = Object.Instantiate(template, _gridContent);
            clone.name = template.name;
        }

        RebuildSlotCacheFromGrid();
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else         Open();
    }

    public void Open()
    {
        // Chưa thoát khỏi sự bám đuổi của ma thì không cho mở túi đồ.
        if (GhostAI.AnyGhostChasing)
        {
            Debug.LogWarning("[Inventory][debug] Open() bị chặn vì GhostAI.AnyGhostChasing = true -- nếu " +
                              "không có ma nào thực sự đang đuổi, đây là nguyên nhân Tab cần bấm nhiều lần.");
            return;
        }

        if (!_slotsCached) CacheSlots();

        _isOpen = true;
        gameObject.SetActive(true);

        // CỐ Ý giữ khoá + ẩn con trỏ -- inventory này điều khiển thuần bàn phím (Tab mở/đóng, W/S chọn
        // dòng, F dùng), không cần chuột. Để cursor hiện ra + unlock trước đây khiến chuột lỡ tay di qua
        // Button cũng tự đổi màu Normal của chính nó (Selectable), ĐÈ LÊN màu highlight tôi set tay cho
        // Stripe theo bàn phím -- nhìn như highlight "không hiện" dù _selectedSlotIndex vẫn đúng.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (Player != null)
            Player.SetInputEnabled(false);

        // Chặn InteractionSystem (E tương tác thế giới, VD HideSpot) bắn trùng
        // trong lúc Tab đang mở — trước đây thiếu dòng này nên đứng trước chỗ nấp
        // + mở túi đồ cùng lúc có thể kích hoạt cả 2.
        InteractionSystem.IsInputBlocked = true;

        // REVERT 2026-07-26: Thử thêm Time.timeScale=0 (pause) nhưng gây regression thật (Tab phải bấm 3
        // lần, HUD ẩn mà Inventory không hiện nội dung) -- nghi có hệ thống khác phản ứng với timeScale=0
        // theo cách không tương thích. Bỏ lại như cũ (Tab không pause) cho tới khi có thời gian điều tra kỹ
        // hơn, ưu tiên sửa Inventory hiển thị được nội dung trước đã.
        // Time.timeScale = 0f;

        InteractPromptUI.Instance?.SetDotVisible(false);
        SetHudVisible(false);

        ClearItemDetails();

        OnOpen.Invoke();
        Refresh();
    }

    private void SetHudVisible(bool visible)
    {
        if (_hudToHideDuringInventory != null)
            foreach (var go in _hudToHideDuringInventory)
                if (go != null) go.SetActive(visible);

        HudMetersUI.Instance.SetVisible(visible);
    }

    public void Close()
    {
        if (_activeExamine != null && _activeExamine.IsExamining)
        {
            _activeExamine.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            _activeExamine.StopExamine();
            _activeExamine = null;
        }

        _isOpen = false;
        gameObject.SetActive(false);

        if (Player != null)
            Player.SetInputEnabled(true);

        InteractionSystem.IsInputBlocked = false;

        // Time.timeScale = 1f; -- xem ghi chú REVERT trong Open()

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        InteractPromptUI.Instance?.SetDotVisible(true);
        SetHudVisible(true);

        OnClose.Invoke();
    }

    private void Update()
    {
        // Input thật (W/S/E/V) chỉ có ý nghĩa lúc Play -- Edit Mode chỉ để xem style tĩnh qua OnEnable().
        if (!Application.isPlaying) return;
        if (!_isOpen) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            MoveSelection(-1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            MoveSelection(1);

        // Đổi từ F sang E -- F trùng phím tắt FlashlightController.Toggle(), dù đã chặn bằng
        // InteractionSystem.IsInputBlocked ở phía đèn pin, đổi hẳn phím ở đây cho chắc chắn không đụng nữa.
        if (Input.GetKeyDown(KeyCode.E))
            UseSelected();

        // THÊM: V Xem kỹ -- trước đây MỌI item (kể cả giấy/sổ ghi nợ cần đọc) chỉ xem được bằng cách click
        // chuột đúng dòng, không có phím tắt nào cả trong lúc đang điều khiển thuần bàn phím (W/S chọn dòng).
        if (Input.GetKeyDown(KeyCode.V))
            ViewSelected();
    }

    private void ViewSelected()
    {
        if (_selectedSlotIndex < 0) return;
        List<string> items = Inv != null ? Inv.GetAllItems() : new List<string>();
        if (_selectedSlotIndex >= items.Count) return;

        OnItemClicked(items[_selectedSlotIndex]);
    }

    public void Refresh()
    {
        if (!_slotsCached) CacheSlots();
        if (_slotIcons.Count == 0) return;

        List<string> items = Inv != null
            ? Inv.GetAllItems()
            : new List<string>();

        // Túi đồ nhiều item hơn số dòng đã dựng sẵn -- tự nhân bản thêm dòng trước khi vẽ, không còn
        // giới hạn cứng theo số GameObject có sẵn trong scene nữa.
        EnsureSlotCount(items.Count);

        for (int i = 0; i < _slotIcons.Count; i++)
        {
            bool hasItem = i < items.Count;

            if (hasItem)
            {
                string   itemId = items[i];
                ItemData data   = Inv?.GetItemData(itemId);

                // LƯU Ý: KHÔNG dùng icon Unicode/emoji trang trí (VD ✋) -- LiberationSans SDF không có glyph
                // đó, TMP sẽ hiện ô vuông rỗng thay thế (bug y hệt vụ số thứ tự ①②③ trước đây). Chỉ dùng chữ
                // thường có dấu tiếng Việt bình thường, đã xác nhận render đúng ở mọi chỗ khác trong game.
                bool isEquippedRow = Handheld != null && Handheld.IsHoldingSomething && Handheld.CurrentItemId == itemId;
                string displayName = data != null && !string.IsNullOrEmpty(data.itemName) ? data.itemName : itemId;

                // SỬA (Jok yêu cầu -- khớp mockup): trước đây nhét "(đang cầm)" thẳng vào tên, giờ tách
                // riêng ra dòng "Status" nhỏ bên dưới ("Đang cầm"/"Đọc được"/"Xem được"/"Cầm được").
                if (_slotLabels[i] != null)
                    _slotLabels[i].text = displayName;

                if (_slotStatusLabels[i] != null)
                    _slotStatusLabels[i].text = GetItemStatusLabel(itemId, data, isEquippedRow);

                if (_slotIcons[i] != null)
                {
                    if (data != null && data.icon != null)
                    {
                        _slotIcons[i].sprite = data.icon;
                        _slotIcons[i].color  = Color.white;
                    }
                    else
                    {
                        _slotIcons[i].sprite = null;
                        _slotIcons[i].color  = _iconFilledColor;
                    }
                }

                // Sọc phân loại: vàng = vật phẩm chính (key item), xanh = dùng được, tối = thường.
                _slotBaseColors[i] = data != null && data.isKeyItem ? _keyItemColor
                                   : data != null && data.isUsable  ? _usableItemColor
                                   : _plainItemColor;

                // POLISH 2026-07-28: Bỏ hẳn nút Dùng/Cất RIÊNG trên từng dòng danh sách -- trùng lặp hoàn
                // toàn với nút to đẹp bên panel chi tiết (chọn dòng là thấy ngay), mà pill mới lại to hơn cả
                // chiều cao dòng nên bị tràn ra ngoài, nhìn như 1 vệt đỏ vỡ hình. Panel chi tiết là nơi DUY
                // NHẤT còn nút này, đúng bố cục mockup đã duyệt.
                if (_slotUseButtons[i] != null) _slotUseButtons[i].gameObject.SetActive(false);
            }
            else
            {
                if (_slotLabels[i]  != null) _slotLabels[i].text   = string.Empty;
                if (_slotStatusLabels[i] != null) _slotStatusLabels[i].text = string.Empty;
                if (_slotIcons[i]   != null)
                {
                    _slotIcons[i].sprite = null;
                    _slotIcons[i].color  = _iconEmptyColor;
                }
                _slotBaseColors[i] = _plainItemColor;

                if (_slotUseButtons[i] != null)
                    _slotUseButtons[i].gameObject.SetActive(false);
            }
        }

        if (_selectedSlotIndex < 0 || _selectedSlotIndex >= items.Count)
            _selectedSlotIndex = items.Count > 0 ? 0 : -1;

        SelectSlot(_selectedSlotIndex);
        UpdateScrollbarSize();
    }

    // BUG THẬT (Jok phát hiện -- "chưa ổn về cái scrollbar"): Unity Scrollbar CHỈ tự tính lại kích thước
    // Handle lúc ScrollRect thật sự chạy runtime (không phải ExecuteAlways) -- Edit Mode/preview để nguyên
    // Handle ở trạng thái mặc định gần lấp đầy CẢ track (rect.height ≈ track height, đo scan xác nhận
    // 864≈804), không phản ánh đúng tỉ lệ nội dung/khung nhìn thật. Tự tính bằng code, set thẳng
    // Scrollbar.size -- chạy được cả Edit Mode lẫn Play, không phụ thuộc ScrollRect tick.
    private void UpdateScrollbarSize()
    {
        if (_listScrollRect == null) return;
        var scrollbar = _listScrollRect.verticalScrollbar;
        var viewport  = _listScrollRect.viewport;
        var content   = _listScrollRect.content;
        if (scrollbar == null || viewport == null || content == null) return;
        if (content.rect.height <= 0f) return;

        scrollbar.size = Mathf.Clamp01(viewport.rect.height / content.rect.height);

        // SỬA (Jok phát hiện: Handle "đâm tọt xuống dưới" ngoài track) -- Handle đang anchor full-stretch
        // (anchorMin=0,0 anchorMax=1,1) nên đáng lẽ sizeDelta phải =0 (Scrollbar tự lo % lấp đầy qua anchor).
        // Scan thực tế cho thấy sizeDelta.y=60 CÒN SÓT LẠI từ lúc dựng tay Handle (số cộng dồn THÊM vào
        // chiều cao full-stretch, không phải absolute height) -- +60px này đẩy đáy Handle tràn xuống dưới
        // track. Ép về 0 mỗi lần refresh để không phụ thuộc giá trị cũ trong Inspector.
        var handleRect = scrollbar.handleRect;
        if (handleRect != null) handleRect.sizeDelta = new Vector2(handleRect.sizeDelta.x, 0f);
    }

    // Dùng chung cho ShowItemDetails/ClearItemDetails -- 2 pill "E ..." + "V Xem kỹ" tô
    // sáng/mờ theo item hiện có dùng được/xem được không, giống hệt logic cũ của _actionHintText.
    private void ApplyActionPills(string useLabelText, bool useEnabled, bool viewEnabled)
    {
        Color activeGold = new Color(0.79f, 0.635f, 0.36f, 1f);
        Color activeDark = new Color(0.15f, 0.14f, 0.13f, 0.92f);
        Color dimBg      = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        Color activeText = new Color(0.1f, 0.08f, 0.04f, 1f);
        Color dimText    = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        if (_usePillLabel != null) { _usePillLabel.text = useLabelText; _usePillLabel.color = useEnabled ? activeText : dimText; }
        if (_usePillBg != null) _usePillBg.color = useEnabled ? activeGold : dimBg;

        if (_viewPillLabel != null) { _viewPillLabel.text = "Xem kỹ"; _viewPillLabel.color = viewEnabled ? Color.white : dimText; }
        if (_viewPillBg != null) _viewPillBg.color = viewEnabled ? activeDark : dimBg;

        // BUG THẬT (Jok phát hiện "làm hời hợt" -- cùng lỗi minWidth=60 cứng như FootHintRow): label build
        // lúc CHƯA có chữ thật ("Sử dụng"/"Cất đi" chỉ set ở ĐÂY, sau khi BuildActionPill() đã chốt minWidth)
        // -- "Cất đi" (E khi đang cầm) hay chữ dài hơn dễ bị cắt vì minWidth tính mù. Tính lại đúng mỗi lần
        // đổi chữ, giống UpdateKeyBadgeWidth() đã dùng ở TutorialHintUI.
        UpdatePillMinWidth(_usePillLabel);
        UpdatePillMinWidth(_viewPillLabel);
    }

    private static void UpdatePillMinWidth(TMP_Text label)
    {
        if (label == null) return;
        var le = label.GetComponent<LayoutElement>();
        if (le == null) return;
        float textWidth = label.GetPreferredValues(label.text).x;
        le.minWidth = Mathf.Max(90f, textWidth + 6f);
    }

    // Dòng trạng thái nhỏ dưới tên mỗi item (Jok yêu cầu khớp mockup: "Đọc được"/"Đang cầm"/"Xem được"/
    // "Cầm được"). Không có field "loại vật phẩm" riêng trong ItemData -- suy ra từ cờ đã có sẵn.
    private string GetItemStatusLabel(string itemId, ItemData data, bool isEquippedRow)
    {
        if (isEquippedRow) return "Đang cầm";
        if (itemId == _diaryItemId) return "Đọc được";
        if (data != null && data.isExaminable) return "Xem được";
        if (data != null && data.isUsable) return "Cầm được";
        return "";
    }

    public void OnItemClicked(string itemId)
    {
        ItemData data = Inv?.GetItemData(itemId);

        ShowItemDetails(data, itemId);

        if (data != null && data.monologueClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(data.monologueClip);

        // Nhật ký đọc kiểu slideshow PNG nhiều trang -- KHÔNG dùng Examine 3D xoay như mọi item khác, nên
        // CỐ Ý không đi qua ExamineStageUI.SetReadableText() -- không cần lớp phủ chữ chồng thêm lên UI lật
        // trang đã có sẵn.
        if (itemId == _diaryItemId && _diaryReader != null)
        {
            _diaryReader.Open();
            return;
        }

        // THÊM: Chữ viết tay trên model 3D (VD sổ ghi nợ) rất khó đọc dù xoay/zoom hết cỡ -- truyền sẵn
        // mô tả (đã có sẵn nội dung đầy đủ trong ItemData, không cần viết thêm) để Examine có thể bật lớp
        // phủ đọc bằng phím [R]. Item nào description rỗng thì đơn giản là hint [R] sẽ không hiện ra.
        ExamineStageUI.GetOrCreate().SetReadableText(data?.description);
        ExamineStageUI.GetOrCreate().SetItemTitle(data != null && !string.IsNullOrEmpty(data.itemName) ? data.itemName : itemId);

        TryOpenExamine(itemId);
    }

    private void ShowItemDetails(ItemData data, string itemId)
    {
        if (data == null)
        {
            ClearItemDetails();
            return;
        }

        if (_itemNameText != null)
            _itemNameText.text = !string.IsNullOrEmpty(data.itemName) ? data.itemName : itemId;

        bool isEquipped = Handheld != null && Handheld.IsHoldingSomething && Handheld.CurrentItemId == itemId;

        // Eyebrow phân loại -- tái dùng ĐÚNG logic GetItemStatusLabel() đã viết cho danh sách bên trái,
        // không viết lại 1 công thức khác cho cùng 1 khái niệm.
        if (_categoryText != null)
            _categoryText.text = GetItemStatusLabel(itemId, data, isEquipped).ToUpperInvariant();

        // Icon lớn -- dùng lại ĐÚNG ItemData.icon (giống icon nhỏ trong danh sách), không có thì ẩn hẳn
        // khung icon đi thay vì hiện 1 ô trống/vỡ hình.
        if (_detailIconImage != null)
        {
            bool hasIcon = data.icon != null;
            _detailIconImage.gameObject.SetActive(hasIcon);
            if (hasIcon) _detailIconImage.sprite = data.icon;
        }

        if (_itemDescText != null)
            _itemDescText.text = isEquipped ? $"{data.description}\n\n<color=#{ColorUtility.ToHtmlStringRGBA(_hintBrightColor)}>(Đang cầm trên tay)</color>" : data.description;

        // 2 pill hành động trong DetailsPanel -- trước đây bấm E xong chữ không đổi gì cả, không biết đang
        // cầm hay chưa. Giờ đổi hẳn "Sử dụng" -> "Cất đi" khi ĐÚNG item này đang cầm trên tay, sáng/mờ theo
        // đúng item có dùng được/xem được không.
        ApplyActionPills(isEquipped ? "Cất đi" : "Sử dụng", data.isUsable || isEquipped, data.isExaminable);
    }

    private void ClearItemDetails()
    {
        if (_itemNameText != null) _itemNameText.text = string.Empty;
        if (_categoryText != null) _categoryText.text = string.Empty;
        if (_itemDescText != null) _itemDescText.text = _defaultDescHint;
        if (_detailIconImage != null) _detailIconImage.gameObject.SetActive(false);
        ApplyActionPills("Sử dụng", false, false);
    }

    // Di chuyển con trỏ chọn dòng bằng W/S — chỉ đổi dòng highlight + xem trước
    // chi tiết, KHÔNG tự mở Examine (Examine chỉ mở khi bấm chuột/Enter, xem OnSlotClicked).
    private void MoveSelection(int delta)
    {
        List<string> items = Inv != null ? Inv.GetAllItems() : new List<string>();
        if (items.Count == 0) return;

        int newIndex = Mathf.Clamp(_selectedSlotIndex + delta, 0, items.Count - 1);
        if (newIndex == _selectedSlotIndex) return;

        SelectSlot(newIndex);
        ShowItemDetails(Inv?.GetItemData(items[newIndex]), items[newIndex]);
    }

    private void SelectSlot(int index)
    {
        _selectedSlotIndex = index;
        UpdateSelectionVisuals();
        ScrollToSelected();
        UpdateMoveHint();
    }

    // Mờ tag "W" khi đang ở dòng đầu (không lên được nữa), mờ tag "S" khi đang ở dòng cuối -- giờ 2 tag
    // RIÊNG (FootHintRow) thay vì rich-text chung 1 dòng như bản Hotbar cũ.
    private void UpdateMoveHint()
    {
        if (_footWText == null || _footSText == null) return;

        int count = Inv != null ? Inv.GetAllItems().Count : 0;
        bool canMoveUp   = _selectedSlotIndex > 0;
        bool canMoveDown = _selectedSlotIndex < count - 1;

        _footWText.color = canMoveUp   ? _hintBrightColor : _hintDimColor;
        _footSText.color = canMoveDown ? _hintBrightColor : _hintDimColor;
    }

    // Không tô nền cả dòng khi chọn — chỉ sọc bên trái dày/sáng hơn + tên chữ sáng hơn (xem Refresh/Label).
    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < _slotStripes.Count; i++)
        {
            bool isSelected = i == _selectedSlotIndex;

            if (_slotStripes[i] != null)
                _slotStripes[i].color = isSelected ? Color.Lerp(_slotBaseColors[i], Color.white, 0.4f) : _slotBaseColors[i];

            if (_slotStripeRects[i] != null)
                _slotStripeRects[i].sizeDelta = new Vector2(isSelected ? _stripeWidthSelected : _stripeWidthNormal, _slotStripeRects[i].sizeDelta.y);

            if (_slotLabels[i] != null)
                _slotLabels[i].color = isSelected ? _hintBrightColor : new Color(0.65f, 0.65f, 0.65f);

            // Tô nền cả dòng khi chọn (Jok yêu cầu khớp mockup) -- màu ấm nhạt, không cần biết trước
            // _slotBaseColors[i] là gì (dùng chung 1 tông tan/gold cho MỌI loại item, không phân biệt màu
            // sọc phân loại, tránh nền tô đè lẫn lộn màu với sọc bên trái).
            if (_slotHighlights[i] != null)
                _slotHighlights[i].color = isSelected ? new Color(0.79f, 0.635f, 0.36f, 0.14f) : new Color(0f, 0f, 0f, 0f);
        }
    }

    private void ScrollToSelected()
    {
        if (_listScrollRect == null || _slotStripes.Count == 0) return;
        if (_selectedSlotIndex < 0) return;

        float t = _slotStripes.Count <= 1 ? 0f : 1f - (float)_selectedSlotIndex / (_slotStripes.Count - 1);
        _listScrollRect.verticalNormalizedPosition = Mathf.Clamp01(t);
    }

    private void UseSelected()
    {
        if (_selectedSlotIndex < 0) return;
        OnUseButtonClicked(_selectedSlotIndex);
    }

    private bool TryOpenExamine(string itemId)
    {
        if (_examineRegistry == null) return false;

        foreach (var entry in _examineRegistry)
        {
            if (entry.itemId != itemId || entry.examineItem == null) continue;

            // ĐÃ SỬA: Tắt item cũ nếu đang xem dở để soi item mới
            if (_activeExamine != null && _activeExamine.IsExamining)
            {
                _activeExamine.OnExamineEnd.RemoveListener(ReopenAfterExamine);
                _activeExamine.StopExamine();
            }

            _activeExamine = entry.examineItem;

            entry.examineItem.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            entry.examineItem.OnExamineEnd.AddListener(ReopenAfterExamine);

            // ĐÃ SỬA: Xóa dòng gameObject.SetActive(false) để UI giữ nguyên
            entry.examineItem.StartExamineFromInventory();
            return true;
        }
        return false;
    }

    private void ReopenAfterExamine()
    {
        if (_activeExamine != null)
        {
            _activeExamine.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            _activeExamine = null;
        }

        Refresh();
    }

    public void OnUseButtonClicked(int slotIndex)
    {
        if (Time.frameCount == _lastClickFrame) return;
        _lastClickFrame = Time.frameCount;

        List<string> items = Inv != null
            ? Inv.GetAllItems()
            : new List<string>();

        if (slotIndex >= items.Count) return;

        string   itemId = items[slotIndex];
        ItemData data    = Inv?.GetItemData(itemId);

        if (data == null || !data.isUsable) return;
        if (Handheld == null) return;

        Handheld.Equip(data);

        // SỬA: Trước đây bấm [E] Sử dụng xong KHÔNG có UI nào đổi cả -- Refresh() vẽ lại nhãn dòng (thêm
        // "(đang cầm)") + ShowItemDetails() đổi chữ hotbar [E] Sử dụng -> [E] Cất xuống ngay lập tức.
        Refresh();
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < items.Count)
            ShowItemDetails(Inv?.GetItemData(items[_selectedSlotIndex]), items[_selectedSlotIndex]);
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (Time.frameCount == _lastClickFrame) return;
        _lastClickFrame = Time.frameCount;

        List<string> items = Inv != null
            ? Inv.GetAllItems()
            : new List<string>();

        if (slotIndex < items.Count)
        {
            SelectSlot(slotIndex);
            OnItemClicked(items[slotIndex]);
        }
    }

    // Tìm SÂU (không chỉ con trực tiếp như Transform.Find) -- phương án dự phòng nếu vì lý do gì đó không
    // có ScrollRect để lấy .content trực tiếp.
    private static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform child in t)
        {
            var found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }
}