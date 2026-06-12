using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gắn lên object nhặt được ngoài scene.
/// Nhấn E → thêm vào Inventory, tắt MeshRenderer + Collider (KHÔNG SetActive false).
/// Như vậy ExamineItem (nếu trên cùng GameObject) vẫn có thể được bật bởi
/// StartExamineFromInventory() khi cần xem từ túi đồ.
///
/// NẾU ExamineItem là proxy RIÊNG (khác GameObject):
///   → vẫn hoạt động bình thường, không ảnh hưởng.
///
/// NẾU ExamineItem nằm TRÊN CÙNG GameObject này:
///   → Sau khi nhặt, object bị ẩn bằng cách tắt Renderer + Collider.
///   → ExamineItem.StartExamineFromInventory() sẽ SetActive(true) + tắt Renderer
///      trong lúc xem → sau khi xem xong trả lại trạng thái ẩn đúng.
/// </summary>
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemData _itemData;

    [Header("References")]
    [SerializeField] private InventorySystem _inventorySystem;

    [Header("Audio")]
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

        // ── THAY ĐỔI CHÍNH ─────────────────────────────────────────────────────
        // Trước đây: gameObject.SetActive(false)
        //   → Tắt luôn cả ExamineItem component nếu nó trên cùng object.
        //   → Khi InventoryUI gọi StartExamineFromInventory(), object đã bị disabled,
        //     StartExamine() không chạy được.
        //
        // Bây giờ: ẩn visual + collider, KHÔNG tắt GameObject.
        //   → ExamineItem vẫn có thể được gọi bởi InventoryUI.
        //   → Object không còn "thấy được" và không thể nhặt lại (Collider tắt).
        // ───────────────────────────────────────────────────────────────────────
        HideInScene();
    }

    /// <summary>Ẩn object trong scene mà không dùng SetActive(false).</summary>
    private void HideInScene()
    {
        // Tắt tất cả Renderer → không nhìn thấy
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Tắt tất cả Collider → không nhặt lại được, không block vật lý
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Tắt chính component này → InteractionSystem sẽ bỏ qua (FindEnabledInteractable)
        this.enabled = false;
    }

    /// <summary>
    /// Bật lại visual + collider nếu cần reset (respawn scene, v.v.).
    /// </summary>
    public void ResetPickup()
    {
        _hasBeenPickedUp = false;
        this.enabled     = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = true;
    }
}