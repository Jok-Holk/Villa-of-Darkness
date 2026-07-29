using UnityEngine;
using UnityEngine.Events;

// Component tương tác DÙNG CHUNG, gắn thẳng lên bất kỳ vật thể nào (cửa, công tắc, vật trang trí
// có thoại...) -- cấu hình TÊN hiển thị + thoại/hành động ngay tại đây trong Inspector, không cần
// viết script riêng cho từng loại vật. Dùng Collider có sẵn trên chính GameObject này (không tự
// thêm collider mới) -- raycast của InteractionSystem tự bắt trúng như mọi IInteractable khác.
//
// VD gắn lên "Cửa": Interact Label = "Cửa" -> hiện "[E] Cửa" khi ngắm trúng. Gán Dialogue On
// Interact nếu muốn bật thoại lúc tương tác, hoặc để trống + dùng On Interact (UnityEvent) cho
// hành động khác (mở cửa, bật đèn...) -- dùng được cả 2 cùng lúc.
[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour, IInteractable, IInteractableLabel
{
    [Tooltip("Tên hiển thị cạnh phím tương tác, VD \"Cửa\" -> hiện \"[E] Cửa\"")]
    [SerializeField] private string interactLabel = "Vật thể";

    [Tooltip("Thoại bật lên khi tương tác -- để trống nếu không cần thoại, chỉ dùng On Interact bên dưới")]
    [SerializeField] private DialogueAsset dialogueOnInteract;

    [Tooltip("Chỉ tương tác được 1 lần duy nhất (VD: đọc xong ghi chú thì thôi) -- tắt nếu muốn tương tác lại được nhiều lần")]
    [SerializeField] private bool interactOnce = false;
    private bool _hasInteracted = false;

    [Tooltip("Hành động thêm khi tương tác (mở cửa, bật đèn...) -- chạy cùng lúc với dialogueOnInteract nếu có cả 2")]
    public UnityEvent OnInteract;

    public string InteractLabel => interactLabel;

    public void Interact()
    {
        if (interactOnce && _hasInteracted) return;
        _hasInteracted = true;

        if (dialogueOnInteract != null)
            DialogueUI.Instance?.StartDialogue(dialogueOnInteract);

        OnInteract?.Invoke();
    }
}
