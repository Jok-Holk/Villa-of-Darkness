using UnityEngine;
using UnityEngine.Events;

public class HideSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _hidePosition;

    private bool _playerIsHiding = false;
    private Vector3 _playerReturnPosition;
    private int _hideFrame = -1;

    public UnityEvent OnHide   = new UnityEvent();
    public UnityEvent OnReveal = new UnityEvent();

    public bool IsPlayerHiding => _playerIsHiding;

    private static HideSpot _currentActive;

    public static bool AnyPlayerHiding =>
        _currentActive != null && _currentActive._playerIsHiding;

    private void Update()
    {
        if (!_playerIsHiding) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (Time.frameCount == _hideFrame) return;
        Interact();
    }

    public void Interact()
    {
        _playerIsHiding = !_playerIsHiding;

        if (_playerIsHiding)
        {
            _currentActive = this;
            _hideFrame     = Time.frameCount;

            if (_playerController != null)
            {
                _playerReturnPosition = _playerController.transform.position;

                Vector3 hidePos = _hidePosition != null
                    ? _hidePosition.position
                    : transform.position;
                _playerController.transform.position = hidePos;
                _playerController.SetInputEnabled(false);

                // Tắt collider của player → ghost không OnTriggerStay được nữa
                var col = _playerController.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // Tắt CharacterController để không conflict với collider
                var cc = _playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
            }

            Debug.Log($"[HideSpot] VÀO TỦ — AnyPlayerHiding = {AnyPlayerHiding}");
            OnHide.Invoke();
        }
        else
        {
            if (_currentActive == this) _currentActive = null;

            if (_playerController != null)
            {
                // Bật lại CharacterController trước khi teleport
                var cc = _playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = true;

                _playerController.transform.position = _playerReturnPosition;
                _playerController.SetInputEnabled(true);

                // Bật lại collider
                var col = _playerController.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            Debug.Log($"[HideSpot] THOÁT TỦ — AnyPlayerHiding = {AnyPlayerHiding}");
            OnReveal.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (_currentActive == this) _currentActive = null;
    }
}