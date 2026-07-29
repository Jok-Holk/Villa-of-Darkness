using UnityEngine;

// Công cụ TEST TẠM (chỉ Editor, tự vô hiệu trong build thật) -- gom TOÀN BỘ ExamineItem có trong scene
// (giấy, chìa khoá, sổ ghi nợ, hộp âm nhạc...) để chỉnh offset xoay 1 LẦN, không cần đi bộ nhặt/tìm từng
// món ngoài map (đỡ tốn thời gian + tránh trúng jumpscare/ma khi đi lại nhiều lần).
//
// Cách dùng: thêm component này vào bất kỳ GameObject nào đang active trong scene (VD Player, hoặc object
// "CheckpointManager" đã có CheckpointDebugTool) -- KHÔNG cần wiring gì thêm, tự tìm hết bằng
// FindObjectsByType. Bấm Play, dùng PageDown/PageUp để chuyển qua từng item, mũi tên xoay + [L] ghi log góc
// (2 phím này đã có sẵn trong ExamineItem.cs), Chuột phải để thoát Examine hiện tại trước khi chuyển tiếp.
public class ExamineOffsetTestHarness : MonoBehaviour
{
#if UNITY_EDITOR
    private ExamineItem[] _items;
    private int _index = -1;
    private ExamineItem _current;

    private void Start()
    {
        _items = Object.FindObjectsByType<ExamineItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[ExamineTest] Tìm thấy {_items.Length} ExamineItem trong scene -- [PageDown]/[PageUp] để nhảy thẳng vào Examine từng món, không cần đi bộ tìm.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageDown)) JumpTo(_index + 1);
        if (Input.GetKeyDown(KeyCode.PageUp))   JumpTo(_index - 1);
    }

    private void JumpTo(int newIndex)
    {
        if (_items == null || _items.Length == 0) return;

        if (_current != null && _current.IsExamining) _current.StopExamine();

        newIndex = ((newIndex % _items.Length) + _items.Length) % _items.Length;
        _index = newIndex;

        var item = _items[_index];
        if (item == null)
        {
            Debug.LogWarning($"[ExamineTest] Item [{_index + 1}/{_items.Length}] đã bị null (object bị xoá?) -- bỏ qua.");
            return;
        }

        _current = item;
        Debug.Log($"[ExamineTest] === [{_index + 1}/{_items.Length}] '{item.gameObject.name}' === Mũi tên: xoay 90 độ | [L]: ghi log góc | Chuột phải: thoát | PageDown/PageUp: chuyển món");

        item.gameObject.SetActive(true);
        item.StartExamineFromInventory(); // nhánh "từ inventory" -- thoát bằng chuột phải, không đụng input world/PickupItem
    }
#endif
}
