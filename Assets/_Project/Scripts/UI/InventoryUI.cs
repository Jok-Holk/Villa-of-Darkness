using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    [Header("Panel chi tiết vật phẩm (bên phải)")]
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemDescText;
    [SerializeField] private string   _defaultDescHint = "Chọn 1 vật phẩm để xem chi tiết";

    [Header("Hint 'F Sử dụng' trên thanh phím tắt dưới cùng — luôn hiện, chỉ mờ đi khi item không dùng được")]
    [SerializeField] private TMP_Text _actionHintText;
    [SerializeField] private string   _useHint = "[F] Sử dụng";

    [Header("Hint 'W/S Di chuyển' — W mờ khi đang ở dòng đầu, S mờ khi đang ở dòng cuối")]
    [SerializeField] private TMP_Text _moveHintText;

    [Header("Cuộn danh sách — để trống thì tự tìm ScrollRect trong con Grid")]
    [SerializeField] private ScrollRect _listScrollRect;

    private Image[]         _slotIcons;
    private TMP_Text[]      _slotLabels;
    private Image[]         _slotStripes;      // sọc màu bên trái mỗi dòng — phân loại (vàng=key, xanh=usable), dày/sáng hơn khi được chọn
    private RectTransform[] _slotStripeRects;
    private Button[]        _slotUseButtons;
    private Color[]         _slotBaseColors;

    private static readonly Color _iconFilledColor  = Color.white;
    private static readonly Color _iconEmptyColor   = new Color(0.267f, 0.267f, 0.267f, 1f);
    private static readonly Color _keyItemColor     = new Color(1f, 0.85f, 0f);
    private static readonly Color _usableItemColor  = new Color(0.33f, 0.53f, 0.8f);
    private static readonly Color _plainItemColor   = new Color(0.15f, 0.15f, 0.15f);
    private static readonly Color _hintDimColor     = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private static readonly Color _hintBrightColor  = new Color(0.85f, 0.76f, 0.6f, 1f);
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

    private void Awake()
    {
        Instance = this;

        CacheSlots();
        gameObject.SetActive(false);

        GhostAI.OnPlayerSpotted += OnGhostSpottedPlayer;
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
        Transform grid = transform.Find("Grid");
        if (grid == null) return;

        if (_listScrollRect == null)
            _listScrollRect = GetComponentInChildren<ScrollRect>(includeInactive: true);

        int count        = grid.childCount;
        _slotIcons       = new Image[count];
        _slotLabels      = new TMP_Text[count];
        _slotStripes     = new Image[count];
        _slotStripeRects = new RectTransform[count];
        _slotUseButtons  = new Button[count];
        _slotBaseColors  = new Color[count];

        for (int i = 0; i < count; i++)
        {
            Transform slot  = grid.GetChild(i);
            _slotIcons[i]   = slot.Find("Icon")?.GetComponent<Image>();
            _slotLabels[i]  = slot.Find("Label")?.GetComponent<TMP_Text>();
            _slotStripes[i] = slot.Find("Stripe")?.GetComponent<Image>();
            _slotStripeRects[i] = _slotStripes[i] != null ? _slotStripes[i].GetComponent<RectTransform>() : null;

            int captured = i;

            Button btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnSlotClicked(captured));

            Transform useBtnTransform = slot.Find("UseButton");
            if (useBtnTransform != null)
            {
                Button useBtn = useBtnTransform.GetComponent<Button>();
                _slotUseButtons[i] = useBtn;
                if (useBtn != null)
                    useBtn.onClick.AddListener(() => OnUseButtonClicked(captured));
            }
        }
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else         Open();
    }

    public void Open()
    {
        // Chưa thoát khỏi sự bám đuổi của ma thì không cho mở túi đồ.
        if (GhostAI.AnyGhostChasing) return;

        if (_slotIcons == null) CacheSlots();

        _isOpen = true;
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (Player != null)
            Player.SetInputEnabled(false);

        // Chặn InteractionSystem (E tương tác thế giới, VD HideSpot) bắn trùng
        // trong lúc Tab đang mở — trước đây thiếu dòng này nên đứng trước chỗ nấp
        // + mở túi đồ cùng lúc có thể kích hoạt cả 2.
        InteractionSystem.IsInputBlocked = true;

        ClearItemDetails();

        OnOpen.Invoke();
        Refresh();
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

        OnClose.Invoke();
    }

    private void Update()
    {
        if (!_isOpen) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            MoveSelection(-1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            MoveSelection(1);

        if (Input.GetKeyDown(KeyCode.F))
            UseSelected();
    }

    public void Refresh()
    {
        if (_slotIcons == null) CacheSlots();
        if (_slotIcons == null || _slotIcons.Length == 0) return;

        List<string> items = Inv != null
            ? Inv.GetAllItems()
            : new List<string>();

        for (int i = 0; i < _slotIcons.Length; i++)
        {
            bool hasItem = i < items.Count;

            if (hasItem)
            {
                string   itemId = items[i];
                ItemData data   = Inv?.GetItemData(itemId);

                if (_slotLabels[i] != null)
                    _slotLabels[i].text = data != null && !string.IsNullOrEmpty(data.itemName)
                        ? data.itemName : itemId;

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

                if (_slotUseButtons[i] != null)
                {
                    bool showUse = data != null && data.isUsable;
                    _slotUseButtons[i].gameObject.SetActive(showUse);
                }
            }
            else
            {
                if (_slotLabels[i]  != null) _slotLabels[i].text   = string.Empty;
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
    }

    public void OnItemClicked(string itemId)
    {
        ItemData data = Inv?.GetItemData(itemId);

        ShowItemDetails(data, itemId);

        if (data != null && data.monologueClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(data.monologueClip);

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

        if (_itemDescText != null)
            _itemDescText.text = data.description;

        // Hotbar dưới cùng luôn hiện "[F] Sử dụng" — chỉ mờ đi (không ẩn hẳn) khi item không dùng được,
        // để giữ layout phím tắt cố định, không nhảy vị trí theo item.
        if (_actionHintText != null)
        {
            _actionHintText.text  = _useHint;
            _actionHintText.color = data.isUsable ? _hintBrightColor : _hintDimColor;
        }
    }

    private void ClearItemDetails()
    {
        if (_itemNameText != null) _itemNameText.text = string.Empty;
        if (_itemDescText != null) _itemDescText.text = _defaultDescHint;
        if (_actionHintText != null)
        {
            _actionHintText.text  = _useHint;
            _actionHintText.color = _hintDimColor;
        }
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

    // Mờ chữ "W" khi đang ở dòng đầu (không lên được nữa), mờ "S" khi đang ở dòng cuối —
    // dùng rich-text color tag trong CÙNG 1 TMP_Text, không cần tách 3 object riêng.
    private void UpdateMoveHint()
    {
        if (_moveHintText == null) return;

        int count = Inv != null ? Inv.GetAllItems().Count : 0;
        bool canMoveUp   = _selectedSlotIndex > 0;
        bool canMoveDown = _selectedSlotIndex < count - 1;

        string bright = ColorUtility.ToHtmlStringRGBA(_hintBrightColor);
        string dim    = ColorUtility.ToHtmlStringRGBA(_hintDimColor);

        string wColor = canMoveUp   ? bright : dim;
        string sColor = canMoveDown ? bright : dim;

        _moveHintText.text = $"<color=#{wColor}>W</color> / <color=#{sColor}>S</color>   Di chuyển";
    }

    // Không tô nền cả dòng khi chọn — chỉ sọc bên trái dày/sáng hơn + tên chữ sáng hơn (xem Refresh/Label).
    private void UpdateSelectionVisuals()
    {
        if (_slotStripes == null) return;

        for (int i = 0; i < _slotStripes.Length; i++)
        {
            bool isSelected = i == _selectedSlotIndex;

            if (_slotStripes[i] != null)
                _slotStripes[i].color = isSelected ? Color.Lerp(_slotBaseColors[i], Color.white, 0.4f) : _slotBaseColors[i];

            if (_slotStripeRects[i] != null)
                _slotStripeRects[i].sizeDelta = new Vector2(isSelected ? _stripeWidthSelected : _stripeWidthNormal, _slotStripeRects[i].sizeDelta.y);

            if (_slotLabels[i] != null)
                _slotLabels[i].color = isSelected ? _hintBrightColor : new Color(0.65f, 0.65f, 0.65f);
        }
    }

    private void ScrollToSelected()
    {
        if (_listScrollRect == null || _slotStripes == null || _slotStripes.Length == 0) return;
        if (_selectedSlotIndex < 0) return;

        float t = _slotStripes.Length <= 1 ? 0f : 1f - (float)_selectedSlotIndex / (_slotStripes.Length - 1);
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
}