using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Kích hoạt chuỗi chết khi player nhìn vào giếng quá lâu.
/// Được gắn vào Well GameObject, nghe event OnGazeComplete từ GazeTrigger.
/// </summary>
public class WellDeathSequence : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DeathScreenUI _deathScreenUI;

    [Header("Audio")]
    [SerializeField] private AudioClip _maDaVoiceClip;  // Tiếng Ma Da ("Minh Khoa")
    [SerializeField] private float _voiceDelay = 0f;
    [Tooltip("Âm thanh giật mình khi Ma Da trồi lên — phát đúng lúc apparition xuất hiện")]
    [SerializeField] private AudioClip _jumpscareSfx;

    [Header("Visual — Bước 1: đốm sáng dưới nước (trước jumpscare)")]
    [SerializeField] private Color _blueOverlayColor = new Color(0f, 0.3f, 1f, 0.6f);
    [SerializeField] private float _overlayFadeDuration = 1.5f;

    [Header("Visual — Bước 2: jumpscare Ma Da trồi lên (tuỳ chọn)")]
    [Tooltip("GameObject Ma Da đặt sẵn trong giếng, mặc định tắt — sẽ SetActive(true) đúng lúc jumpscare. Để trống nếu chưa có model, tự dùng '_jumpscareSprite' 2D thay thế (nếu có gán); nếu cả 2 đều trống thì bỏ qua bước ảnh, sequence vẫn chạy.")]
    [SerializeField] private GameObject _maDaApparition;
    [Tooltip("Ảnh jumpscare 2D (AI-generated) DÙNG THAY nếu chưa dựng được model 3D Ma Da -- bật full-screen đột ngột, ưu tiên model 3D hơn nếu cả 2 đều gán.")]
    [SerializeField] private Sprite _jumpscareSprite;
    [Tooltip("Ma Da trồi lên (hoặc ảnh 2D bật ra) trong bao lâu trước khi màn hình bắt đầu tối")]
    [SerializeField] private float _apparitionPopDuration = 0.15f;
    [Tooltip("Giữ hình Ma Da hiện rõ bao lâu trước khi fade đen — đây là nhịp 'giật mình' của jumpscare")]
    [SerializeField] private float _apparitionHoldDuration = 0.6f;

    [Header("Visual — Bước 2b: kéo Player chìm xuống (CHẠY ĐỒNG THỜI với jumpscare bước 2, không phải trước/sau)")]
    [Tooltip("1 thế lực vô hình dưới nước kéo Player chìm dần -- chạy song song với jumpscare cho hợp lý (nếu để tuần tự, người chơi đã bị kéo xong xuôi mới thấy ảnh bật ra thì vô lý).")]
    [SerializeField] private float _pullDownDistance = 1.5f;
    [SerializeField] private float _pullDownDuration = 1.2f;

    [Header("Visual — Bước 3: fade đen")]
    [SerializeField] private float _screenFadeDuration = 1f;

    [Header("References")]
    [SerializeField] private GazeTrigger _gazeTrigger;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private float _requiredDistance = 2f;

    private CanvasGroup _screenFadeCanvas;
    private bool _deathSequenceTriggered = false;

    private void Start()
    {
        if (_playerController == null) _playerController = PlayerController.Instance;
    }

    private void OnEnable()
    {
        if (_gazeTrigger != null)
            _gazeTrigger.OnGazeComplete.AddListener(OnWellGazeComplete);
    }

    private void OnDisable()
    {
        if (_gazeTrigger != null)
            _gazeTrigger.OnGazeComplete.RemoveListener(OnWellGazeComplete);
    }

    public void OnWellGazeComplete()
    {
        if (_deathSequenceTriggered) return;

        if (_playerController != null)
        {
            float distance = Vector3.Distance(_playerController.transform.position, transform.position);
            if (distance > _requiredDistance)
            {
                Debug.Log($"[WellDeathSequence] Gaze complete ignored - too far ({distance:F2}m). Required <= {_requiredDistance}m.");
                return;
            }
        }

        _deathSequenceTriggered = true;
        StartCoroutine(PlayDeathSequence());
    }

    private IEnumerator PlayDeathSequence()
    {
        // 1. Tắt input player
        if (_playerController != null)
            _playerController.SetInputEnabled(false);

        Debug.Log("[WellDeathSequence] Death sequence started!");

        // 2. Phát âm thanh Ma Da (giọng thì thầm dưới nước)
        if (_maDaVoiceClip != null && _voiceDelay >= 0f)
        {
            yield return new WaitForSeconds(_voiceDelay);
            AudioManager.Instance?.PlaySFX(_maDaVoiceClip);
        }

        // 3. Đốm sáng dưới nước — cảnh báo trước jumpscare, đúng nhịp GDD
        yield return StartCoroutine(ApplyBlueOverlay());

        // 4. Kéo Player chìm xuống + Jumpscare Ma Da trồi lên — ĐỒNG THỜI, không tuần tự (đang bị kéo
        // xuống mà màn hình đứng yên chờ ảnh mới bật ra thì không hợp lý).
        Coroutine pullDown = StartCoroutine(PullPlayerDown());
        yield return StartCoroutine(PlayJumpscare());
        yield return pullDown;

        // 5. Fade màn hình đen
        yield return StartCoroutine(FadeScreenToBlack());

        // 6. Báo GameManager player đã chết — GameManager tự lo việc hiện DeathScreenUI,
        //    KHÔNG gọi _deathScreenUI.Show() ở đây nữa (tránh gọi Show() 2 lần chồng nhau).
        GameManager.Instance?.PlayerDead();
        Debug.Log("[WellDeathSequence] Death sequence completed!");
    }

    private IEnumerator PlayJumpscare()
    {
        // SFX phát bất kể có model/ảnh hay chưa — trước đây bị kẹt trong nhánh early-return của
        // _maDaApparition nên nếu chưa gắn model thì im lặng luôn, dù đã có sẵn SFX_WellJumpscare_MaDa.
        if (_jumpscareSfx != null)
            AudioManager.Instance?.PlaySFX(_jumpscareSfx);

        if (_maDaApparition != null)
        {
            // Trồi lên đột ngột: pop scale từ 0 lên 1 trong thời gian rất ngắn — cảm giác giật mình
            Transform t = _maDaApparition.transform;
            Vector3 fullScale = t.localScale == Vector3.zero ? Vector3.one : t.localScale;
            t.localScale = Vector3.zero;
            _maDaApparition.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _apparitionPopDuration)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(Vector3.zero, fullScale, elapsed / _apparitionPopDuration);
                yield return null;
            }
            t.localScale = fullScale;

            // Giữ hình rõ một nhịp trước khi màn hình bắt đầu tối — đây là khoảnh khắc "giật mình"
            yield return new WaitForSeconds(_apparitionHoldDuration);
        }
        else if (_jumpscareSprite != null)
        {
            yield return StartCoroutine(PlaySpriteJumpscare());
        }
        // Cả 2 đều trống — chưa có model lẫn ảnh, bỏ qua bước hình ảnh, sequence vẫn chạy tiếp (đã phát SFX ở trên).
    }

    private IEnumerator PlaySpriteJumpscare()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) yield break;

        GameObject imgGO = new GameObject("MaDaJumpscareImage");
        imgGO.transform.SetParent(canvas.transform, false);
        var img = imgGO.AddComponent<Image>();
        img.sprite = _jumpscareSprite;
        img.preserveAspect = true;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < _apparitionPopDuration)
        {
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / _apparitionPopDuration);
            yield return null;
        }
        rt.localScale = Vector3.one;

        yield return new WaitForSeconds(_apparitionHoldDuration);

        // FadeScreenToBlack ngay sau đó tự tạo Image riêng của nó — không cần giữ ảnh jumpscare này lại.
        Object.Destroy(imgGO);
    }

    private IEnumerator PullPlayerDown()
    {
        if (_playerController == null) yield break;

        Transform p = _playerController.transform;
        Vector3 start = p.position;
        Vector3 end = start + Vector3.down * _pullDownDistance;

        float t = 0f;
        while (t < _pullDownDuration)
        {
            t += Time.deltaTime;
            float progress = t / _pullDownDuration;
            // Rung nhẹ giảm dần — cảm giác vùng vẫy trước khi bị kéo hẳn xuống, không phải trượt đều tăm tắp.
            Vector3 jitter = new Vector3(Mathf.Sin(t * 23f), 0f, Mathf.Cos(t * 19f)) * 0.03f * (1f - progress);
            p.position = Vector3.Lerp(start, end, progress) + jitter;
            yield return null;
        }
        p.position = end;
    }

    private IEnumerator ApplyBlueOverlay()
    {
        // Tạo overlay canvas nếu chưa có
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[WellDeathSequence] Không tìm thấy Canvas!");
            yield break;
        }

        Image overlayImage = canvas.GetComponentInChildren<Image>();
        if (overlayImage == null)
        {
            // Tạo Image mới cho overlay
            GameObject overlayGO = new GameObject("BlueOverlay");
            overlayGO.transform.SetParent(canvas.transform, false);
            overlayImage = overlayGO.AddComponent<Image>();
            overlayImage.color = Color.clear;
        }

        // Fade in overlay xanh
        float elapsed = 0f;
        Color startColor = Color.clear;
        while (elapsed < _overlayFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _overlayFadeDuration;
            overlayImage.color = Color.Lerp(startColor, _blueOverlayColor, t);
            yield return null;
        }

        overlayImage.color = _blueOverlayColor;
    }

    private IEnumerator FadeScreenToBlack()
    {
        // Nếu chưa có screen fade canvas, skip
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            yield break;
        }

        // Tìm hoặc tạo Image cho fade
        Image fadeImage = canvas.GetComponentInChildren<Image>();
        if (fadeImage == null)
        {
            GameObject fadeGO = new GameObject("ScreenFade");
            fadeGO.transform.SetParent(canvas.transform, false);
            fadeImage = fadeGO.AddComponent<Image>();
            fadeImage.color = Color.clear;
        }

        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = Color.black;

        while (elapsed < _screenFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _screenFadeDuration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        fadeImage.color = targetColor;
    }
}
