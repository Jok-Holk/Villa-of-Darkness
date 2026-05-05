using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySystem _inventorySystem;
    [SerializeField] private AudioClip _itemSelectClip;

    private Image[]    _slotIcons;
    private TMP_Text[] _slotLabels;

    private static readonly Color _iconFilledColor = Color.white;
    private static readonly Color _iconEmptyColor  = new Color(0.267f, 0.267f, 0.267f, 1f);

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    public UnityEvent OnOpen  = new UnityEvent();
    public UnityEvent OnClose = new UnityEvent();

    // ─── AWAKE: chỉ SetActive(false), KHÔNG cache slot ở đây ──────────────────
    // Lý do: trong test, BuildWithGrid() gọi AddComponent<InventoryUI>() TRƯỚC
    // khi tạo Grid và Slot con → nếu cache trong Awake thì Find("Grid") = null.
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    // ─── START: cache slot sau khi tất cả child đã được tạo ───────────────────
    private void Start()
    {
        CacheSlots();
    }

    private void CacheSlots()
    {
        Transform grid = transform.Find("Grid");
        if (grid == null) return;

        int count   = grid.childCount;
        _slotIcons  = new Image[count];
        _slotLabels = new TMP_Text[count];

        for (int i = 0; i < count; i++)
        {
            Transform slot = grid.GetChild(i);
            _slotIcons[i]  = slot.Find("Icon")?.GetComponent<Image>();
            _slotLabels[i] = slot.Find("Label")?.GetComponent<TMP_Text>();

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
        // Cache lại slot nếu chưa có (trường hợp Start() chưa chạy khi panel inactive)
        if (_slotIcons == null) CacheSlots();

        _isOpen = true;
        gameObject.SetActive(true);
        OnOpen.Invoke();
        Refresh();
    }

    public void Close()
    {
        _isOpen = false;
        gameObject.SetActive(false);
        OnClose.Invoke();
    }

    public void Refresh()
    {
        // Cache lại nếu cần
        if (_slotIcons == null) CacheSlots();
        if (_slotIcons == null || _slotIcons.Length == 0) return;

        List<string> items = _inventorySystem != null
            ? _inventorySystem.GetAllItems()
            : new List<string>();

        for (int i = 0; i < _slotIcons.Length; i++)
        {
            bool hasItem = i < items.Count;

            if (_slotLabels[i] != null)
                _slotLabels[i].text = hasItem ? items[i] : string.Empty;

            if (_slotIcons[i] != null)
                _slotIcons[i].color = hasItem ? _iconFilledColor : _iconEmptyColor;
        }
    }

    public void OnItemClicked(string itemId)
    {
        if (AudioManager.Instance != null && _itemSelectClip != null)
            AudioManager.Instance.PlaySFX(_itemSelectClip);

        Debug.Log($"[InventoryUI] Clicked item: {itemId}");
    }

    private void OnSlotClicked(int slotIndex)
    {
        List<string> items = _inventorySystem != null
            ? _inventorySystem.GetAllItems()
            : new List<string>();

        if (slotIndex < items.Count)
            OnItemClicked(items[slotIndex]);
    }
}