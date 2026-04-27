using UnityEngine;
using UnityEngine.Events; // Quan trọng để dùng UnityEvent


    public class DoorController : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private bool _isLocked = false;

        public UnityEvent OnDoorOpen = new UnityEvent();
        public UnityEvent OnDoorClose = new UnityEvent();

        public void SetLocked(bool state) => _isLocked = state;
        public void Interact()
        {
            if (_isLocked) return; // QUAN TRỌNG: Test LockedDoor_CannotBeOpened cần dòng này
            _isOpen = !_isOpen;
            if (_isOpen) OnDoorOpen.Invoke(); else OnDoorClose.Invoke();
        }
    }
