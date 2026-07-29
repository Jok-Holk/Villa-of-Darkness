using UnityEngine;
using System.Collections;

/// <summary>
/// Cinematic mở đầu Chapter 1. Đúng thứ tự:
///   1. Màn hình đen ngay từ đầu.
///   2. Player tự đi A→B trong lúc còn đen (chỉ nghe tiếng bước chân).
///   3. Hết di chuyển: camera xoay từ cắm đất lên hướng cuối (tự chỉnh tay), ĐỒNG THỜI panel đen
///      "chớp" (tắt/mở) theo nhịp không đều (Jok tự custom từng bước trong blinkSteps).
///   4. Chạy dialogue intro.
/// Gắn vào 1 GameObject trong nhóm GAMEPLAY SYSTEMS của Chapter1.unity.
/// </summary>
public class IntroManager : MonoBehaviour
{
    [System.Serializable]
    public class BlinkStep
    {
        [Tooltip("Giữ đen bao lâu trước khi mở")]
        public float holdBlackDuration = 0.3f;
        [Tooltip("Tốc độ mở panel đen ra — 0 = cắt cứng, không mượt")]
        public float fadeOutOfBlack = 0.1f;
        [Tooltip("Giữ mở (thấy cảnh) bao lâu trước khi (có thể) tối lại")]
        public float holdRevealDuration = 0.3f;
        [Tooltip("Tốc độ tối lại — bước CUỐI trong mảng nên để 0 (không tối lại nữa, giữ sáng luôn)")]
        public float fadeIntoBlack = 0.1f;
    }

