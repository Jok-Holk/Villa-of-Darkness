using UnityEngine;

// Gắn lên cửa (hoặc bất kỳ object nào) bị mở khoá TỪ XA khi giải xong 1 puzzle -- KHÔNG tự mở cửa ngay,
// chỉ phát tiếng mở khoá bất ngờ (nghe từ xa, không thấy cửa) + 1 suy nghĩ ngắn phản ứng lại tiếng đó.
// Cửa vẫn cần Player tự đi tới bấm E mới thực sự mở ra (DoorController.Interact() xử lý bình thường, vì
// lúc này _isLocked đã là false rồi -- không cần chìa, không cần gọi Open() ở đây).
public class PianoCompletionReaction : MonoBehaviour
{
    [SerializeField] private AudioClip _unlockSfx;
    [SerializeField] private DialogueAsset _reactionThought;

    public void OnRemoteUnlock()
    {
        if (_unlockSfx != null) AudioManager.Instance?.PlaySFX(_unlockSfx);
        if (_reactionThought != null) DialogueUI.Instance?.StartDialogue(_reactionThought);
    }
}
