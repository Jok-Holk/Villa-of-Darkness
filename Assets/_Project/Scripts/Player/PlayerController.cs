using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed        = 3f;
    [SerializeField] private float _runSpeed         = 6f;
    [SerializeField] private float _crouchSpeed      = 1.5f;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _cc;
    private float _xRotation;
    private bool  _isCrouching;

    // ─── INPUT FLAGS ──────────────────────────────────────────────────────────
    private bool _movementEnabled = true;
    private bool _lookEnabled     = true;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (_lookEnabled)     HandleMouseLook();
        if (_movementEnabled) HandleMovement();
        if (_movementEnabled) HandleCrouch(); // guard bằng _movementEnabled — không crouch khi input bị lock
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

        float speed = _isCrouching                       ? _crouchSpeed
                    : Input.GetKey(KeyCode.LeftShift)    ? _runSpeed
                    : _walkSpeed;

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
}