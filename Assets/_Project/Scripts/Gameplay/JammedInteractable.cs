using UnityEngine;

// Vật/cửa "kẹt cứng" DÙNG CHUNG (không riêng cửa sổ phòng ăn) -- bấm E chỉ phát SFX/thoại báo kẹt, không
// làm gì khác. Dùng cho: cửa sổ phòng ăn cảnh 3 ("cửa sổ kẹt, phải tìm lối khác"), cửa chính lúc cảnh 3
// ("cửa kẹt không mở được" -- dù cảnh 1 đã bảo khoá rồi, giờ đổi hẳn sang thông báo khác)...
// KHÁC DoorController._forceJammed (dùng cho cửa CÓ animation mở/đóng thật) -- cái này cho vật hoàn toàn
// KHÔNG mở được, không animation, chỉ có phản hồi "kẹt" khi bấm E.
public class JammedInteractable : MonoBehaviour, IInteractable, IInteractableLabel
{
    [SerializeField] private string _interactLabel = "Cửa sổ";

    [Tooltip("Phát khi bấm E -- tiếng kẹt/rung nhẹ, không bắt buộc.")]
    [SerializeField] private AudioClip _jammedSfx;

    [Tooltip("Thoại/suy nghĩ ngắn báo kẹt (VD \"Cửa sổ kẹt mất rồi, phải tìm lối khác.\") -- để trống thì chỉ phát SFX.")]
    [SerializeField] private DialogueAsset _jammedDialogue;

    [Tooltip("Chặn spam bấm E liên tục trong lúc thoại đang chạy.")]
    [SerializeField] private bool _blockWhileDialogueOpen = true;

    public string InteractLabel => _interactLabel;

    public void Interact()
    {
        if (_blockWhileDialogueOpen && DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen())
            return;

        if (_jammedSfx != null)
            AudioManager.Instance?.PlaySFX(_jammedSfx);

        if (_jammedDialogue != null)
            DialogueUI.Instance?.StartDialogue(_jammedDialogue);
    }
}
