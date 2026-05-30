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

    [Header("Visual")]
    [SerializeField] private Color _blueOverlayColor = new Color(0f, 0.3f, 1f, 0.6f);
    [SerializeField] private float _overlayFadeDuration = 1.5f;
    [SerializeField] private float _screenFadeDuration = 1f;

    [Header("References")]
    [SerializeField] private GazeTrigger _gazeTrigger;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private float _requiredDistance = 2f;

    private CanvasGroup _screenFadeCanvas;
    private bool _deathSequenceTriggered = false;

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

        // 2. Phát âm thanh Ma Da
        if (_maDaVoiceClip != null && _voiceDelay >= 0f)
        {
            yield return new WaitForSeconds(_voiceDelay);
            AudioManager.Instance?.PlaySFX(_maDaVoiceClip);
        }

        // 3. Áp dụng overlay xanh
        yield return StartCoroutine(ApplyBlueOverlay());

        // 4. Fade màn hình đen
        yield return StartCoroutine(FadeScreenToBlack());

        // 5. Hiển thị death screen với tên nhân vật và năm
        if (_deathScreenUI != null)
            _deathScreenUI.Show("Minh Khoa", "1979 – 2000");

        // 6. Báo GameManager rằng player đã chết nếu cần để cập nhật state chung
        GameManager.Instance?.PlayerDead();
        Debug.Log("[WellDeathSequence] Death sequence completed!");
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
