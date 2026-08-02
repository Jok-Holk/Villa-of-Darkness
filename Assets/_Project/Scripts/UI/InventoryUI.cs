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

    [Header("Đọc nhật ký — UI paged-reader RIÊNG, khác hẳn Examine 3D thường (ch chỉ áp dụng cho đúng itemId này)")]
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
        rt.offsetMin = new Vector2(0f, -20f);
        rt.offsetMax = new Vector2(0f, 0f);

        return img;
    }

    private static void ExtendStripeHeight(RectTransform stripeRt)
    {
        if (stripeRt == null) return;
        stripeRt.offsetMin = new Vector2(stripeRt.offsetMin.x, -20f);
        stripeRt.offsetMax = new Vector2(stripeRt.offsetMax.x, 0f);
    }

    private static TMP_Text GetOrCreateStatusLabel(Transform slot)
    {
        var statusT = slot.Find("Status");
        GameObject statusGO = statusT != null ? statusT.gameObject : new GameObject("Status", typeof(RectTransform));
        statusGO.transform.SetParent(slot, false);

        var status = statusGO.GetComponent<TMP_Text>();
        if (status == null) status = statusGO.AddComponent<TextMeshProUGUI>();

        var statusRt = status.rectTransform;
        statusRt.anchoredPosition = new Vector2(statusRt.anchoredPosition.x, -58f);

        return status;
    }

    // ĐÃ SỬA: Ép xoá component ngay lập tức để tránh lỗi NullReference và cảnh báo Can't add component.
    private static TMP_Text FixLabelFontBug(TMP_Text label)
    {
        if (label == null) return null;

        GameObject go = label.gameObject;
        string savedText          = label.text;
        float savedFontSize       = label.fontSize;
        Color savedColor          = label.color;
        TextAlignmentOptions savedAlignment = label.alignment;
        bool savedAutoSizing      = label.enableAutoSizing;

        Object.DestroyImmediate(label);

        for (int i = go.transform.childCount - 1; i >= 0; i--)
        {
            var child = go.transform.GetChild(i);
            if (!child.name.StartsWith("TMP SubMeshUI")) continue;
            Object.DestroyImmediate(child.gameObject);
        }

        var fresh = go.AddComponent<TextMeshProUGUI>();
        fresh.text             = savedText;
        fresh.fontSize         = savedFontSize;
        fresh.color            = savedColor;
        fresh.alignment        = savedAlignment;
        fresh.enableAutoSizing = savedAutoSizing;
        fresh.fontStyle        = FontStyles.Bold;

        return fresh;
    }

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
        if (GhostAI.AnyGhostChasing)
        {
            Debug.LogWarning("[Inventory][debug] Open() bị chặn vì GhostAI.AnyGhostChasing = true -- nếu " +
                              "không có ma nào thực sự đang đuổi, đây là nguyên nhân Tab cần bấm nhiều lần.");
            return;
        }

        if (!_slotsCached) CacheSlots();

        _isOpen = true;
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (Player != null)
            Player.SetInputEnabled(false);

        InteractionSystem.IsInputBlocked = true;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        InteractPromptUI.Instance?.SetDotVisible(true);
        SetHudVisible(true);

        OnClose.Invoke();
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        if (!_isOpen) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            MoveSelection(-1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            MoveSelection(1);

        if (Input.GetKeyDown(KeyCode.E))
            UseSelected();

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

        EnsureSlotCount(items.Count);

        for (int i = 0; i < _slotIcons.Count; i++)
        {
            bool hasItem = i < items.Count;

            if (hasItem)
            {
                string   itemId = items[i];
                ItemData data   = Inv?.GetItemData(itemId);

                bool isEquippedRow = Handheld != null && Handheld.IsHoldingSomething && Handheld.CurrentItemId == itemId;
                string displayName = data != null && !string.IsNullOrEmpty(data.itemName) ? data.itemName : itemId;

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

                _slotBaseColors[i] = data != null && data.isKeyItem ? _keyItemColor
                                   : data != null && data.isUsable  ? _usableItemColor
                                   : _plainItemColor;

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

    private void UpdateScrollbarSize()
    {
        if (_listScrollRect == null) return;
        var scrollbar = _listScrollRect.verticalScrollbar;
        var viewport  = _listScrollRect.viewport;
        var content   = _listScrollRect.content;
        if (scrollbar == null || viewport == null || content == null) return;
        if (content.rect.height <= 0f) return;

        scrollbar.size = Mathf.Clamp01(viewport.rect.height / content.rect.height);

        var handleRect = scrollbar.handleRect;
        if (handleRect != null) handleRect.sizeDelta = new Vector2(handleRect.sizeDelta.x, 0f);
    }

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

        if (itemId == _diaryItemId && _diaryReader != null)
        {
            _diaryReader.Open();
            return;
        }

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

        if (_categoryText != null)
            _categoryText.text = GetItemStatusLabel(itemId, data, isEquipped).ToUpperInvariant();

        if (_detailIconImage != null)
        {
            bool hasIcon = data.icon != null;
            _detailIconImage.gameObject.SetActive(hasIcon);
            if (hasIcon) _detailIconImage.sprite = data.icon;
        }

        if (_itemDescText != null)
            _itemDescText.text = isEquipped ? $"{data.description}\n\n<color=#{ColorUtility.ToHtmlStringRGBA(_hintBrightColor)}>(Đang cầm trên tay)</color>" : data.description;

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

    private void UpdateMoveHint()
    {
        if (_footWText == null || _footSText == null) return;

        int count = Inv != null ? Inv.GetAllItems().Count : 0;
        bool canMoveUp   = _selectedSlotIndex > 0;
        bool canMoveDown = _selectedSlotIndex < count - 1;

        _footWText.color = canMoveUp   ? _hintBrightColor : _hintDimColor;
        _footSText.color = canMoveDown ? _hintBrightColor : _hintDimColor;
    }

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

            if (_activeExamine != null && _activeExamine.IsExamining)
            {
                _activeExamine.OnExamineEnd.RemoveListener(ReopenAfterExamine);
                _activeExamine.StopExamine();
            }

            _activeExamine = entry.examineItem;

            entry.examineItem.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            entry.examineItem.OnExamineEnd.AddListener(ReopenAfterExamine);

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