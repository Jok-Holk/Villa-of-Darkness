using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Full-screen fade đen dùng cho chuyển cảnh (Main Menu → Chapter1, đổi flythrough...).
/// Gắn vào 1 Canvas riêng (Screen Space - Overlay, sort order cao) có 1 Image đen phủ kín màn hình + CanvasGroup.
/// Tồn tại xuyên suốt qua scene load (DontDestroyOnLoad) nên chỉ cần 1 instance duy nhất trong toàn game.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup _canvasGroup;

    private Coroutine _routine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        StartFade(0f, 1f, duration, onComplete);
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        StartFade(1f, 0f, duration, onComplete);
    }

    /// <summary>Fade đen → load scene mới (async) → fade sáng lại.</summary>
    public void FadeToScene(string sceneName, float fadeOutDuration = 1f, float fadeInDuration = 1f)
    {
        StartFade(_canvasGroup.alpha, 1f, fadeOutDuration, () =>
        {
            StartCoroutine(LoadSceneRoutine(sceneName, fadeInDuration));
        });
    }

    private IEnumerator LoadSceneRoutine(string sceneName, float fadeInDuration)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        FadeIn(fadeInDuration);
    }

    private void StartFade(float from, float to, float duration, Action onComplete)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeRoutine(from, to, duration, onComplete));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
    {
        _canvasGroup.blocksRaycasts = true;
        float t = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        _canvasGroup.alpha = to;
        _canvasGroup.blocksRaycasts = to > 0.99f;
        onComplete?.Invoke();
    }
}
