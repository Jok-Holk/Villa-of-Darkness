using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Gắn lên object nhặt được ngoài scene.
/// Nhấn E → thêm vào Inventory, tắt MeshRenderer + Collider (KHÔNG SetActive false).
/// Như vậy ExamineItem (nếu trên cùng GameObject) vẫn có thể được bật bởi
/// StartExamineFromInventory() khi cần xem từ túi đồ.
///
/// NẾU ExamineItem là proxy RIÊNG (khác GameObject):
///   → vẫn hoạt động bình thường, không ảnh hưởng.
///
/// NẾU ExamineItem nằm TRÊN CÙNG GameObject này:
///   → Sau khi nhặt, object bị ẩn bằng cách tắt Renderer + Collider.
///   → ExamineItem.StartExamineFromInventory() sẽ SetActive(true) + tắt Renderer
///      trong lúc xem → sau khi xem xong trả lại trạng thái ẩn đúng.
///
/// MỚI — BẮT BUỘC EXAMINE TRƯỚC KHI NHẶT:
///   Một số item (VD: cần soi kỹ mới biết có nên nhặt không, hoặc lore item)
///   yêu cầu player phải StartExamine() trước, rồi mới bấm phím riêng (_pickupKey,
///   mặc định F) trong lúc đang examine để thực sự nhặt.
///   Bật bằng cách tick _requireExamineFirst = true và kéo ExamineItem cùng
///   GameObject vào _examineItem. Phía ExamineItem cũng cần kéo ngược lại
///   PickupItem này vào _linkedPickupItem của nó.
/// </summary>
public class PickupItem : MonoBehaviour, IInteractable, IInteractableLabel
{
    [Header("Item")]
    [SerializeField] private ItemData _itemData;

    [Header("Nhãn hiện lên UI tương tác (VD \"Mảnh giấy\", \"Chìa khoá\"...)")]
    [SerializeField] private string _interactLabel = "Vật phẩm";

    [Header("References")]
    [SerializeField] private InventorySystem _inventorySystem;

    [Header("Audio")]
    [SerializeField] private AudioClip _pickupSFX;

    [Header("Events")]
    public UnityEvent OnPickedUp = new UnityEvent();

    [Header("Bắt buộc Examine trước khi nhặt (MỚI)")]
    [Tooltip("Nếu bật: Interact() sẽ KHÔNG nhặt ngay, mà chuyển qua ExamineItem.StartExamine().\n" +
             "Player phải bấm phím Pickup (mặc định F) trong lúc đang examine để nhặt thật sự.")]
    [SerializeField] private bool _requireExamineFirst = false;

    [Tooltip("ExamineItem trên CÙNG GameObject này — dùng khi _requireExamineFirst = true.")]
    [SerializeField] private ExamineItem _examineItem;

    [Header("Suy nghĩ TRƯỚC khi mở Examine (tuỳ chọn, chỉ áp dụng khi _requireExamineFirst = true)")]
    [Tooltip("Để trống thì mở Examine ngay như cũ. Có gán thì chạy xong lời thoại/suy nghĩ này mới tự mở Examine liền sau, không cần bấm E lại.")]
    [SerializeField] private DialogueAsset _preExamineThought;

    private bool _hasBeenPickedUp = false;
    public bool HasBeenPickedUp => _hasBeenPickedUp;
    public ItemData Data => _itemData;
    public string InteractLabel => _interactLabel;

    private void Reset()
    {
        if (_examineItem == null) _examineItem = GetComponent<ExamineItem>();
    }

    private void Start()
    {
        if (_inventorySystem == null) _inventorySystem = InventorySystem.Instance;
    }

