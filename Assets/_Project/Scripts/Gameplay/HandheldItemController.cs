using UnityEngine;

/// <summary>
/// Gắn lên Player (cạnh FlashlightController, PlayerController...).
/// Quản lý model item đang được "cầm" ở tay trái sau khi player bấm nút
/// "Sử dụng" trong Inventory (InventoryUI.OnUseClicked).
///
/// QUAN TRỌNG — KHÔNG BAO GIỜ DESTROY ITEM GỐC:
///   Model gắn lên tay LUÔN LÀ BẢN INSTANTIATE (bản sao) từ ItemData.handHeldPrefab.
///   Item gốc trong túi đồ (GameData.collectedItems) và object gốc ngoài scene
///   (nếu có) hoàn toàn không bị đụng tới. Đổi/bỏ item trên tay chỉ Destroy
///   CHÍNH BẢN SAO ĐANG CẦM, không ảnh hưởng tới dữ liệu inventory.
/// </summary>
public class HandheldItemController : MonoBehaviour
{
    [Header("Socket tay trái — kéo Transform trên rig/camera vào đây")]
    [SerializeField] private Transform _leftHandSocket;

    [Header("Đang cầm gì (chỉ để debug/hiển thị)")]
    [SerializeField] private string _currentItemId;

    private GameObject _currentInstance;
    private ItemData   _currentData;

    public bool   IsHoldingSomething => _currentInstance != null;
    public string CurrentItemId      => _currentItemId;

    /// <summary>Gọi khi player bấm "Sử dụng" trên 1 item trong Inventory.</summary>
    public void Equip(ItemData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[HandheldItemController] Equip() nhận ItemData null.");
            return;
        }
        if (!data.isUsable)
        {
            Debug.Log($"[HandheldItemController] {data.itemId} không phải item dùng được (isUsable=false).");
            return;
        }
        if (data.handHeldPrefab == null)
        {
            Debug.LogWarning($"[HandheldItemController] {data.itemId} chưa gán handHeldPrefab!");
            return;
        }
        if (_leftHandSocket == null)
        {
            Debug.LogWarning("[HandheldItemController] Chưa gán _leftHandSocket!");
            return;
        }

        // Đang cầm đúng item này rồi → toggle bỏ xuống (bấm Use lần nữa để cất đi)
        if (_currentData == data)
        {
            Unequip();
            return;
        }

        // Đổi item khác → dọn bản cũ trên tay (chỉ Destroy bản sao, KHÔNG đụng item gốc)
        Unequip();

        _currentInstance = Instantiate(data.handHeldPrefab, _leftHandSocket);
        _currentInstance.transform.localPosition = Vector3.zero;
        _currentInstance.transform.localRotation = Quaternion.identity;

        _currentData     = data;
        _currentItemId   = data.itemId;

        Debug.Log($"[HandheldItemController] Đã cầm lên tay trái: {data.itemId}");
    }

    /// <summary>Cất item đang cầm khỏi tay (KHÔNG xoá khỏi túi đồ).</summary>
    public void Unequip()
    {
        if (_currentInstance != null)
            Destroy(_currentInstance); // chỉ destroy bản sao đang cầm, item gốc trong túi vẫn còn

        _currentInstance = null;
        _currentData     = null;
        _currentItemId   = string.Empty;
    }
}