using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySystem  _inventorySystem;

    [Header("Kéo Player vào để tắt input khi mở inventory")]
    [SerializeField] private PlayerController _playerController;

    [Header("Xem 3D item từ túi đồ")]
    [Tooltip("itemId → ExamineItem proxy. Click item → xem 3D → E → về lại túi đồ.")]
    [SerializeField] private ExamineItemEntry[] _examineRegistry;

    [System.Serializable]
    public struct ExamineItemEntry
    {
        public string     itemId;
        public ExamineItem examineItem;
    }

    private Image[]    _slotIcons;
    private TMP_Text[] _slotLabels;
    private Image[]    _slotBorders;

    private static readonly Color _iconFilledColor = Color.white;
    private static readonly Color _iconEmptyColor  = new Color(0.267f, 0.267f, 0.267f, 1f);

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    // BUG FIX 5: track xem có examine đang chạy không để chặn Tab
    private ExamineItem _activeExamine = null;
    public  bool IsExamining => _activeExamine != null && _activeExamine.IsExamining;

    private int _lastClickFrame = -1;

    public UnityEvent OnOpen  = new UnityEvent();
    public UnityEvent OnClose = new UnityEvent();

    private void Awake()
    {
        CacheSlots();
        gameObject.SetActive(false);
    }

    private void CacheSlots()
    {
        Transform grid = transform.Find("Grid");
        if (grid == null) return;

        int count    = grid.childCount;
        _slotIcons   = new Image[count];
        _slotLabels  = new TMP_Text[count];
        _slotBorders = new Image[count];

        for (int i = 0; i < count; i++)
        {
            Transform slot  = grid.GetChild(i);
            _slotIcons[i]   = slot.Find("Icon")?.GetComponent<Image>();
            _slotLabels[i]  = slot.Find("Label")?.GetComponent<TMP_Text>();
            _slotBorders[i] = slot.GetComponent<Image>();

            int captured = i;
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnSlotClicked(captured));
        }
    }

    // ─── PUBLIC API ────────────────────────────────────────────────────────────
    public void Toggle()
    {
        if (_isOpen) Close();
        else         Open();
    }

    public void Open()
    {
        if (_slotIcons == null) CacheSlots();

        _isOpen = true;
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (_playerController != null)
            _playerController.SetInputEnabled(false);

        OnOpen.Invoke();
        Refresh();
    }

    public void Close()
    {
        // BUG FIX 3: Nếu đang có examine chạy, dừng examine trước, KHÔNG đóng inventory ngay.
        // ReopenAfterExamine sẽ KHÔNG được gọi vì ta xóa listener trước.
        if (_activeExamine != null && _activeExamine.IsExamining)
        {
            _activeExamine.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            _activeExamine.StopExamine();
            _activeExamine = null;
        }

        _isOpen = false;
        gameObject.SetActive(false);

        if (_playerController != null)
            _playerController.SetInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        OnClose.Invoke();
    }

    // ─── REFRESH ───────────────────────────────────────────────────────────────
    public void Refresh()
    {
        if (_slotIcons == null) CacheSlots();
        if (_slotIcons == null || _slotIcons.Length == 0) return;

        List<string> items = _inventorySystem != null
            ? _inventorySystem.GetAllItems()
            : new List<string>();

        for (int i = 0; i < _slotIcons.Length; i++)
        {
            bool hasItem = i < items.Count;

            if (hasItem)
            {
                string   itemId = items[i];
                ItemData data   = _inventorySystem?.GetItemData(itemId);

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

                if (_slotBorders[i] != null && data != null)
                    _slotBorders[i].color = data.isKeyItem
                        ? new Color(1f, 0.85f, 0f)
                        : new Color(0.1f, 0.1f, 0.1f);
            }
            else
            {
                if (_slotLabels[i]  != null) _slotLabels[i].text   = string.Empty;
                if (_slotIcons[i]   != null)
                {
                    _slotIcons[i].sprite = null;
                    _slotIcons[i].color  = _iconEmptyColor;
                }
                if (_slotBorders[i] != null) _slotBorders[i].color = new Color(0.1f, 0.1f, 0.1f);
            }
        }
    }

    // ─── ON ITEM CLICKED ───────────────────────────────────────────────────────
    public void OnItemClicked(string itemId)
    {
        ItemData data = _inventorySystem?.GetItemData(itemId);

        if (data != null && data.monologueClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(data.monologueClip);

        if (!TryOpenExamine(itemId))
        {
            if (data != null)
                Debug.Log($"[InventoryUI] {data.itemName}: {data.description}");
        }
    }

    private bool TryOpenExamine(string itemId)
    {
        if (_examineRegistry == null) return false;

        foreach (var entry in _examineRegistry)
        {
            if (entry.itemId != itemId || entry.examineItem == null) continue;

            // Ẩn panel inventory tạm thời — _isOpen giữ nguyên true
            gameObject.SetActive(false);

            // BUG FIX 3: Lưu lại examine đang active để Tab/Close có thể kiểm tra
            _activeExamine = entry.examineItem;

            // Đăng ký callback — RemoveListener trước để không bị double-register
            entry.examineItem.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            entry.examineItem.OnExamineEnd.AddListener(ReopenAfterExamine);

            // StartExamineFromInventory() tự gọi SetActive(true) bên trong
            entry.examineItem.StartExamineFromInventory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gọi khi ExamineItem.OnExamineEnd fire (player nhấn E thoát xem).
    /// Mở lại panel inventory, player vẫn locked.
    /// </summary>
    private void ReopenAfterExamine()
    {
        // BUG FIX 3: Xóa listener + reset active examine ngay khi callback chạy
        if (_activeExamine != null)
        {
            _activeExamine.OnExamineEnd.RemoveListener(ReopenAfterExamine);
            _activeExamine = null;
        }

        _isOpen = true;
        gameObject.SetActive(true);

        if (_playerController != null)
            _playerController.SetInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        Refresh();
        Debug.Log("[InventoryUI] Mở lại sau khi thoát xem.");
    }

    // ─── INTERNAL ──────────────────────────────────────────────────────────────
    private void OnSlotClicked(int slotIndex)
    {
        if (Time.frameCount == _lastClickFrame) return;
        _lastClickFrame = Time.frameCount;

        List<string> items = _inventorySystem != null
            ? _inventorySystem.GetAllItems()
            : new List<string>();

        if (slotIndex < items.Count)
            OnItemClicked(items[slotIndex]);
    }
}