    // ─── IInteractable ─────────────────────────────────────────────────────
    public void Interact()
    {
        if (_hasBeenPickedUp) return;

        if (_itemData == null)
        {
            Debug.LogWarning($"[PickupItem] {gameObject.name} chưa gán _itemData!");
            return;
        }
        if (_inventorySystem == null)
        {
            Debug.LogWarning($"[PickupItem] {gameObject.name} chưa gán _inventorySystem!");
            return;
        }

        // ── MỚI: nếu bắt buộc examine trước, KHÔNG nhặt ngay ────────────────
        if (_requireExamineFirst)
        {
            if (_examineItem == null)
            {
                Debug.LogWarning($"[PickupItem] {gameObject.name} bật _requireExamineFirst " +
                                  "nhưng chưa gán _examineItem! Nhặt thẳng luôn để không kẹt player.");
                DoPickup();
                return;
            }

            if (_preExamineThought != null && DialogueUI.Instance != null)
            {
                StartCoroutine(RunPreExamineThoughtThenExamine());
                return;
            }

            Debug.Log($"[PickupItem] {gameObject.name} — cần Examine trước. Đang mở xem...");
            _examineItem.StartExamine();
            return;
        }

        DoPickup();
    }

    // Chạy 1 câu suy nghĩ ngắn (Popup, không giọng) TRƯỚC, đợi đóng hẳn rồi mới mở Examine 360° -- đúng
    // yêu cầu "lồng suy nghĩ nhân vật trước khi soi xét". DialogueUI đã tự khoá input trong lúc thoại chạy,
    // không cần khoá thêm gì ở đây.
    private IEnumerator RunPreExamineThoughtThenExamine()
    {
        DialogueUI.Instance.StartDialogue(_preExamineThought);
        while (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen())
            yield return null;

        _examineItem.StartExamine();
    }

    /// <summary>
    /// Logic nhặt thực sự — tách riêng (public) để ExamineItem có thể gọi lại
    /// khi player bấm phím Pickup trong lúc đang examine (case _requireExamineFirst).
    /// </summary>
    public void DoPickup()
    {
        if (_hasBeenPickedUp) return;
        if (_itemData == null || _inventorySystem == null) return;

        _hasBeenPickedUp = true;
        _inventorySystem.AddItem(_itemData.itemId);

        if (_pickupSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(_pickupSFX);

        Debug.Log($"[PickupItem] Đã nhặt: {_itemData.itemId}");
        OnPickedUp.Invoke();

        // ── Ẩn visual + collider, KHÔNG tắt GameObject ──────────────────────
        // → ExamineItem vẫn có thể được gọi bởi InventoryUI.
        // → Object không còn "thấy được" và không thể nhặt lại (Collider tắt).
        // → KHÔNG BAO GIỜ Destroy() — đúng yêu cầu "cầm nó lên, đừng destroy nó".
        HideInScene();
    }

    /// <summary>Ẩn object trong scene mà không dùng SetActive(false) và không Destroy.</summary>
    private void HideInScene()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Tắt chính component này → InteractionSystem sẽ bỏ qua (FindEnabledInteractable)
        this.enabled = false;
    }

    /// <summary>
    /// Bật lại visual + collider nếu cần reset (respawn scene, v.v.).
    /// </summary>
    public void ResetPickup()
    {
        _hasBeenPickedUp = false;
        this.enabled     = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = true;
    }

    /// <summary>
    /// Gọi lúc scene vừa load xong (InventorySystem.Start()) cho item MÀ NGƯỜI CHƠI ĐÃ CÓ SẴN trong
    /// GameData.collectedItems TỪ TRƯỚC (VD Retry/reload) -- chỉ ẩn object trong world cho khớp trạng thái
    /// đã nhặt, KHÔNG gọi lại AddItem/SFX/OnPickedUp (item vốn đã có trong túi rồi, không phải nhặt mới).
    /// Không có cơ chế này thì mỗi lần scene reload, object nhặt được sẽ "hồi sinh" hiện lại trong map dù
    /// vẫn đang nằm trong túi đồ -- lộ ra 2 bản cùng lúc (1 trong túi, 1 vẫn đứng ngoài world).
    /// </summary>
    public void SyncAlreadyPickedUp()
    {
        if (_hasBeenPickedUp) return;
        _hasBeenPickedUp = true;
        HideInScene();
    }
}