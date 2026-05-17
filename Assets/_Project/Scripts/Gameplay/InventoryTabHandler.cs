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
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_inventoryUI != null)
                _inventoryUI.Toggle();
        }
    }
}
