using UnityEngine;

// Gắn script này vào BẤT KỲ GameObject nào (có thể gắn ngay vào chính cái Button,
// hoặc vào 1 GameObject rỗng riêng tên "DialogueTriggers" nếu muốn nhiều dialogue khác nhau).
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Kéo object đang chứa script DialogueUI vào đây (vd: DialoguePanel)")]
    public DialogueUI dialogueUI;

    [Tooltip("Kéo cái DialogueAsset bạn vừa tạo vào đây")]
    public DialogueAsset dialogueAsset;

    // Hàm này KHÔNG có tham số -> Button.OnClick() gọi được trực tiếp.
    public void PlayDialogue()
    {
        if (dialogueUI == null || dialogueAsset == null)
        {
            Debug.LogWarning("DialogueTrigger: chưa gán đủ dialogueUI hoặc dialogueAsset trong Inspector.");
            return;
        }

        dialogueUI.StartDialogue(dialogueAsset);
    }
}