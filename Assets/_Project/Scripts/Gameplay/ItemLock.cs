using UnityEngine;
using UnityEngine.Events;

public class ItemLock : MonoBehaviour, IInteractable
{
    [Header("Inventory")]
    [SerializeField] private InventorySystem _inventorySystem;

    [Header("Required Item")]
    [SerializeField] private string _requiredItemId;
    [SerializeField] private bool   _consumeRequired = true;

    [Header("Grant Item (tuỳ chọn)")]
    [SerializeField] private string _grantItemId;

    [Header("Door (tuỳ chọn)")]
    [Tooltip("Kéo DoorController vào để tự động gọi Open() ngay khi unlock.\n"
           + "Để trống nếu chỉ dùng OnUnlocked event.")]
    [SerializeField] private DoorController _doorController;

    [Header("Hint Text")]
    [SerializeField] private string _lockedHint   = "Cần thêm thứ gì đó để mở...";
    [SerializeField] private string _unlockedHint = "Đã mở.";

    [Header("Audio")]
    [SerializeField] private AudioClip _unlockSFX;
    [SerializeField] private AudioClip _lockedSFX;

    [Header("Events")]
    public UnityEvent OnUnlocked = new UnityEvent();
    public UnityEvent OnLocked   = new UnityEvent();

    private bool _isUnlocked = false;

    // ─── INTERACT ─────────────────────────────────────────────────────────────
    public void Interact()
    {
        if (_isUnlocked) return;

        if (_inventorySystem == null)
        {
            Debug.LogWarning($"[ItemLock] {gameObject.name} chưa gán _inventorySystem!");
            return;
        }

        bool hasItem = !string.IsNullOrEmpty(_requiredItemId)
                       && _inventorySystem.HasItem(_requiredItemId);

        if (!hasItem)
        {
            if (_lockedSFX != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(_lockedSFX);
            Debug.Log($"[ItemLock] {gameObject.name} — locked. Hint: {_lockedHint}");
            OnLocked.Invoke();
            return;
        }

        Unlock();
    }

    // ─── UNLOCK ───────────────────────────────────────────────────────────────
    private void Unlock()
    {
        _isUnlocked = true;

        if (_consumeRequired && !string.IsNullOrEmpty(_requiredItemId))
            _inventorySystem.RemoveItem(_requiredItemId);

        if (!string.IsNullOrEmpty(_grantItemId))
            _inventorySystem.AddItem(_grantItemId);

        if (_unlockSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(_unlockSFX);

        Debug.Log($"[ItemLock] {gameObject.name} — UNLOCKED!"
                  + (_consumeRequired && !string.IsNullOrEmpty(_requiredItemId)
                      ? $" Consumed: {_requiredItemId}" : "")
                  + (!string.IsNullOrEmpty(_grantItemId)
                      ? $" | Granted: {_grantItemId}" : ""));

        OnUnlocked.Invoke();

        // Mở khoá cửa rồi mở ra — sau đó player có thể đóng/mở tự do bằng E
        if (_doorController != null)
        {
            _doorController.SetLocked(false);
            _doorController.Open();
        }

        // Disable component này → InteractionSystem sẽ bỏ qua ItemLock
        // và tìm thấy DoorController cho các lần nhấn E tiếp theo
        this.enabled = false;
    }

    // ─── HELPER ───────────────────────────────────────────────────────────────
    public void ResetLock()
    {
        _isUnlocked  = false;
        this.enabled = true;
        Debug.Log($"[ItemLock] {gameObject.name} — lock reset.");
    }

    public bool IsUnlocked     => _isUnlocked;
    public string LockedHint   => _lockedHint;
    public string UnlockedHint => _unlockedHint;
}