using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private float _walkSpeed        = 3f;
    [SerializeField] private float _runSpeed         = 6f;
    [SerializeField] private float _crouchSpeed      = 1.5f;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private Transform _cameraTransform;

    [Header("Stamina — giới hạn chạy (Shift)")]
    [Tooltip("Số giây chạy liên tục tối đa (pin đầy)")]
    [SerializeField] private float _maxStamina        = 5f;
    [Tooltip("Tốc độ hồi stamina mỗi giây khi KHÔNG chạy")]
    [SerializeField] private float _staminaRegenRate  = 1f;
    [Tooltip("Hết stamina rồi phải hồi lại tới % này mới được chạy tiếp (tránh nhấp nhả chạy-hết-chạy liên tục)")]
    [Range(0f, 1f)] [SerializeField] private float _staminaResumeThresh = 0.2f;

    private float _stamina;
    private bool  _staminaLocked; // true khi vừa hết pin, chờ hồi đủ _staminaResumeThresh mới cho chạy lại
    private bool  _isRunning;     // trạng thái chạy THẬT SỰ (đã trừ trường hợp bị khoá do hết stamina) — UpdateAnimator() dùng cái này, không đọc thẳng phím Shift nữa
    public float Stamina01 => _stamina / _maxStamina;

    private CharacterController _cc;
    private Animator _anim; // Khai báo Animator

    private float _xRotation;
    private bool  _isCrouching;
    private float _speedParameter = 0f; // Biến tạm để làm mượt chuyển động của Animator

    // ─── INPUT FLAGS ──────────────────────────────────────────────────────────
    private bool _movementEnabled = true;
    private bool _lookEnabled     = true;

    private void Awake()
{
    Instance = this;

    _stamina = _maxStamina;

    _cc = GetComponent<CharacterController>();

    // SỬA DÒNG NÀY: Đổi từ GetComponent thành GetComponentInChildren
    _anim = GetComponentInChildren<Animator>();

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible   = false;
}

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (_lookEnabled)     HandleMouseLook();
        if (_movementEnabled) HandleMovement();
        if (_movementEnabled) HandleCrouch(); // guard bằng _movementEnabled — không crouch khi input bị lock
        
        // Luôn cập nhật Animator để đồng bộ trạng thái thực tế
        UpdateAnimator(); 
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation  = Mathf.Clamp(_xRotation, -80f, 80f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        bool wantsRun = !_isCrouching && isMoving && Input.GetKey(KeyCode.LeftShift)
                        && _stamina > 0f && !_staminaLocked;
        _isRunning = wantsRun;

        if (wantsRun)
        {
            _stamina = Mathf.Max(0f, _stamina - Time.deltaTime);
            if (_stamina <= 0f) _staminaLocked = true; // hết sạch — khoá tới khi hồi đủ ngưỡng resume
        }
        else
        {
            _stamina = Mathf.Min(_maxStamina, _stamina + _staminaRegenRate * Time.deltaTime);
            if (_staminaLocked && Stamina01 >= _staminaResumeThresh) _staminaLocked = false;
        }

        float speed = _isCrouching ? _crouchSpeed : wantsRun ? _runSpeed : _walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * speed * Time.deltaTime);

        if (!_cc.isGrounded)
            _cc.Move(Vector3.down * 9.8f * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
            _isCrouching = !_isCrouching;
    }

    private void UpdateAnimator()
    {
        if (_anim == null) return;

        // Nếu input di chuyển bị tắt (ví dụ đang trong cutscene), ép nhân vật về trạng thái đứng im
        if (!_movementEnabled)
        {
            _speedParameter = Mathf.Lerp(_speedParameter, 0f, Time.deltaTime * 5f);
            _anim.SetFloat("Speed", _speedParameter);
            _anim.SetBool("IsCrouching", _isCrouching);
            return;
        }

        // Kiểm tra xem người chơi có đang bấm nút di chuyển WASD không
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f);

        float targetSpeed = 0f;

        if (isMoving)
        {
            if (_isCrouching)
            {
                targetSpeed = 1f; // Crouch Walk (Trong Blend Tree: Crouch Idle = 0, Crouch Walk = 1)
            }
            else
            {
                // Chạy thật sự (còn stamina) thì ra dáng Chạy (Speed = 2), không thì Đi bộ (Speed = 1) —
                // đọc _isRunning (đã trừ trường hợp hết stamina) chứ không đọc thẳng phím Shift nữa.
                targetSpeed = _isRunning ? 2f : 1f;
            }
        }
        else
        {
            targetSpeed = 0f; // Đứng im (Idle hoặc Crouch Idle)
        }

        // Làm mượt giá trị Speed để animation chuyển đổi mượt mà, không bị giật
        _speedParameter = Mathf.Lerp(_speedParameter, targetSpeed, Time.deltaTime * 6f);
        
        // Truyền các giá trị vào Animator Controller
        _anim.SetFloat("Speed", _speedParameter);
        _anim.SetBool("IsCrouching", _isCrouching);
    }

    // ─── PUBLIC API ───────────────────────────────────────────────────────────

    /// <summary>Lock/unlock toàn bộ input (movement + look + crouch). Dùng cho cutscene, piano, inventory.</summary>
    public void SetInputEnabled(bool enabled)
    {
        _movementEnabled = enabled;
        _lookEnabled     = enabled;
    }

    /// <summary>Chỉ lock/unlock di chuyển WASD + crouch. Camera vẫn xoay được.</summary>
    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;
    }

    /// <summary>Chỉ lock/unlock xoay camera.</summary>
    public void SetLookEnabled(bool enabled)
    {
        _lookEnabled = enabled;
    }

    /// <summary>Đồng bộ lại góc pitch nội bộ sau khi script khác (vd IntroManager) tự xoay camera bằng
    /// tay lúc _lookEnabled=false — tránh camera "giật" về góc cũ ngay khung hình đầu tiên mouse-look chạy lại.</summary>
    public void SetPitch(float xRotationDegrees)
    {
        _xRotation = xRotationDegrees;
    }
}