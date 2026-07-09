using UnityEngine;
using UnityEngine.Events;

public class ItemLock : MonoBehaviour, IInteractable
{
    [Header("Inventory & Hand")]
    [SerializeField] private InventorySystem _inventorySystem;
    [SerializeField] private HandheldItemController _handheldController;

    [Header("Required Item")]
    [SerializeField] private string _requiredItemId;
    [SerializeField] private bool   _consumeRequired = true;

    [Header("Door (tuỳ chọn)")]
    [SerializeField] private DoorController _doorController;

    [Header("Audio & Events")]
    [SerializeField] private AudioClip _unlockSFX;
    [SerializeField] private AudioClip _lockedSFX;
    public UnityEvent OnUnlocked = new UnityEvent();
    public UnityEvent OnLocked   = new UnityEvent();

    private bool _isUnlocked = false;

    public void Interact()
    {
        if (_isUnlocked) return;

        // KIỂM TRA: Có cầm trên tay không?
        bool isHolding = _handheldController != null && 
                         _handheldController.IsHoldingSomething && 
                         _handheldController.CurrentItemId == _requiredItemId;

        if (!isHolding)
        {
            if (_lockedSFX != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(_lockedSFX);
            
            Debug.Log($"[ItemLock] Bạn cần cầm {_requiredItemId} trên tay!");
            OnLocked.Invoke();
            return;
        }

        Unlock();
    }

    private void Unlock()
    {
        _isUnlocked = true;

        if (_consumeRequired)
        {
            _inventorySystem.RemoveItem(_requiredItemId);
            // Sau khi dùng xong, cất item đi
            _handheldController.Unequip(); 
        }

        if (_unlockSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(_unlockSFX);

        if (_doorController != null)
        {
            _doorController.SetLocked(false);
            _doorController.Open();
        }

        OnUnlocked.Invoke();
        this.enabled = false; // Tắt ổ khóa để tương tác thẳng vào DoorController
    }
}