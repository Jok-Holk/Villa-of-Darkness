using UnityEngine;
using UnityEngine.Events;

public class ExamineItem : MonoBehaviour, IInteractable, IInteractableLabel
{
    [Header("Settings")]
    [Tooltip("Toc do xoay — gia tri hop ly: 3~6")]
    [SerializeField] private float _rotateSpeed = 4f;
    [Tooltip("Rotation offset khi item hien ra truoc camera.")]
    [SerializeField] private Vector3 _examineRotationOffset = Vector3.zero;

    // BUG THẬT (Jok phát hiện): tiêu đề "ĐANG XEM / tên vật" trên ExamineStageUI chỉ hiện khi mở TỪ
    // Inventory (InventoryUI.OnItemClicked gọi SetItemTitle trước) -- soi TRỰC TIẾP ngoài world (ExamineItem
    // gắn thẳng lên prop, không qua Inventory) chưa từng gọi SetItemTitle() nên tiêu đề luôn trống. Thêm field
    // này để: (1) cấp tên cho prompt "[E] Tên vật" ngoài world qua IInteractableLabel (giống DoorController/
    // WindowEntryTrigger đã làm), (2) truyền cùng tên đó vào title card lúc Show().
    [Header("Tên hiển thị (soi trực tiếp ngoài world)")]
    [Tooltip("Hiện ở prompt '[E] ...' khi ngắm trúng NGOÀI world, và ở tiêu đề 'ĐANG XEM' lúc soi. Để trống nếu item này CHỈ soi từ Inventory (Inventory đã tự truyền tên qua ItemData).")]
    [SerializeField] private string _itemName;

    public string InteractLabel => _itemName;

    [Header("References")]
    [SerializeField] private PlayerController _playerController;

    [Header("Nhặt sau khi Examine")]
    [SerializeField] private PickupItem _linkedPickupItem;
    [SerializeField] private KeyCode _pickupKey = KeyCode.E;

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

    // KIẾN TRÚC (Jok yêu cầu): luồng UI chỉ được 1 CHIỀU -- Gameplay -> Inventory -> Examine/Diary, thoát
    // đúng bằng phím riêng (Chuột phải/E), KHÔNG được bấm Tab đè chồng lên trong lúc đang Examine. InventoryUI.
    // IsExamining CHỈ biết case soi TỪ Inventory (_activeExamine) -- soi TRỰC TIẾP ngoài world (giữ nguyên
    // Inventory đóng) không có cách nào khác để InventoryTabHandler biết mà chặn Tab. Cờ static này (giống
    // pattern HideSpot.AnyPlayerHiding có sẵn) phủ ĐỦ CẢ 2 đường, không phân biệt nguồn gốc.
    public static bool AnyExamining { get; private set; }

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

        // Chuột trái để xoay -- dùng camera của SÂN KHẤU riêng (ExamineStageUI) làm trục tham chiếu xoay,
        // KHÔNG phải Camera.main nữa vì vật giờ không còn đứng trước mặt player trong thế giới game.
        if (Input.GetMouseButton(0))
        {
            Camera cam = ExamineStageUI.Instance != null ? ExamineStageUI.Instance.StageCamera : Camera.main;
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
        AnyExamining = true;
        _enterFrame  = Time.frameCount;

        _originalParent = transform.parent;
        _originalPos    = transform.position;
        _originalRot    = transform.rotation;
        _originalScale  = transform.localScale;

        // SỬA 2026-07-26: Dời hẳn vật ra sân khấu riêng (ExamineStageUI) thay vì đứng trước Camera.main
        // trong chính thế giới game -- trước đây vẫn ăn nguyên ánh sáng thật (rất tối khi tắt đèn pin),
        // không đọc được chữ trên chìa khoá/giấy/sổ ghi nợ. Sân khấu riêng LUÔN đủ sáng, không phụ thuộc
        // đèn pin/bóng tối/giờ giấc trong game, hiện qua UI Canvas riêng (không phải không gian world nữa).
        var stage = ExamineStageUI.GetOrCreate();
        transform.SetParent(stage.StagePivot, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(_examineRotationOffset);

        // CHỈ set title ở đây khi soi TRỰC TIẾP ngoài world -- soi từ Inventory thì InventoryUI.OnItemClicked
        // đã gọi SetItemTitle() với đúng ItemData.itemName TRƯỚC khi tới đây rồi, gọi đè lên bằng _itemName
        // (thường để trống cho item dạng proxy) sẽ xoá mất tên đúng đã set.
        if (!_openedFromInventory)
            stage.SetItemTitle(_itemName);

        // REVERT 2026-07-26: pauseGame tạm tắt hẳn (false) -- bật lên gây regression thật (Tab phải bấm 3
        // lần, HUD/Inventory hiển thị sai) ở phía InventoryUI, nghi cùng nguyên nhân. Giữ lại tham số cho
        // tương lai điều tra kỹ hơn, nhưng KHÔNG dùng lúc này.
        string exitHint = _openedFromInventory ? "Chuột phải" : "E";
        stage.Show(pauseGame: false, exitHint: exitHint);

        if (!_openedFromInventory && _playerController != null)
            _playerController.SetInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        OnExamineStart.Invoke();
    }

    public void StopExamine()
    {
        if (!_isExamining) return;
        _isExamining = false;
        AnyExamining = false;
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

        // restoreHud: false khi về Inventory -- Inventory vẫn đang mở, tự ẩn Stamina/Flashlight của chính
        // nó, Hide() ở đây bật lại thì lộ ngay 2 UI đó trong lúc Inventory còn hiện.
        ExamineStageUI.Instance?.Hide(pauseGame: false, restoreHud: !wasFromInventory); // REVERT pauseGame -- xem ghi chú trong StartExamine()

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

        ExamineStageUI.Instance?.Hide(pauseGame: false, restoreHud: !_openedFromInventory); // REVERT pauseGame -- xem ghi chú trong StartExamine()

        _isExamining         = false;
        AnyExamining         = false;
        _openedFromInventory = false;
    }
}