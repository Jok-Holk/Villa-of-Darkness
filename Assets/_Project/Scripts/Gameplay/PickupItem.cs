using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gắn lên object nhặt được (nến, key_01, hộp nhạc, v.v.)
/// Player nhìn vào và nhấn E → item vào Inventory, object ẩn đi.
/// Tạo ItemData asset tương ứng rồi kéo vào _itemData.
///
/// Phase 2.5 — Võ Văn Thuận
/// Path: Assets/_Project/Scripts/Gameplay/PickupItem.cs
/// </summary>
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [Tooltip("ItemData asset của vật phẩm này — tạo tại Create → Inventory → Item Data")]
    [SerializeField] private ItemData _itemData;

    [Header("References")]
    [SerializeField] private InventorySystem _inventorySystem;

    [Header("Audio")]
    [Tooltip("Tiếng nhặt đồ — để trống nếu không cần")]
    [SerializeField] private AudioClip _pickupSFX;

    [Header("Events")]
    public UnityEvent OnPickedUp = new UnityEvent();

    private bool _hasBeenPickedUp = false;

    public void Interact()
    {
        if (_hasBeenPickedUp) return;

        if (_itemData == null)
        {
            Debug.LogWarning($"[PickupItem] {gameObject.name} chưa gán _itemData!");
            return;
        }
        if (_inventorySystem == null)
        {
            Debug.LogWarning($"[PickupItem] {gameObject.name} chưa gán _inventorySystem!");
            return;
        }

        _hasBeenPickedUp = true;

        _inventorySystem.AddItem(_itemData.itemId);

        if (_pickupSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(_pickupSFX);

        Debug.Log($"[PickupItem] Đã nhặt: {_itemData.itemId}");
        OnPickedUp.Invoke();

        // Ẩn object sau khi nhặt (không Destroy để tránh mất reference Inspector)
        gameObject.SetActive(false);
    }
}