using UnityEngine;
using UnityEngine.Events;

public class ExamineItem : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private float _examineDistance = 0.6f;
    
    [Tooltip("Vị trí hiển thị khi soi 3D từ túi đồ (ví dụ: kéo sang phải một chút để không che UI)")]
    [SerializeField] private Vector3 _inventoryExamineOffset = new Vector3(0.35f, -0.05f, 0.6f);
    
    [Tooltip("Toc do xoay — gia tri hop ly: 3~6")]
    [SerializeField] private float _rotateSpeed = 4f;
    [Tooltip("Rotation offset khi item hien ra truoc camera.")]
    [SerializeField] private Vector3 _examineRotationOffset = Vector3.zero;

    [Header("References")]
    [SerializeField] private PlayerController _playerController;

    [Header("Nhặt sau khi Examine")]
    [SerializeField] private PickupItem _linkedPickupItem;
    [SerializeField] private KeyCode _pickupKey = KeyCode.F;

    [Header("Events")]
    public UnityEvent OnExamineStart = new UnityEvent();
    public UnityEvent OnExamineEnd   = new UnityEvent();

    private bool       _isExamining          = false;
    private bool       _disableIsIntentional = false;
    private int        _enterFrame           = -1;
    private float      _lastStopTime         = -1f;

    private Vector3    _originalPos;
    private Quaternion _originalRot;
    private Vector3    _originalScale;
    private Transform  _originalParent;

    private bool _openedFromInventory    = false;
    private Renderer[] _renderers;
    private bool[]     _rendererStates;

    public bool IsExamining => _isExamining;

    private void Reset()
    {
        if (_linkedPickupItem == null) _linkedPickupItem = GetComponent<PickupItem>();
    }

    private void Start()
    {
        if (_playerController == null) _playerController = PlayerController.Instance;
    }

    public void Interact() 
    {
        if (Time.time - _lastStopTime < 0.25f) return;
        StartExamine();
    }

    private void Update()
    {
        if (!_isExamining) return;

        // Chuột trái để xoay
        if (Input.GetMouseButton(0))
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float mouseX = Input.GetAxis("Mouse X") * _rotateSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * _rotateSpeed;
                transform.Rotate(cam.transform.up,    -mouseX, Space.World);
                transform.Rotate(cam.transform.right,  mouseY, Space.World);
            }
        }

        if (_linkedPickupItem != null && !_openedFromInventory
            && Input.GetKeyDown(_pickupKey) && Time.frameCount != _enterFrame)
        {
            _linkedPickupItem.DoPickup();
            StopExamine();
            return;
        }

        // ĐÃ SỬA: Tách biệt input tránh xung đột
        // 1. Đang soi ngoài map -> Bấm E để thoát
        if (!_openedFromInventory && Input.GetKeyDown(KeyCode.E) && Time.frameCount != _enterFrame)
            StopExamine();
            
        // 2. Đang soi trong túi đồ -> Bấm Chuột Phải để thoát (chống Input Bleed)
        if (_openedFromInventory && Input.GetMouseButtonDown(1) && Time.frameCount != _enterFrame)
            StopExamine();
    }

    public void StartExamineFromInventory()
    {
        _openedFromInventory = true;
        StartExamine();
    }

    public void StartExamine()
    {
        if (_isExamining) return;

        _renderers      = GetComponentsInChildren<Renderer>(includeInactive: true);
        _rendererStates = new bool[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _rendererStates[i]    = _renderers[i].enabled;
            _renderers[i].enabled = true;   
        }

        _disableIsIntentional = false;
        gameObject.SetActive(true);

        _isExamining = true;
        _enterFrame  = Time.frameCount;

        _originalParent = transform.parent;
        _originalPos    = transform.position;
        _originalRot    = transform.rotation;
        _originalScale  = transform.localScale;

        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.SetParent(cam.transform, worldPositionStays: true);
            
            // ĐÃ SỬA: Đẩy vật phẩm sang 1 bên nếu mở từ túi đồ
            transform.localPosition = _openedFromInventory ? _inventoryExamineOffset : new Vector3(0f, -0.05f, _examineDistance);
            transform.localRotation = Quaternion.Euler(_examineRotationOffset);
        }

        if (!_openedFromInventory && _playerController != null)
            _playerController.SetInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        SetBackdropActive(true);

        OnExamineStart.Invoke();
    }

    public void StopExamine()
    {
        if (!_isExamining) return;
        _isExamining = false;
        _lastStopTime = Time.time;

        transform.SetParent(_originalParent, worldPositionStays: true);
        transform.position   = _originalPos;
        transform.rotation   = _originalRot;
        transform.localScale = _originalScale;

        bool wasFromInventory  = _openedFromInventory;
        _openedFromInventory   = false;

        if (wasFromInventory)
        {
            if (_renderers != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                    if (_renderers[i] != null) _renderers[i].enabled = false;
            }
        }
        else
        {
            bool pickedUpDuringExamine = _linkedPickupItem != null && _linkedPickupItem.HasBeenPickedUp;
            
            if (_renderers != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                        _renderers[i].enabled = pickedUpDuringExamine ? false : _rendererStates[i];
                }
            }
        }

        _renderers      = null;
        _rendererStates = null;

        if (!wasFromInventory)
        {
            if (_playerController != null) _playerController.SetInputEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        SetBackdropActive(false);

        OnExamineEnd.Invoke();
    }

    private void OnDisable()
    {
        if (_disableIsIntentional)
        {
            _disableIsIntentional = false;
            return;
        }

        if (!_isExamining) return;

        transform.SetParent(_originalParent, worldPositionStays: true);
        transform.position   = _originalPos;
        transform.rotation   = _originalRot;
        transform.localScale = _originalScale;

        if (_renderers != null)
        {
            bool pickedUp = _linkedPickupItem != null && _linkedPickupItem.HasBeenPickedUp;
            bool shouldHide = _openedFromInventory || pickedUp;
            
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null)
                    _renderers[i].enabled = shouldHide ? false : _rendererStates[i];
        }
        _renderers      = null;
        _rendererStates = null;

        if (!_openedFromInventory && _playerController != null)
            _playerController.SetInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        SetBackdropActive(false);

        _isExamining         = false;
        _openedFromInventory = false;
    }

    // Che nền gameplay phía sau item khi soi 3D — tách biệt hẳn khỏi thế giới game.
    // Backdrop là 1 Cube mỏng đặt cố định trước Camera.main (xem VoD/Fix/17), luôn tồn tại sẵn trong scene, tắt/bật theo trạng thái examine.
    private static void SetBackdropActive(bool active)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Transform backdrop = cam.transform.Find("ExamineBackdrop");
        if (backdrop != null) backdrop.gameObject.SetActive(active);
    }
}