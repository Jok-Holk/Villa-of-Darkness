using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public void AddItem(string itemId)
    {
        if (!GameData.collectedItems.Contains(itemId))
            GameData.collectedItems.Add(itemId);
    }
    public bool HasItem(string itemId) => GameData.collectedItems.Contains(itemId);
}
