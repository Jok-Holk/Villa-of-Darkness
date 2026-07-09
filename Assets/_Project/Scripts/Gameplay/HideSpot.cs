using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HideSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _hidePosition;

    [Header("Door")]
    [SerializeField] private DoorController _door;
    [SerializeField] private float _doorWaitTime = 0.4f; // thời gian chờ cửa xoay xong

    private bool _playerIsHiding = false;
    private Vector3 _playerReturnPosition;
    private int _hideFrame = -1;
    private bool _isBusy = false;

    private static int _lastInteractFrame = -1;
    private static HideSpot _currentActive;

    public UnityEvent OnHide   = new UnityEvent();
    public UnityEvent OnReveal = new UnityEvent();

    public bool IsPlayerHiding => _playerIsHiding;
    public static bool AnyPlayerHiding =>
        _currentActive != null && _currentActive._playerIsHiding;

    private void Start()
    {
        if (_playerController == null)
            _playerController = FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        if (!_playerIsHiding || _isBusy) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (Time.frameCount == _hideFrame) return;
        Interact();
    }

    public void Interact()
    {
        if (_isBusy) return;
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        if (!_playerIsHiding)
            StartCoroutine(EnterRoutine());
        else
            StartCoroutine(ExitRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        _isBusy = true;
        _currentActive = this;
        _hideFrame = Time.frameCount;

        // 1. MỞ CỬA trước
        if (_door != null)
        {
            Debug.Log("[HideSpot] Gọi Door.Open()");
            _door.Open();
        }
        yield return new WaitForSeconds(_doorWaitTime);

        // 2. TELEPORT PLAYER VÀO TRONG
        if (_playerController != null)
        {
            _playerReturnPosition = _playerController.transform.position;

            var cc = _playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            var col = _playerController.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Vector3 hidePos = _hidePosition != null ? _hidePosition.position : transform.position;
            _playerController.transform.position = hidePos;

            _playerController.SetMovementEnabled(false);
        }

        // 3. ĐÓNG CỬA LẠI
        if (_door != null)
        {
            Debug.Log("[HideSpot] Gọi Door.Close()");
            _door.Close();
        }
        yield return new WaitForSeconds(_doorWaitTime);

        _playerIsHiding = true;
        _isBusy = false;
        Debug.Log($"[HideSpot] VÀO TỦ — AnyPlayerHiding = {AnyPlayerHiding}");
        OnHide.Invoke();
    }

    private IEnumerator ExitRoutine()
    {
        _isBusy = true;

        // 2. MỞ CỬA
        if (_door != null) _door.Open();
        yield return new WaitForSeconds(_doorWaitTime);

        // 3. TELEPORT PLAYER RA NGOÀI
        if (_playerController != null)
        {
            var cc = _playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            _playerController.transform.position = _playerReturnPosition;

            if (cc != null) cc.enabled = true;

            var col = _playerController.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            _playerController.SetInputEnabled(true);
        }

        // 4. ĐÓNG CỬA LẠI
        if (_door != null) _door.Close();
        yield return new WaitForSeconds(_doorWaitTime);

        if (_currentActive == this) _currentActive = null;
        _playerIsHiding = false;
        _isBusy = false;
        Debug.Log($"[HideSpot] THOÁT TỦ — AnyPlayerHiding = {AnyPlayerHiding}");
        OnReveal.Invoke();
    }

    private void OnDestroy()
    {
        if (_currentActive == this) _currentActive = null;
    }
}