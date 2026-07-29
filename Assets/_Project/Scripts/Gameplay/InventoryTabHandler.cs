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

    private void Start()
    {
        if (_inventoryUI == null) _inventoryUI = InventoryUI.Instance;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;
        if (_inventoryUI == null) return;

        // KIẾN TRÚC (Jok yêu cầu): luồng chỉ 1 CHIỀU -- Gameplay -> Tab -> Inventory -> Examine/Diary.
        // Thoát khỏi Examine/Diary CHỈ bằng đúng phím riêng của nó (Chuột phải khi mở từ Inventory, E khi soi
        // trực tiếp ngoài world) -- Tab bị "force đè lên" trong lúc đang ở 2 màn này dễ sinh lỗi (từng phải vá
        // tạm ở BUG FIX 5/6 cũ: đóng nhầm/kẹt lớp). Giờ chặn HẲN Tab (không làm gì cả) trong 2 trạng thái này,
        // thay vì cố xử lý "lùi 1 lớp" -- chỉ 1 đường thoát duy nhất, không còn nhập nhằng.
        if (DiaryReaderUI.Instance != null && DiaryReaderUI.Instance.IsOpen) return;
        if (ExamineItem.AnyExamining) return;

        if (_inventoryUI.IsOpen)
            _inventoryUI.Close();
        else
            _inventoryUI.Open();
    }
}