using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private bool _isOpen   = false;
    [SerializeField] private bool _isLocked = false;

    [Header("Cài đặt Loại cửa")]
    [Tooltip("Tick vào nếu đây là ngăn kéo (trượt). Bỏ tick nếu là cửa phòng (xoay).")]
    [SerializeField] private bool _isDrawer = false;
    
    [Tooltip("Tick vào nếu muốn cửa/ngăn kéo TỰ ĐỘNG KHÓA sau khi đóng lại.")]
    [SerializeField] private bool _autoLockOnClose = false; // <-- TÍNH NĂNG MỚI THÊM VÀO
    
    [Header("Animation - Ngăn kéo (Position)")]
    [SerializeField] private Vector3 _slideOffset = new Vector3(0, 0, 0.4f);

    [Header("Animation - Cửa cánh (Rotation)")]
    [SerializeField] private float _openAngle = 90f;

    [Header("Hé cửa (dùng khi trốn — không đóng kín hẳn để có khe hở nhìn ra ngoài)")]
    [SerializeField] private float _ajarAngle = 15f;
    
    [Header("Animation - Tốc độ chung")]
    [SerializeField] private float _animSpeed = 3f;

    public UnityEvent OnDoorOpen  = new UnityEvent();
    public UnityEvent OnDoorClose = new UnityEvent();

    private Quaternion _closedRot;
    private Quaternion _openRot;
    private Quaternion _ajarRot;
    private Quaternion _targetRot;

    private Vector3 _closedPos;
    private Vector3 _openPos;
    private Vector3 _targetPos;

    private int _lastInteractFrame = -1;

    private void Start()
    {
        _closedRot = transform.localRotation;
        _openRot   = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, _openAngle, 0));
        _ajarRot   = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, _ajarAngle, 0));

        _closedPos = transform.localPosition;
        _openPos   = _closedPos + _slideOffset;

        _targetRot = _isOpen ? _openRot : _closedRot;
        _targetPos = _isOpen ? _openPos : _closedPos;
    }

    private void Update()
    {
        if (_isDrawer)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, _targetPos, Time.deltaTime * _animSpeed);
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, _targetRot, Time.deltaTime * _animSpeed);
        }
    }

    public void Interact()
    {
        // Bị khóa thì không cho bấm E tác động trực tiếp
        if (_isLocked) return;

        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        Toggle();
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen    = true;
        
        _targetRot = _openRot; 
        _targetPos = _openPos; 
        
        Debug.Log("[Door] Mở tủ");
        OnDoorOpen.Invoke();
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen    = false;
        
        _targetRot = _closedRot;
        _targetPos = _closedPos;
        
        Debug.Log("[Door] Đóng tủ");
        OnDoorClose.Invoke();

        // TỰ ĐỘNG CHỐT KHÓA NẾU ĐƯỢC TICK
        if (_autoLockOnClose)
        {
            SetLocked(true);
            Debug.Log("[Door] Tủ đã tự động chốt khóa!");
        }
    }

    // Hé cửa (dùng khi player đang trốn) — không đóng kín, giữ 1 khe hở nhỏ để nhìn ra ngoài.
    public void SetAjar()
    {
        _isOpen    = true; // coi như đang mở 1 phần — Interact() thường (Toggle) sẽ đóng kín nếu bấm nhầm
        _targetRot = _ajarRot;
        _targetPos = _openPos; // ngăn kéo (nếu có) vẫn coi hé = mở, không dùng cho cửa cánh
    }

    public void Toggle()
    {
        if (_isOpen)
        {
            Close(); // Gọi hàm Close để tận dụng tính năng Auto Lock
        }
        else
        {
            Open();
        }
    }

    public void SetLocked(bool state)
    {
        _isLocked = state;
    }

    public bool IsOpen   => _isOpen;
    public bool IsLocked => _isLocked;
}