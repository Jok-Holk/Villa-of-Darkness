using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Kéo object đang chứa script DialogueUI vào đây (vd: DialoguePanel)")]
    public DialogueUI dialogueUI;

    [Tooltip("Kéo cái DialogueAsset bạn vừa tạo vào đây")]
    public DialogueAsset dialogueAsset;

    [Tooltip("Tick lên = chỉ phát đúng 1 lần trong 1 lượt chơi (vd lời thoại intro) — tương tác lại lần sau sẽ không phát lại nữa. Runtime-only, mất khi restart scene.")]
    public bool playOnce = false;
    private bool _hasPlayed = false;

    // KHÔNG lắng nghe Space ở đây nữa — DialogueUI.Update() đã tự xử lý advance/skip/choice
    // cho hội thoại ĐANG MỞ. Nếu mỗi DialogueTrigger trong scene đều tự lắng nghe Space toàn cục,
    // bấm Space ở bất kỳ đâu có thể vô tình StartDialogue() một asset không liên quan tới vị trí
    // player đang đứng — đây là nguyên nhân gây "spam" hội thoại ngẫu nhiên.
    // PlayDialogue() dưới đây chỉ nên được gọi từ bên ngoài (TriggerZone.OnTriggered, InteractionSystem, nút UI).

    // Hàm kích hoạt hội thoại ban đầu (Vẫn giữ nguyên để nút Button click bằng chuột gọi được)
    public void PlayDialogue()
    {
        if (playOnce && _hasPlayed) return;

        if (dialogueUI == null || dialogueAsset == null)
        {
            Debug.LogWarning("DialogueTrigger: THẤT BẠI! Chưa gán đủ dialogueUI hoặc dialogueAsset trong Inspector.");
            return;
        }

        _hasPlayed = true;
        dialogueUI.AdvanceOrStartDialogue(dialogueAsset);
    }
}