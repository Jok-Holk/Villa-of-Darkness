using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private bool _isOpen   = false;
    [SerializeField] private bool _isLocked = false;

    [Header("Animation")]
    [SerializeField] private float _openAngle = 90f;
    [SerializeField] private float _animSpeed = 3f;

    public UnityEvent OnDoorOpen  = new UnityEvent();
    public UnityEvent OnDoorClose = new UnityEvent();

    private Quaternion _closedRot;
    private Quaternion _openRot;
    private Quaternion _targetRot;

    private int _lastInteractFrame = -1;

    private void Start()
    {
        _closedRot = transform.localRotation;
        _openRot   = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, _openAngle, 0));
        _targetRot = _isOpen ? _openRot : _closedRot;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation, _targetRot, Time.deltaTime * _animSpeed);
    }

    // ─── IInteractable — player nhấn E trực tiếp vào cửa ─────────────────────
    public void Interact()
    {
        // _isLocked chỉ chặn player nhấn E trực tiếp
        // KHÔNG chặn Open() / Close() gọi từ code
        if (_isLocked) return;

        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        Toggle();
    }

    // ─── PUBLIC API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Mở cửa trực tiếp từ code (ItemLock, cutscene...).
    /// Không bị _isLocked hay frame guard chặn.
    /// </summary>
    public void Open()
    {
        if (_isOpen) return;
        _isOpen    = true;
        _targetRot = _openRot;
        Debug.Log("[Door] Mở (Open)");
        OnDoorOpen.Invoke();
    }

    /// <summary>Đóng cửa trực tiếp từ code.</summary>
    public void Close()
    {
        if (!_isOpen) return;
        _isOpen    = false;
        _targetRot = _closedRot;
        Debug.Log("[Door] Đóng (Close)");
        OnDoorClose.Invoke();
    }

    /// <summary>Toggle đóng/mở — dùng nội bộ bởi Interact().</summary>
    public void Toggle()
    {
        _isOpen = !_isOpen;
        if (_isOpen)
        {
            _targetRot = _openRot;
            Debug.Log("[Door] Mở");
            OnDoorOpen.Invoke();
        }
        else
        {
            _targetRot = _closedRot;
            Debug.Log("[Door] Đóng");
            OnDoorClose.Invoke();
        }
    }

    /// <summary>
    /// Giữ lại để tương thích với các file test cũ.
    /// Lock chỉ chặn player nhấn E — không chặn Open()/Close() từ code.
    /// </summary>
    public void SetLocked(bool state)
    {
        _isLocked = state;
        Debug.Log($"[Door] SetLocked({state})");
    }

    public bool IsOpen   => _isOpen;
    public bool IsLocked => _isLocked;
}