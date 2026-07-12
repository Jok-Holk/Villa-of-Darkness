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

        StartCoroutine(RunIntro());
    }

    private IEnumerator RunIntro()
    {
        _skipRequested = false;
        PlayerController.Instance?.SetInputEnabled(false);
        eyelidBlink?.SnapClosed();

        // ─── BƯỚC 1: di chuyển A→B lúc còn đen ─────────────────────────────
        if (pointA != null && pointB != null && PlayerController.Instance != null)
        {
            Transform player = PlayerController.Instance.transform;
            if (_cc != null) _cc.enabled = false;
            player.position = pointA.position;

            float t = 0f;
            while (t < moveDuration)
            {
                if (allowDebugSkip && Input.GetKeyDown(skipKey)) { _skipRequested = true; break; }
                t += Time.deltaTime;
                player.position = Vector3.Lerp(pointA.position, pointB.position, Mathf.Clamp01(t / moveDuration));
                yield return null;
            }
            player.position = pointB.position;
            if (_cc != null) _cc.enabled = true;
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

        // ─── BƯỚC 3: dialogue intro ─────────────────────────────────────────
        if (introDialogue != null)
            DialogueUI.Instance?.StartDialogue(introDialogue);
        else
            PlayerController.Instance?.SetInputEnabled(true); // không có dialogue thì tự trả quyền điều khiển luôn
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
