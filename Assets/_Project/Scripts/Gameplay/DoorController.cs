using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable, IInteractableLabel
{
    [Header("Nhãn hiện trên UI tương tác ([E] + tên) -- mọi cửa đều phải có, giống cửa chính")]
    [SerializeField] private string _interactLabel = "Cửa";
    public string InteractLabel => _interactLabel;

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

    [Header("Khoá — yêu cầu chìa khoá riêng (để trống itemId = khoá cứng, không có cách mở qua Interact() thường)")]
    [Tooltip("itemId của chìa khoá cần thiết -- để trống thì cửa khoá không có cách mở qua tương tác thường (VD chờ ItemLock/trigger khác tự SetLocked(false))")]
    [SerializeField] private string _requiredItemId;
    [Tooltip("Tiêu hao chìa khỏi túi đồ sau khi mở khoá thành công -- mặc định KHÔNG tiêu hao (chìa nhà chính thường dùng lại được nhiều lần)")]
    [SerializeField] private bool _consumeKeyOnUnlock = false;

    [Header("Kẹt cứng (Chapter1 cảnh 3) — CÓ CHÌA ĐÚNG VẪN KHÔNG MỞ ĐƯỢC, chỉ phát SFX kẹt")]
    [Tooltip("Bật lên thì HandleLockedInteract() luôn coi như KHÔNG có chìa (dù đang cầm đúng chìa) -- dùng cho " +
             "case \"cửa phòng ăn ra hành lang sau bị kẹt dù có chìa\" ở cảnh 3. Tắt lại = trở về hành vi khoá/chìa bình thường.")]
    [SerializeField] private bool _forceJammed = false;

    public bool IsForceJammed => _forceJammed;

    /// <summary>Bật/tắt trạng thái kẹt cứng bằng code (VD Chapter1Scene3Manager lúc chuyển cảnh 3).</summary>
    public void SetForceJammed(bool jammed) => _forceJammed = jammed;

    [Header("SFX — cửa có khoá")]
    [Tooltip("Cửa kẹt do khoá -- phát khi bấm E lúc đang khoá mà KHÔNG có chìa trong túi đồ")]
    [SerializeField] private AudioClip _lockedSfx;
    [Tooltip("Mở khoá thành công -- phát khi bấm E lúc đang CẦM ĐÚNG CHÌA trên tay (tự trang bị chìa lên tay là thao tác thủ công qua Inventory, không phải tool này)")]
    [SerializeField] private AudioClip _unlockSfx;
    [Tooltip("Tiếng mở cửa (chậm, theo đúng _animSpeed) -- chỉ phát lúc cửa ĐANG đóng chuyển sang mở, không phát lúc đóng lại")]
    [SerializeField] private AudioClip _openSfx;

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

    // THÊM 2026-07-27: Đăng ký trạng thái khoá theo checkpoint -- trước đây Retry (chết) luôn reset cửa về
    // đúng giá trị mặc định lúc thiết kế scene, kể cả cửa đã mở khoá TỪ TRƯỚC checkpoint gần nhất (vẫn còn
    // chìa trong túi nhưng cửa lại khoá lại, phải mở khoá lại vô lý). Đăng ký ở Awake() (không phải Start())
    // vì CheckpointManager.Restore() được gọi ngay sau khi scene load xong, TRƯỚC Start() của các object --
    // đăng ký ở Start() sẽ trễ mất 1 nhịp, bỏ lỡ lần Restore() đầu tiên.
    //
    // SỬA 2026-07-31 (Jok hỏi "cùng 1 đồ vật, rotation thay đổi thì làm sao Retry tự xoay lại đúng"): trước
    // đây CHỈ theo dõi khoá/mở khoá, CỐ Ý bỏ qua rotation mở/đóng vật lý vì coi là "trạng thái tức thời,
    // scene reload tự về đúng góc đóng mặc định" -- SAI trong trường hợp 1 cửa dùng xuyên suốt nhiều mốc
    // checkpoint (VD Player đã mở ở cảnh 2, Retry ở cảnh 3 mà đóng kín lại là vô lý). Giờ theo dõi luôn
    // _isOpen y hệt _isLocked -- KHÔNG cần code riêng phân biệt "cảnh 2 hay cảnh 3" cho từng cửa nữa, mỗi
    // checkpoint tự chụp đúng trạng thái mở/đóng tại thời điểm lưu, Retry xong tự về đúng y vậy.
    private bool _pendingRestoredOpenState;
    private bool _hasPendingRestoredOpenState;

    private void Awake()
    {
        string id = "Door." + CheckpointManager.GetHierarchyPath(transform);
        CheckpointManager.RegisterFlag(id, () => _isLocked, v => SetLocked(v));

        // Setter CHỈ lưu tạm -- KHÔNG set thẳng _isOpen/_targetRot ở đây, vì CheckpointManager.Restore()
        // chạy TRƯỚC Start() (xem comment gốc trong CheckpointManager.cs), lúc đó _openRot/_closedRot chưa
        // được tính (vẫn là Quaternion.identity mặc định) -- set thẳng ngay bây giờ sẽ ra góc xoay rác. Áp
        // dụng thật sự dời xuống Start(), sau khi đã tính xong baseline.
        CheckpointManager.RegisterFlag(id + ".Open", () => _isOpen,
            v => { _pendingRestoredOpenState = v; _hasPendingRestoredOpenState = true; });
    }

    private void Start()
    {
        _closedRot = transform.localRotation;
        _openRot   = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, _openAngle, 0));
        _ajarRot   = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, _ajarAngle, 0));

        _closedPos = transform.localPosition;
        _openPos   = _closedPos + _slideOffset;

        // Checkpoint vừa restore xong 1 trạng thái mở/đóng khác với mặc định authored trong scene -- ghi đè
        // _isOpen NGAY TẠI ĐÂY (sau khi _openRot/_closedRot đã tính xong đúng).
        if (_hasPendingRestoredOpenState)
        {
            _isOpen = _pendingRestoredOpenState;
            _hasPendingRestoredOpenState = false;
        }

        _targetRot = _isOpen ? _openRot : _closedRot;
        _targetPos = _isOpen ? _openPos : _closedPos;

        // Đặt thẳng luôn transform hiện tại thay vì đợi Update() Lerp dần -- tránh cửa "trôi" từ từ ngay
        // khung hình đầu tiên sau khi scene vừa load xong (đây là lúc restore, không phải Player tương tác
        // thật, không cần hiệu ứng xoay mượt).
        if (_isDrawer) transform.localPosition = _targetPos;
        else transform.localRotation = _targetRot;
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
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        if (_isLocked)
        {
            HandleLockedInteract();
            return;
        }

        if (!_isOpen) PlaySfx(_openSfx); // chỉ phát tiếng lúc THỰC SỰ mở, không phát lúc đóng lại
        Toggle();
    }

    // SỬA 2026-07-26: Trước đây mở khoá + mở cửa LUÔN trong CÙNG 1 lần bấm E -- SFX mở khoá và SFX mở cửa
    // phát chồng lên nhau cùng lúc, nghe rối. Giờ tách hẳn 2 hành động qua 2 LẦN BẤM E RIÊNG:
    //   Lần 1 (đang khoá + cầm đúng chìa) -> chỉ mở KHOÁ (SFX mở khoá), CỬA VẪN ĐÓNG.
    //   Lần 2 (đã hết khoá) -> rơi thẳng vào nhánh Interact() bình thường bên trên -> SFX mở cửa + Toggle().
    // Không có chìa trong túi (hoặc có nhưng CHƯA cầm lên tay -- Jok tự vào Inventory chọn chìa + bấm E để
    // trang bị, KHÔNG tự động trang bị hộ) -> chỉ phát tiếng kẹt, không đổi gì cả.
    private void HandleLockedInteract()
    {
        // Kẹt cứng (cảnh 3) -- LUÔN coi như không có chìa, kể cả đang cầm đúng chìa trên tay.
        bool isHoldingKey = !_forceJammed
                             && !string.IsNullOrEmpty(_requiredItemId)
                             && HandheldItemController.Instance != null
                             && HandheldItemController.Instance.IsHoldingSomething
                             && HandheldItemController.Instance.CurrentItemId == _requiredItemId;

        if (!isHoldingKey)
        {
            PlaySfx(_lockedSfx);
            return;
        }

        PlaySfx(_unlockSfx);

        SetLocked(false);
        if (_consumeKeyOnUnlock) InventorySystem.Instance.RemoveItem(_requiredItemId);
        // KHÔNG PlaySfx(_openSfx)/Toggle() ở đây nữa -- chờ Player bấm E LẦN NỮA mới thực sự mở cửa.
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null) AudioManager.Instance?.PlaySFX(clip);
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