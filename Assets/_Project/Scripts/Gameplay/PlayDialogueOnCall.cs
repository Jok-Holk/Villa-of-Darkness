using UnityEngine;

// Component nhỏ dùng chung -- wire Play() vào bất kỳ UnityEvent nào (VD HideSpot.OnReveal) để phát 1
// DialogueAsset đúng lúc đó, KHÔNG cần Player đi ngang qua trigger collider nào (khác ThoughtTrigger.cs).
// Dùng khi có 4 HideDoor khác nhau nhưng chỉ cần đúng 1 suy nghĩ chung phát ra dù Player thoát ra từ tủ nào.
public class PlayDialogueOnCall : MonoBehaviour
{
    [SerializeField] private DialogueAsset _dialogue;
    [Tooltip("Chỉ chạy khi ĐANG ở cảnh 3 -- tắt nếu muốn dùng ở cảnh khác.")]
    [SerializeField] private bool _onlyDuringScene3 = true;
    [SerializeField] private bool _playOnce = true;

    private bool _hasPlayed = false;

    /// <summary>Gọi từ UnityEvent (VD HideSpot.OnReveal) -- phát đúng DialogueAsset đã gán.</summary>
    public void Play()
    {
        if (_playOnce && _hasPlayed) return;
        if (_onlyDuringScene3 && !Chapter1Scene3Manager.IsActive) return;
        if (_dialogue == null || DialogueUI.Instance == null) return;
        _hasPlayed = true;
        DialogueUI.Instance.StartDialogue(_dialogue);
    }
}
