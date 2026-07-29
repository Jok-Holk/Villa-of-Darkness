using UnityEngine;
using System.Collections;

// Gắn lên chính GameObject cái gương (cùng object với GazeTrigger + GazeSettings + Collider).
//
// SỬA 2026-07-26: Trước đây tưởng "vải còn che thì raycast tự trúng vải trước, không trúng gương nên
// GazeTrigger chưa tính giờ được" -- SAI, vì GazeTrigger coi trúng CHÍNH NÓ hoặc BẤT KỲ CON NÀO của nó là
// "đang nhìn" (transform.IsChildOf), mà tấm vải (ClothCover) lại là CON của chính gương này -- nghĩa là
// Player nhìn tấm vải (lúc còn che) đã bị tính giờ luôn rồi! Nếu Player đứng nhìn vải đủ lâu trước khi gỡ,
// vừa gỡ xong là raycast chuyển sang trúng gương thật -- timer coi như đã đầy từ trước, giật mình TỨC THÌ.
// Giờ khoá hẳn GazeTrigger (enabled=false) từ đầu, CHỈ mở lại sau khi vải bị gỡ (OnTornOff) + chờ thêm
// _gazeArmDelay giây nữa (grace period Jok yêu cầu, mặc định 3s) mới cho phép bắt đầu tính giờ nhìn.
[RequireComponent(typeof(GazeTrigger))]
public class MirrorJumpscareReaction : MonoBehaviour
{
    [SerializeField] private GazeTrigger _gazeTrigger;
    [SerializeField] private Sprite _jumpscareImage;
    [SerializeField] private AudioClip _screamSfx;
    [Tooltip("Số giây chờ SAU KHI gỡ vải mới cho phép GazeTrigger bắt đầu tính giờ nhìn -- tránh giật mình tức thì ngay lúc vừa gỡ xong")]
    [SerializeField] private float _gazeArmDelay = 3f;

    private bool _triggered = false;

    private void Reset()
    {
        _gazeTrigger = GetComponent<GazeTrigger>();
    }

    private void OnEnable()
    {
        if (_gazeTrigger == null) _gazeTrigger = GetComponent<GazeTrigger>();
        if (_gazeTrigger != null)
        {
            _gazeTrigger.OnGazeComplete.AddListener(OnMirrorGazeComplete);
            _gazeTrigger.enabled = false; // Khoá ngay từ đầu -- chỉ mở lại sau khi gỡ vải + grace period
        }

        var cloth = GetComponentInChildren<TearOffClothCover>();
        if (cloth != null) cloth.OnTornOff.AddListener(OnClothTornOff);
    }

    private void OnDisable()
    {
        if (_gazeTrigger != null) _gazeTrigger.OnGazeComplete.RemoveListener(OnMirrorGazeComplete);

        var cloth = GetComponentInChildren<TearOffClothCover>();
        if (cloth != null) cloth.OnTornOff.RemoveListener(OnClothTornOff);
    }

    private void OnClothTornOff()
    {
        StartCoroutine(ArmGazeAfterDelay());
    }

    private IEnumerator ArmGazeAfterDelay()
    {
        yield return new WaitForSeconds(_gazeArmDelay);
        if (_gazeTrigger != null) _gazeTrigger.enabled = true;
    }

    public void OnMirrorGazeComplete()
    {
        if (_triggered) return;
        _triggered = true;
        JumpscareGameOverUI.Trigger(_jumpscareImage, _screamSfx, 1f, 3f);
    }
}
