using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Kéo object đang chứa script DialogueUI vào đây (vd: DialoguePanel)")]
    public DialogueUI dialogueUI;

    [Tooltip("Kéo cái DialogueAsset bạn vừa tạo vào đây")]
    public DialogueAsset dialogueAsset;

    // Hàm Update kiểm tra phím bấm mỗi khung hình
    void Update()
    {
        // Kiểm tra nếu người chơi nhấn phím Space xuống
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueUI == null) return;

            dialogueUI.AdvanceOrStartDialogue(dialogueAsset);
        }
    }

    // Hàm kích hoạt hội thoại ban đầu (Vẫn giữ nguyên để nút Button click bằng chuột gọi được)
    public void PlayDialogue()
    {
        if (dialogueUI == null || dialogueAsset == null)
        {
            Debug.LogWarning("DialogueTrigger: THẤT BẠI! Chưa gán đủ dialogueUI hoặc dialogueAsset trong Inspector.");
            return;
        }

        dialogueUI.AdvanceOrStartDialogue(dialogueAsset);
    }
}