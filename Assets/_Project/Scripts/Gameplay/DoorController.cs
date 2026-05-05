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

    // Chống gọi Interact() 2 lần trong cùng 1 frame
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

    public void SetLocked(bool state) => _isLocked = state;

    public void Interact()
    {
        if (_isLocked) return;

        // Bỏ qua nếu đã gọi trong frame này rồi
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

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
}