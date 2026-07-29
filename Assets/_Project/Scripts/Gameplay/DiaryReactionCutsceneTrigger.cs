using UnityEngine;
using System.Collections;

// Vùng trigger đặt gần cửa Salon->Hành Lang (hướng Thư Phòng) -- CHỈ chạy cutscene (mở cửa + Ma Vú Dài
// lướt qua + lời thoại) nếu Player ĐANG Ở TRONG vùng này VÀ đã đọc xong nhật ký (DiaryReaderUI.Instance.
// HasFinishedReading). KHÔNG trigger ngay lúc đọc xong nhật ký (Player có thể đang ở phòng khác lúc đó,
// đọc từ Inventory không đổi vị trí) -- phải đợi tới khi thực sự đứng gần đúng khu vực này mới hợp lý.
[RequireComponent(typeof(Collider))]
public class DiaryReactionCutsceneTrigger : MonoBehaviour
{
    [Header("Cửa Salon -> Hành Lang (khoá cứng, không chìa nào mở được -- chỉ mở qua cutscene này)")]
    [SerializeField] private DoorController _door;

    [Header("Hướng Player bị force nhìn về lúc cửa mở")]
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private float _rotateDuration = 0.8f;

    [Header("Ma Vú Dài lướt qua nhanh -- ĐỂ TRỐNG _ghost thì TỰ SPAWN bằng code từ _ghostPrefab lúc Awake(), " +
             "không cần đặt tay/kéo tay object có sẵn trong scene nữa (Jok báo -- \"chưa hề có cái của Thuận " +
             "spawn ra bao giờ\"). Có gán tay _ghost sẵn thì giữ nguyên, không spawn thêm.")]
    [SerializeField] private Transform _ghost;
    [Tooltip("Model thật (VD Thuan.fbx) -- chỉ dùng khi _ghost để trống.")]
    [SerializeField] private GameObject _ghostPrefab;
    [Tooltip("Animator Controller gán cho model vừa spawn (VD MonsterAnimator.controller).")]
    [SerializeField] private RuntimeAnimatorController _ghostAnimatorController;
    [SerializeField] private Transform _ghostGlideStart;
    [SerializeField] private Transform _ghostGlideEnd;
    [SerializeField] private float _ghostGlideDuration = 1.2f;

    [Header("Lời thoại lúc thấy thoáng qua (tuỳ chọn)")]
    [SerializeField] private DialogueAsset _glimpseDialogue;

    private bool _hasTriggered = false;

    private void Awake()
    {
        if (_ghost == null && _ghostPrefab != null)
        {
            var instance = Instantiate(_ghostPrefab, transform);
            instance.name = _ghostPrefab.name + "_Spawned";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.SetActive(false); // RunCutscene() tự SetActive(true) đúng lúc cần

            var anim = instance.GetComponentInChildren<Animator>();
            if (anim == null) anim = instance.AddComponent<Animator>();
            if (_ghostAnimatorController != null) anim.runtimeAnimatorController = _ghostAnimatorController;

            _ghost = instance.transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (DiaryReaderUI.Instance == null || !DiaryReaderUI.Instance.HasFinishedReading) return;

        _hasTriggered = true;
        StartCoroutine(RunCutscene());
    }

    private IEnumerator RunCutscene()
    {
        PlayerController.Instance?.SetInputEnabled(false);

        // Xoay mượt Player hướng về phía cửa/hành lang
        if (_lookAtTarget != null && PlayerController.Instance != null)
        {
            Transform player = PlayerController.Instance.transform;
            Vector3 dir = _lookAtTarget.position - player.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion fromRot = player.rotation;
                Quaternion toRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                float t = 0f;
                while (t < _rotateDuration)
                {
                    t += Time.deltaTime;
                    player.rotation = Quaternion.Slerp(fromRot, toRot, t / _rotateDuration);
                    yield return null;
                }
                player.rotation = toRot;
            }
        }

        // Mở cửa -- khoá cứng, không cần chìa, cutscene tự SetLocked(false) rồi Open()
        if (_door != null)
        {
            _door.SetLocked(false);
            _door.Open();
        }

        yield return new WaitForSeconds(0.3f);

        // Ma Vú Dài lướt qua nhanh (Lerp đơn giản, không phải AI chase thật)
        if (_ghost != null && _ghostGlideStart != null && _ghostGlideEnd != null)
        {
            _ghost.gameObject.SetActive(true);
            _ghost.position = _ghostGlideStart.position;

            // Quay mặt theo đúng hướng di chuyển (Start->End) -- không dùng rotation có sẵn của điểm Start,
            // nếu không model sẽ trượt ngang/lùi trông giả tạo thay vì có cảm giác đang bước tới.
            Vector3 travelDir = _ghostGlideEnd.position - _ghostGlideStart.position;
            if (travelDir.sqrMagnitude > 0.0001f)
                _ghost.rotation = Quaternion.LookRotation(travelDir.normalized, Vector3.up);

            // Chạy animation đi/chạy (param "Speed" -- MonsterAnimator.controller của Thuận, cùng quy ước với
            // GhostAI) trong lúc lướt, tránh đứng yên/T-pose khi trượt vị trí.
            var ghostAnimator = _ghost.GetComponentInChildren<Animator>();
            if (ghostAnimator != null) ghostAnimator.SetFloat("Speed", 2f);

            float t = 0f;
            while (t < _ghostGlideDuration)
            {
                t += Time.deltaTime;
                _ghost.position = Vector3.Lerp(_ghostGlideStart.position, _ghostGlideEnd.position, t / _ghostGlideDuration);
                yield return null;
            }

            if (ghostAnimator != null) ghostAnimator.SetFloat("Speed", 0f);
            _ghost.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        if (_glimpseDialogue != null)
        {
            DialogueUI.Instance?.StartDialogue(_glimpseDialogue);
            while (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen())
                yield return null;
        }

        PlayerController.Instance?.SetInputEnabled(true);
    }
}
