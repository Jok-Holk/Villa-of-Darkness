using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin 1 vật phẩm trong túi đồ.
/// Tạo: Assets → chuột phải → Create → Inventory → Item Data
/// Mỗi vật phẩm tạo 1 asset riêng, đặt vào Assets/_Project/ScriptableObjects/Items/
/// </summary>
[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    [Tooltip("ID duy nhất — dùng để AddItem/HasItem/RemoveItem, không được trùng")]
    public string itemId;

    [Tooltip("Tên hiển thị trong túi đồ")]
    public string itemName;

    [Tooltip("Mô tả ngắn khi click vào item")]
    [TextArea(2, 4)]
    public string description;

    [Header("Hiển thị")]
    [Tooltip("Icon hiển thị trong slot túi đồ")]
    public Sprite icon;

    [Tooltip("Màu viền slot — vàng cho di vật quan trọng, trắng bình thường")]
    public Color slotBorderColor = Color.white;

    [Header("Audio")]
    [Tooltip("Clip phát khi click vào item (monologue nhân vật)")]
    public AudioClip monologueClip;

    [Header("Thuộc tính")]
    [Tooltip("Di vật quan trọng — không thể bỏ khỏi túi")]
    public bool isKeyItem = false;

    // ── MỚI: Sử dụng / cầm tay ──────────────────────────────────────────────
    [Header("Sử dụng — Cầm lên tay trái (MỚI)")]
    [Tooltip("Item này có dùng được không (bấm nút 'Sử dụng' trong Inventory).\n" +
             "Ví dụ: đèn cầy, chìa khoá, gương... KHÔNG áp dụng cho giấy tờ chỉ để đọc.")]
    public bool isUsable = false;

    [Tooltip("Prefab sẽ được Instantiate và gắn vào tay trái player khi bấm 'Sử dụng'.\n" +
             "Đây LUÔN LÀ BẢN SAO (Instantiate) — KHÔNG đụng tới item gốc trong túi/scene, " +
             "nên item KHÔNG BAO GIỜ bị mất/destroy khi Use.")]
    public GameObject handHeldPrefab;

    [Header("Examine trong túi đồ (MỚI)")]
    [Tooltip("Item này có thể/cần xem 3D (xoay để đọc mật khẩu, xem chi tiết...) khi click trong Inventory.\n" +
             "Nếu true, nhớ đăng ký entry tương ứng trong InventoryUI._examineRegistry.")]
    public bool isExaminable = false;
}