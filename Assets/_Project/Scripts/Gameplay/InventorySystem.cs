using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class ItemEvent : UnityEvent<string> { }

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Kéo tất cả ItemData asset vào đây")]
    [SerializeField] private ItemData[] _itemDatabase;

    [Header("Liên kết tay cầm — để trống thì tự lấy HandheldItemController.Instance lúc chạy")]
    [SerializeField] private HandheldItemController _handheldController;

    public ItemEvent OnItemAdded = new ItemEvent();
    public ItemEvent OnItemRemoved = new ItemEvent(); // Cần khai báo thêm event này để UI biết mà load lại

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (_handheldController == null) _handheldController = HandheldItemController.Instance;
        SyncAlreadyCollectedPickups();
    }

    // Scene vừa load (lần đầu, Retry, hay debug-jump checkpoint) -- ẩn NGAY mọi PickupItem trong world mà
    // item tương ứng đã có sẵn trong GameData.collectedItems, tránh vừa có trong túi vừa "hồi sinh" đứng
    // ngoài world. Item bị revertInventory loại khỏi collectedItems thì KHÔNG nằm trong danh sách này nữa
    // -- PickupItem của nó giữ nguyên trạng thái mặc định (hiện/nhặt được), đúng ý "đồ hồi lại trong map".
    private void SyncAlreadyCollectedPickups()
    {
        var allPickups = FindObjectsByType<PickupItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in allPickups)
        {
            if (p.Data != null && GameData.collectedItems.Contains(p.Data.itemId))
                p.SyncAlreadyPickedUp();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─── ITEM DATABASE ─────────────────────────────────────────────────────────
    /// <summary>Tìm ItemData theo itemId.</summary>
    public ItemData GetItemData(string itemId)
    {
        if (_itemDatabase == null) return null;
        foreach (var data in _itemDatabase)
            if (data != null && data.itemId == itemId) return data;
        return null;
    }

    // ─── LOGIC ─────────────────────────────────────────────────────────────────
    public void AddItem(string id)
    {
        if (!GameData.collectedItems.Contains(id))
        {
            GameData.collectedItems.Add(id);
            OnItemAdded.Invoke(id);
            Debug.Log($"[Inventory] Thêm: {id}");

            // Vật phẩm ĐẦU TIÊN trong suốt đời game -- dạy người chơi mới biết Tab mở túi đồ, chỉ 1 lần
            // duy nhất là đủ nhớ, không cần lặp lại mỗi lần nhặt thêm.
            if (GameData.collectedItems.Count == 1)
                TutorialHintUI.Instance.ShowOnce("tab_inventory", "Tab", "Mở túi đồ");
        }
    }

    public void RemoveItem(string id)
    {
        // Không xóa key item
        ItemData data = GetItemData(id);
        if (data != null && data.isKeyItem)
        {
            Debug.Log($"[Inventory] {id} là di vật quan trọng, không thể bỏ.");
            return;
        }

        if (GameData.collectedItems.Contains(id))
        {
            GameData.collectedItems.Remove(id);

            // ── ĐÂY LÀ ĐOẠN LOGIC GIÚP CẤT ĐỒ KHỎI TAY ──
            if (_handheldController != null && _handheldController.CurrentItemId == id)
            {
                _handheldController.Unequip();
            }

            if (OnItemRemoved != null) OnItemRemoved.Invoke(id);
            Debug.Log($"[Inventory] Đã xóa: {id}");
        }
    }

    public bool HasItem(string id) => GameData.collectedItems.Contains(id);

    public List<string> GetAllItems() => new List<string>(GameData.collectedItems);

    // ─── CONTEXT MENU — test trong Play Mode ───────────────────────────────────
    [ContextMenu("Test: Add music_box")]
    private void TestAddMusicBox() => AddItem("music_box");

    [ContextMenu("Test: Add mirror")]
    private void TestAddMirror() => AddItem("mirror");

    [ContextMenu("Test: Add salt_jar")]
    private void TestAddSaltJar() => AddItem("salt_jar");

    [ContextMenu("Debug: Add ALL Items (kể cả nhật ký)")]
    private void DebugAddAllItems()
    {
        if (_itemDatabase == null) { Debug.LogWarning("[Inventory] _itemDatabase trống."); return; }
        int count = 0;
        foreach (var data in _itemDatabase)
        {
            if (data == null || string.IsNullOrEmpty(data.itemId)) continue;
            AddItem(data.itemId);
            count++;
        }
        Debug.Log($"[Inventory] Debug Add ALL: đã thêm {count} item.");
    }

    [ContextMenu("Test: Clear All Items")]
    private void TestClearAll()
    {
        GameData.collectedItems.Clear();
        if (_handheldController != null) _handheldController.Unequip();
        Debug.Log("[Inventory] Cleared all items");
    }
}