using UnityEngine;
using UnityEngine.Events;

// Gắn lên tấm vải phủ (che gương, hoặc bất kỳ đồ vật nào đang bị phủ vải) -- bấm E là XÉ/GỠ VẢI RA NGAY,
// phá huỷ hẳn (Destroy), KHÔNG hiện thoại/suy nghĩ gì cả -- khác PickupItem/ExamineItem có thể có suy nghĩ
// trước khi xem. Gỡ xong, GazeTrigger đặt trên vật phía sau (VD gương) mới raycast trúng được vật thật.
public class TearOffClothCover : MonoBehaviour, IInteractable, IInteractableLabel
{
    [SerializeField] private string _interactLabel = "Tấm vải phủ";
    [SerializeField] private AudioClip _tearSfx;

    [Tooltip("Bắn ra NGAY TRƯỚC khi Destroy -- MirrorJumpscareReaction (hoặc hệ thống tương tự) lắng nghe để bắt đầu đếm grace period trước khi cho phép jumpscare, tránh trường hợp Player đã nhìn vào tấm vải/gương từ trước lúc còn che (GazeTrigger tính luôn cả con là tấm vải) nên vừa gỡ xong là giật mình ngay lập tức.")]
    public UnityEvent OnTornOff;

    public string InteractLabel => _interactLabel;

    public void Interact()
    {
        if (_tearSfx != null) AudioManager.Instance?.PlaySFX(_tearSfx);
        OnTornOff?.Invoke();
        Destroy(gameObject);
    }
}
