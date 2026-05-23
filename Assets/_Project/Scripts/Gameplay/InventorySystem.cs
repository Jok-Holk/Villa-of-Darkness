using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class ItemEvent : UnityEvent<string> { }

public class InventorySystem : MonoBehaviour
{
    [Header("Kéo tất cả ItemData asset vào đây")]
    [SerializeField] private ItemData[] _itemDatabase;

    public ItemEvent OnItemAdded = new ItemEvent();

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
        GameData.collectedItems.Remove(id);
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

    [ContextMenu("Test: Clear All Items")]
    private void TestClearAll()
    {
        GameData.collectedItems.Clear();
        Debug.Log("[Inventory] Cleared all items");
    }
}