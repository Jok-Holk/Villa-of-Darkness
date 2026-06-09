using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class ChapterTransition : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasGroup;         // trên ChapterTransitionPanel
    [SerializeField] private Image _blackOverlay;
    [SerializeField] private TextMeshProUGUI _chapterText;
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Settings")]
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private float _typewriterSpeed = 0.05f;   // giây/ký tự
    [SerializeField] private float _subtitleDelay = 0.3f;      // chờ sau khi type xong
    [SerializeField] private float _holdDuration = 1.5f;       // giữ màn hình trước khi fade out
    [SerializeField] private float _fadeOutDuration = 1.0f;

    [SerializeField] private bool _isPlaying = false;

    public UnityEvent OnTransitionComplete = new UnityEvent();

    private void Awake()
    {
        // Ẩn panel khi bắt đầu
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void PlayTransition(string chapterName, string year)
    {
        if (_isPlaying) return;
        _isPlaying = true;
        gameObject.SetActive(true);
        StartCoroutine(Run(chapterName, year));
    }

    private IEnumerator Run(string chapterName, string year)
    {
        // Reset text
        _chapterText.text = "";
        _subtitleText.alpha = 0f;

        // 1. FADE IN màn đen
        yield return StartCoroutine(FadeCanvas(0f, 1f, _fadeInDuration));

        // 2. TYPEWRITER tên chương
        yield return StartCoroutine(Typewriter(_chapterText, chapterName));

        // 3. Chờ rồi hiện subtitle mờ dần
        yield return new WaitForSeconds(_subtitleDelay);
        _subtitleText.text = $"Biệt Thự Gia Đình {year.Split('–')[0].Trim()} · {year}";
        // Hoặc nếu truyền thẳng subtitle text:
        // _subtitleText.text = year; // dùng field year làm subtitle
        yield return StartCoroutine(FadeText(_subtitleText, 0f, 0.7f, 0.6f));

        // 4. GIỮ màn hình
        yield return new WaitForSeconds(_holdDuration);

        // 5. FADE OUT → vào scene
        yield return StartCoroutine(FadeCanvas(1f, 0f, _fadeOutDuration));

        // Kết thúc
        gameObject.SetActive(false);
        _isPlaying = false;
        OnTransitionComplete.Invoke();
    }

    // ── Helpers ──────────────────────────────────────────

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }

    private IEnumerator FadeText(TextMeshProUGUI tmp, float from, float to, float duration)
    {
        float elapsed = 0f;
        tmp.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        tmp.alpha = to;
    }

    private IEnumerator Typewriter(TextMeshProUGUI tmp, string text)
    {
        tmp.text = "";
        foreach (char c in text)
        {
            tmp.text += c;
            yield return new WaitForSeconds(_typewriterSpeed);
        }
    }
}