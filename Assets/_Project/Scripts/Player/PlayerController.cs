using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed  = 6f;
    [SerializeField] private float _crouchSpeed = 1.5f;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _cc;
    private float _xRotation;
    private bool _isCrouching;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float speed = _isCrouching ? _crouchSpeed
            : Input.GetKey(KeyCode.LeftShift) ? _runSpeed
            : _walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * speed * Time.deltaTime);

        // Gravity
        if (!_cc.isGrounded)
            _cc.Move(Vector3.down * 9.8f * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
            _isCrouching = !_isCrouching;
    }

    // Gọi từ nơi khác để tắt input khi cutscene
    public void SetInputEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}