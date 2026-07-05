using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ExamineItem — xem vật phẩm 3D trước mặt camera.
///
/// CÓ HAI CHẾ ĐỘ:
///   1. Trực tiếp từ scene  : player đứng gần, nhấn E → StartExamine()
///      → StopExamine() tự unlock player, lock cursor, ẩn object.
///
///   2. Từ Inventory (Tab)  : InventoryUI gọi StartExamineFromInventory()
///      → Renderer được bật lên để hiển thị (dù PickupItem đã tắt chúng)
///      → StopExamine() tắt lại Renderer, fire OnExamineEnd
///      → InventoryUI.ReopenAfterExamine() mở lại túi đồ.
///
/// SETUP TRONG UNITY:
///   • Proxy riêng (ExamineItem trên GameObject khác với PickupItem):
///       - Proxy object nên luôn có Renderer bật sẵn.
///       - InventoryUI._examineRegistry: itemId → ExamineItem proxy.
///   • Cùng GameObject với PickupItem:
///       - Sau khi nhặt, PickupItem tắt Renderer+Collider (không SetActive false).
///       - StartExamineFromInventory() tự bật Renderer → xem được.
///       - StopExamine() tắt lại Renderer → sạch.
/// </summary>
public class ExamineItem : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private float _examineDistance = 0.6f;
    [Tooltip("Toc do xoay — gia tri hop ly: 3~6")]
    [SerializeField] private float _rotateSpeed = 4f;
    [Tooltip("Rotation offset khi item hien ra truoc camera.\n" +
             "Chinh o day neu mesh bi quay sai huong (thay canh mong thay vi mat chinh).\n" +
             "Vi du SheetMusic nam ngang → X = 90. MusicBox nhin nghieng → Y = 180.")]
    [SerializeField] private Vector3 _examineRotationOffset = Vector3.zero;

    [Header("References")]
    [SerializeField] private PlayerController _playerController;

    [Header("Events")]
    public UnityEvent OnExamineStart = new UnityEvent();
    public UnityEvent OnExamineEnd   = new UnityEvent();

    private bool       _isExamining          = false;
    private bool       _disableIsIntentional = false;
    private int        _enterFrame           = -1;
    private Vector3    _originalPos;
    private Quaternion _originalRot;
    private Transform  _originalParent;

    private bool _openedFromInventory    = false;

    // Lưu trạng thái Renderer TRƯỚC KHI examine để restore đúng sau khi xong
    private Renderer[] _renderers;
    private bool[]     _rendererStates;

    public bool IsExamining => _isExamining;

    /// <summary>
    /// IInteractable — cho phép InteractionSystem (raycast E) gọi trực tiếp khi player
    /// đứng gần và nhìn vào item. Trước đây ExamineItem không implement interface này
    /// nên KHÔNG BAO GIỜ được raycast tìm thấy — đây là lý do "soi 3D" không hoạt động
    /// khi tương tác trực tiếp từ scene dù code StartExamine() đã đúng.
    /// Nếu GameObject này còn có PickupItem enabled (item nhặt-rồi-soi-sau), PickupItem
    /// vẫn được ưu tiên xử lý trước theo thứ tự component — Interact() ở đây chỉ có tác
    /// dụng cho item soi-tại-chỗ (không nhặt được), đúng use case "Proxy riêng" mô tả ở trên.
    /// </summary>
    public void Interact() => StartExamine();

    // ─── UPDATE ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!_isExamining) return;

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

        if (Input.GetKeyDown(KeyCode.E) && Time.frameCount != _enterFrame)
            StopExamine();
    }

    // ─── PUBLIC API ────────────────────────────────────────────────────────────

    /// <summary>Gọi từ InventoryUI khi click item trong túi đồ.</summary>
    public void StartExamineFromInventory()
    {
        _openedFromInventory = true;
        StartExamine();
    }

    /// <summary>Gọi trực tiếp từ scene (InteractionSystem / E key).</summary>
    public void StartExamine()
    {
        if (_isExamining) return;

        // ── Lưu và bật Renderer ───────────────────────────────────────────────
        // Dù PickupItem đã tắt Renderer, ta bật lại để xem được.
        // Lưu trạng thái cũ để restore đúng sau khi xem xong.
        _renderers      = GetComponentsInChildren<Renderer>(includeInactive: true);
        _rendererStates = new bool[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _rendererStates[i]    = _renderers[i].enabled;
            _renderers[i].enabled = true;   // BẬT để thấy item khi xem
        }

        // Đảm bảo GameObject active
        _disableIsIntentional = false;
        gameObject.SetActive(true);

        _isExamining = true;
        _enterFrame  = Time.frameCount;

        // ── Lưu transform gốc ────────────────────────────────────────────────
        _originalParent = transform.parent;
        if (_originalParent != null)
            transform.SetParent(null, worldPositionStays: true);

        _originalPos = transform.position;
        _originalRot = transform.rotation;

        // ── Đặt item trước mặt camera ─────────────────────────────────────────
        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.SetParent(cam.transform);
            transform.localPosition = new Vector3(0f, -0.05f, _examineDistance);
            // Áp dụng rotation offset — chỉnh _examineRotationOffset trong Inspector
            // nếu mesh bị quay sai (thấy cạnh mỏng thay vì mặt chính của item).
            // SheetMusic thường cần X=90 vì mesh nằm ngang. MusicBox thường Y=180.
            transform.localRotation = Quaternion.Euler(_examineRotationOffset);
        }
        else
        {
            Debug.LogWarning("[ExamineItem] Camera.main là null! Kiểm tra tag 'MainCamera' trên camera.");
        }

        // Từ inventory → player đã bị lock bởi InventoryUI, không lock lại
        if (!_openedFromInventory && _playerController != null)
            _playerController.SetInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        Debug.Log($"[ExamineItem] Bắt đầu xem: {gameObject.name} | fromInventory={_openedFromInventory}");
        OnExamineStart.Invoke();
    }

    public void StopExamine()
    {
        if (!_isExamining) return;
        _isExamining = false;

        // ── Trả về parent và vị trí ban đầu ──────────────────────────────────
        transform.SetParent(_originalParent, worldPositionStays: false);
        transform.position = _originalPos;
        transform.rotation = _originalRot;

        // ── Restore Renderer về trạng thái TRƯỚC KHI examine ─────────────────
        if (_renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = _rendererStates[i];
            }
        }
        _renderers      = null;
        _rendererStates = null;

        bool wasFromInventory  = _openedFromInventory;
        _openedFromInventory   = false;

        Debug.Log($"[ExamineItem] Dừng xem: {gameObject.name} | fromInventory={wasFromInventory}");

        if (!wasFromInventory)
        {
            // ── Mở trực tiếp từ scene ──────────────────────────────────────
            if (_playerController != null)
                _playerController.SetInputEnabled(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;

            _disableIsIntentional = true;
            gameObject.SetActive(false);

            OnExamineEnd.Invoke();
        }
        else
        {
            // ── Mở từ Inventory ───────────────────────────────────────────
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            // Fire event → ReopenAfterExamine() mở lại inventory panel
            OnExamineEnd.Invoke();
        }
    }

    // ─── EMERGENCY DISABLE ─────────────────────────────────────────────────────
    private void OnDisable()
    {
        if (_disableIsIntentional)
        {
            _disableIsIntentional = false;
            return;
        }

        if (!_isExamining) return;

        // Bị disable từ bên ngoài trong khi đang xem → dọn dẹp khẩn cấp
        // Restore Renderer
        if (_renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null)
                    _renderers[i].enabled = _rendererStates[i];
        }
        _renderers      = null;
        _rendererStates = null;

        transform.SetParent(_originalParent, worldPositionStays: false);
        transform.position = _originalPos;
        transform.rotation = _originalRot;

        if (!_openedFromInventory && _playerController != null)
            _playerController.SetInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        _isExamining         = false;
        _openedFromInventory = false;
    }
}