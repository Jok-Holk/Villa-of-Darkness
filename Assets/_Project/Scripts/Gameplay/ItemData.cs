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
}