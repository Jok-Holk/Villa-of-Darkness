using UnityEngine;
using UnityEngine.Events;

    public class HideSpot : MonoBehaviour, IInteractable
    {
        private bool _playerIsHiding = false;
        public UnityEvent OnHide = new UnityEvent();
        public UnityEvent OnReveal = new UnityEvent();

        // Property để Test Runner truy cập trực tiếp
        public bool IsPlayerHiding => _playerIsHiding;

        public void Interact()
        {
            _playerIsHiding = !_playerIsHiding;
            if (_playerIsHiding) OnHide.Invoke();
            else OnReveal.Invoke();
        }
    }