    [Header("Di chuyển Player A→B lúc còn đen")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveDuration = 3f;

    [Header("Xoay camera — từ cắm đất lên hướng cuối, tự chỉnh Euler tay")]
    [SerializeField] private Vector3 cameraStartEuler = new Vector3(75f, 0f, 0f);
    [SerializeField] private Vector3 cameraEndEuler    = new Vector3(0f, 0f, 0f);
    [SerializeField] private float   revealRotationDuration = 2f;
    [Tooltip("Easing riêng cho xoay camera — mặc định chậm ở đầu/cuối để tạo cảm giác nặng nề, có chủ ý (không tuyến tính)")]
    [SerializeField] private AnimationCurve rotationEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Chớp đen — 2 panel trượt trên/dưới giả mí mắt (EyelidBlink), tự custom từng bước")]
    [SerializeField] private EyelidBlink eyelidBlink;
    [SerializeField] private BlinkStep[] blinkSteps = new BlinkStep[]
    {
        new BlinkStep { holdBlackDuration = 0.4f, fadeOutOfBlack = 0.08f, holdRevealDuration = 0.15f, fadeIntoBlack = 0.12f },
        new BlinkStep { holdBlackDuration = 0.5f, fadeOutOfBlack = 0.1f,  holdRevealDuration = 0.25f, fadeIntoBlack = 0.2f  },
        new BlinkStep { holdBlackDuration = 0.3f, fadeOutOfBlack = 0.5f,  holdRevealDuration = 0f,     fadeIntoBlack = 0f    },
    };

    [Header("Dialogue")]
    [SerializeField] private DialogueAsset introDialogue;

    [Header("HUD ẩn suốt intro (VD: thanh Stamina, icon pin đèn) — tự hiện lại khi gameplay bắt đầu")]
    [SerializeField] private GameObject[] hudToHideDuringIntro;

    [Header("Ambient ngoài trời (AmbientZone, Play On Start) — vặn nhỏ mượt lúc dialogue mở")]
    [SerializeField] private AmbientZone exteriorAmbient;
    [Tooltip("Mức volume ambient giữ nguyên vĩnh viễn sau khi dialogue mở (VD: 0.3 = 30%)")]
    [Range(0f, 1f)] [SerializeField] private float exteriorAmbientDuckedVolume = 0.2f;
    [Tooltip("Thời gian vặn nhỏ mượt — KHÔNG tức thì")]
    [SerializeField] private float exteriorAmbientDuckDuration = 2.5f;

    [Header("Debug / Test")]
    [Tooltip("Tick lên = bỏ qua TOÀN BỘ intro (không đen màn hình, không di chuyển ép buộc, không dialogue) — dùng khi test phần khác của map")]
    [SerializeField] private bool skipIntroEntirely = false;
    [SerializeField] private bool allowDebugSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Backspace;

    private CharacterController _cc;
    private bool _skipRequested;

    private void Start()
    {
        if (skipIntroEntirely) return;

        if (PlayerController.Instance != null)
            _cc = PlayerController.Instance.GetComponent<CharacterController>();

        // Đã có checkpoint (cổng sau khi xong intro lần đầu, hoặc xa hơn — VD phòng ăn) -- KHÔNG chạy
        // lại cinematic nữa, restore thẳng vị trí/hướng nhìn đã lưu rồi trả quyền điều khiển ngay.
        if (CheckpointManager.HasCheckpoint)
        {
            CheckpointManager.Restore(PlayerController.Instance.transform);
            PlayerController.Instance?.SetInputEnabled(true);
            SetHudVisible(true);
            return;
        }

        StartCoroutine(RunIntro());
    }

    private IEnumerator RunIntro()
    {
        _skipRequested = false;
        PlayerController.Instance?.SetInputEnabled(false);
        SetHudVisible(false); // ẩn HUD (stamina/pin đèn) suốt intro -- DialogueUI.CloseDialogue() sẽ tự hiện lại khi dialogue đóng, còn nếu KHÔNG có dialogue thì tự hiện ở nhánh else bên dưới

        // Panel đen (EyelidBlink) mặc định có thể đang tắt trong Hierarchy -- StartCoroutine trên
        // GameObject inactive sẽ KHÔNG chạy (Unity chỉ log warning, không throw), khiến callback
        // onComplete không bao giờ gọi -> BlinkStepsRoutine treo vĩnh viễn ở vòng "while (!done)" sau
        // này -> intro không bao giờ mở dialogue, input khoá luôn. Bật active lên trước, chắc chắn.
        if (eyelidBlink != null) eyelidBlink.gameObject.SetActive(true);
        eyelidBlink?.SnapClosed();

        // ─── BƯỚC 1: di chuyển A→B lúc còn đen ─────────────────────────────
        // Đi bằng CharacterController.Move() THẬT (giữ enabled xuyên suốt), KHÔNG lerp thẳng transform
        // -- HeadbobSystem đo tốc độ qua độ dời vị trí + _cc.isGrounded, mà isGrounded luôn false khi
        // CharacterController bị tắt (như bản cũ) nên tắt hẳn head-bob/tiếng bước chân dù vị trí vẫn
        // đổi mỗi frame. Move() thật còn tự tôn trọng va chạm tường/đồ vật dọc đường, không xuyên tường.
        if (pointA != null && pointB != null && PlayerController.Instance != null && _cc != null)
        {
            Transform player = PlayerController.Instance.transform;
            _cc.enabled = true;
            player.position = pointA.position;

            Vector3 flatDir = pointB.position - pointA.position;
            flatDir.y = 0f;
            float totalDist = flatDir.magnitude;
            if (flatDir.sqrMagnitude > 0.0001f) player.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
            float speed = totalDist / Mathf.Max(0.01f, moveDuration);

            // Giới hạn theo QUÃNG ĐƯỜNG CÒN LẠI (không phải theo thời gian) -- vòng lặp kiểu
            // "while (t < moveDuration) { t += dt; Move(...) }" luôn đi lố ở frame cuối (frame khiến t
            // vượt ngưỡng vẫn full step trước khi thoát), cộng dồn ra lố hẳn 1 khoảng nhìn thấy được so
            // với B. Trừ dần remaining, clamp step không vượt quá phần còn lại -> không bao giờ đi lố.
            float remaining = totalDist;
            while (remaining > 0.001f)
            {
                if (allowDebugSkip && Input.GetKeyDown(skipKey)) { _skipRequested = true; break; }
                float step = Mathf.Min(speed * Time.deltaTime, remaining);
                _cc.Move(player.forward * step);
                remaining -= step;
                if (!_cc.isGrounded) _cc.Move(Vector3.down * 9.8f * Time.deltaTime);
                yield return null;
            }

            // Chốt lại đúng X/Z điểm B (giữ nguyên Y hiện tại, tránh lún đất) -- phòng trường hợp sân
            // dốc nhẹ khiến CharacterController trượt thêm theo phương ngang lúc xử lý va chạm/trọng
            // lực, cộng dồn lệch khỏi B dù bước di chuyển đã clamp đúng quãng đường.
            if (!_skipRequested)
            {
                Vector3 finalPos = player.position;
                finalPos.x = pointB.position.x;
                finalPos.z = pointB.position.z;
                player.position = finalPos;
            }
        }

        // ─── BƯỚC 2: xoay camera + chớp đen song song ──────────────────────
        if (!_skipRequested)
        {
            Coroutine rotRoutine   = StartCoroutine(RotateCameraRoutine());
            Coroutine blinkRoutine = StartCoroutine(BlinkStepsRoutine());

            // Chờ cả 2 coroutine con xong (dựa vào cờ bool tự set ở đầu/cuối mỗi routine) — hoặc dừng
            // ngay lập tức nếu người chơi bấm skipKey giữa chừng.
            while (_rotRunning || _blinkRunning)
            {
                if (allowDebugSkip && Input.GetKeyDown(skipKey)) _skipRequested = true;
                if (_skipRequested)
                {
                    StopCoroutine(rotRoutine);
                    StopCoroutine(blinkRoutine);
                    _rotRunning = false;
                    _blinkRunning = false;
                    break;
                }
                yield return null;
            }
        }

        // ─── Snap về trạng thái cuối (luôn chạy, kể cả khi skip) ───────────
        if (Camera.main != null) Camera.main.transform.localRotation = Quaternion.Euler(cameraEndEuler);
        eyelidBlink?.SnapOpen(); // đảm bảo mở hẳn dù đang giữa 1 blink dở khi skip
        if (_cc != null) _cc.enabled = true;
        PlayerController.Instance?.SetPitch(cameraEndEuler.x);

        // Checkpoint "cổng vào" (cảnh 1) -- lưu NGAY sau khi xong intro (đi A→B + xoay camera + chớp
        // mắt), TRƯỚC khi dialogue chạy. Lần sau vào lại Chapter1 (Retry hoặc load lại scene) sẽ thấy
        // CheckpointManager.HasCheckpoint=true ở Start() bên trên -- tự bỏ qua toàn bộ cinematic này,
        // restore thẳng vào đúng vị trí/hướng nhìn hiện tại của Player lúc này.
        if (PlayerController.Instance != null)
            CheckpointManager.Save(1, PlayerController.Instance.transform.position, PlayerController.Instance.transform.rotation);

        // ─── BƯỚC 3: dialogue intro ─────────────────────────────────────────
        exteriorAmbient?.FadeToVolume(exteriorAmbientDuckedVolume, exteriorAmbientDuckDuration); // vặn nhỏ mượt, giữ nguyên từ đây trở đi
        if (introDialogue != null)
            DialogueUI.Instance?.StartDialogue(introDialogue);
        else
        {
            PlayerController.Instance?.SetInputEnabled(true); // không có dialogue thì tự trả quyền điều khiển luôn
            SetHudVisible(true); // ...và tự hiện lại HUD luôn, vì sẽ không có DialogueUI.CloseDialogue() nào gọi việc này
        }
    }

    private void SetHudVisible(bool visible)
    {
        if (hudToHideDuringIntro == null) return;
        foreach (var go in hudToHideDuringIntro)
            if (go != null) go.SetActive(visible);
    }

    private bool _rotRunning;
    private IEnumerator RotateCameraRoutine()
    {
        _rotRunning = true;
        if (Camera.main == null) { _rotRunning = false; yield break; }

        Camera.main.transform.localRotation = Quaternion.Euler(cameraStartEuler);
        float t = 0f;
        while (t < revealRotationDuration)
        {
            t += Time.deltaTime;
            float eased = rotationEaseCurve.Evaluate(Mathf.Clamp01(t / revealRotationDuration));
            Camera.main.transform.localRotation = Quaternion.Slerp(
                Quaternion.Euler(cameraStartEuler), Quaternion.Euler(cameraEndEuler), eased);
            yield return null;
        }
        Camera.main.transform.localRotation = Quaternion.Euler(cameraEndEuler);
        _rotRunning = false;
    }

    private bool _blinkRunning;
    private IEnumerator BlinkStepsRoutine()
    {
        _blinkRunning = true;
        foreach (var step in blinkSteps)
        {
            if (step.holdBlackDuration > 0f) yield return new WaitForSeconds(step.holdBlackDuration);

            // Mở mắt — 2 panel trượt ra khỏi màn hình.
            {
                bool done = false;
                eyelidBlink?.Open(step.fadeOutOfBlack, () => done = true);
                while (!done) yield return null;
            }

            if (step.holdRevealDuration > 0f) yield return new WaitForSeconds(step.holdRevealDuration);

            if (step.fadeIntoBlack > 0f)
            {
                // Nhắm mắt lại — bước CUỐI trong mảng nên để fadeIntoBlack = 0 (giữ mở luôn, không nhắm lại).
                bool done = false;
                eyelidBlink?.Close(step.fadeIntoBlack, () => done = true);
                while (!done) yield return null;
            }
        }
        _blinkRunning = false;
    }
}
