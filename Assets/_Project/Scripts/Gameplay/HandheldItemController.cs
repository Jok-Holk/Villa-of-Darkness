using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gắn lên Player (cạnh FlashlightController, PlayerController...).
/// Quản lý model item đang được "cầm" ở tay trái sau khi player bấm nút
/// "Sử dụng" trong Inventory (InventoryUI.OnUseClicked).
///
/// QUAN TRỌNG — KHÔNG BAO GIỜ DESTROY ITEM GỐC:
///   Model gắn lên tay LUÔN LÀ BẢN INSTANTIATE (bản sao) từ ItemData.handHeldPrefab.
///   Item gốc trong túi đồ (GameData.collectedItems) và object gốc ngoài scene
///   (nếu có) hoàn toàn không bị đụng tới.
///
/// POOL: Mỗi ItemData chỉ Instantiate 1 lần — đổi/bỏ item trên tay chỉ ẩn/hiện
/// lại bản đã tạo (SetActive), không Instantiate/Destroy lặp lại mỗi lần bấm Use.
/// </summary>
public class HandheldItemController : MonoBehaviour
{
    public static HandheldItemController Instance { get; private set; }

    [Header("Socket tay trái — kéo Transform trên rig/camera vào đây")]
    [SerializeField] private Transform _leftHandSocket;

    [Header("Đang cầm gì (chỉ để debug/hiển thị)")]
    [SerializeField] private string _currentItemId;

    private readonly Dictionary<ItemData, GameObject> _pool = new Dictionary<ItemData, GameObject>();
    private GameObject _currentInstance;
    private ItemData   _currentData;

    public bool   IsHoldingSomething => _currentInstance != null;
    public string CurrentItemId      => _currentItemId;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

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

        // Đổi item khác → ẩn bản cũ trên tay (không Destroy, giữ lại trong pool để dùng lại)
        Unequip();

        if (!_pool.TryGetValue(data, out GameObject instance) || instance == null)
        {
            instance = Instantiate(data.handHeldPrefab, _leftHandSocket);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            _pool[data] = instance;
        }
        else
        {
            instance.SetActive(true);
        }

        _currentInstance = instance;
        _currentData     = data;
        _currentItemId   = data.itemId;

        Debug.Log($"[HandheldItemController] Đã cầm lên tay trái: {data.itemId}");
    }

    /// <summary>Cất item đang cầm khỏi tay (KHÔNG xoá khỏi túi đồ, KHÔNG Destroy — chỉ ẩn để dùng lại).</summary>
    public void Unequip()
    {
        if (_currentInstance != null)
            _currentInstance.SetActive(false);

        _currentInstance = null;
        _currentData     = null;
        _currentItemId   = string.Empty;
    }
}