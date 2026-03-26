using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Phase1.VoVanThuan
{
    // Class kế thừa UnityEvent có tham số string để báo tên Item
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
    }
}