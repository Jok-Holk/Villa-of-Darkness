// Interface PHỤ, KHÔNG bắt buộc -- IInteractable hiện có 10+ script implement (HideSpot,
// PickupItem, ExamineItem, ItemLock, PianoInteractable...), thêm field vào IInteractable gốc sẽ
// buộc sửa hết từng file đó. Script nào muốn hiện thêm TÊN vật bên cạnh phím tương tác (VD "[E] Cửa")
// thì implement thêm interface này; script nào không cần thì thôi, InteractionSystem tự kiểm tra
// bằng "as IInteractableLabel", không có thì chỉ hiện phím không kèm tên (giữ nguyên hành vi cũ).
public interface IInteractableLabel
{
    string InteractLabel { get; }
}
