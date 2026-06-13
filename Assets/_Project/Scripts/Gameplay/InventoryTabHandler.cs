using UnityEngine;

/// <summary>
/// Gắn script này lên Player (hoặc bất kỳ GameObject nào luôn active).
/// Lý do tách riêng: InventoryUI.gameObject bị SetActive(false) khi đóng
/// nên Update() của nó không chạy được → không bắt được input Tab.
/// Script này luôn active, chỉ làm 1 việc: nhấn Tab → gọi InventoryUI.Toggle().
/// </summary>
public class InventoryTabHandler : MonoBehaviour
{
    [SerializeField] private InventoryUI _inventoryUI;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;
        if (_inventoryUI == null) return;

        // BUG FIX 5: Nếu inventory đang ẩn nhưng examine đang chạy (trạng thái
        // hợp lệ khi click item từ inventory), Tab phải đóng examine + inventory
        // thay vì gọi Toggle() → Toggle sẽ thấy _isOpen=true → Close() đúng rồi.
        // Nhưng nếu _isOpen=false vì inventory bị ẩn khi examine → Toggle() sẽ Open()
        // thay vì Close(). Fix: dùng IsOpen || IsExamining.
        if (_inventoryUI.IsOpen || _inventoryUI.IsExamining)
            _inventoryUI.Close();
        else
            _inventoryUI.Open();
    }
}