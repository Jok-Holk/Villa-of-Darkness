using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class ItemEvent : UnityEvent<string> { }

public class InventorySystem : MonoBehaviour
{
    public ItemEvent OnItemAdded = new ItemEvent();

    public void AddItem(string id)
    {
        if (!GameData.collectedItems.Contains(id))
        {
            GameData.collectedItems.Add(id);
            OnItemAdded.Invoke(id);
        }
    }

    public void RemoveItem(string id) => GameData.collectedItems.Remove(id);

    public bool HasItem(string id) => GameData.collectedItems.Contains(id);

    public List<string> GetAllItems() => new List<string>(GameData.collectedItems);

    // ─── CONTEXT MENU — dùng để test trong Play Mode ───────────────────────────
    // Chuột phải vào component InventorySystem trong Inspector → chọn tên method
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