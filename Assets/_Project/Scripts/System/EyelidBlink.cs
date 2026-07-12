using System;
using System.Collections;
using UnityEngine;

// 2 panel đen (nửa trên/nửa dưới màn hình) trượt tách ra/khép lại theo chiều dọc ngược nhau — giả lập
// mở/nhắm mắt thật, khác ScreenFader (alpha fade toàn màn hình phẳng). Dùng cho IntroManager cinematic.
// topPanel/bottomPanel neo sẵn đúng nửa màn hình (anchorMin/Max) — script chỉ dịch anchoredPosition.y
// theo rect.height của chính panel đó nên tự đúng theo mọi độ phân giải/Canvas Scaler, không hardcode px.
public class EyelidBlink : MonoBehaviour
{
    [SerializeField] private RectTransform topPanel;
    [SerializeField] private RectTransform bottomPanel;

    private Coroutine _routine;

    // Đóng hẳn (2 panel phủ kín màn hình) — gọi lúc khởi tạo để tránh loé sáng 1 frame trước khi cinematic
    // kịp chạy.
    public void SnapClosed()
    {
        if (_routine != null) StopCoroutine(_routine);
        if (topPanel != null) topPanel.anchoredPosition = Vector2.zero;
        if (bottomPanel != null) bottomPanel.anchoredPosition = Vector2.zero;
    }

    public void SnapOpen()
    {
        if (_routine != null) StopCoroutine(_routine);
        if (topPanel != null) topPanel.anchoredPosition = new Vector2(0f, topPanel.rect.height);
        if (bottomPanel != null) bottomPanel.anchoredPosition = new Vector2(0f, -bottomPanel.rect.height);
    }

    // Mở mắt — 2 panel trượt ra khỏi màn hình, lộ cảnh ra. Tương đương ScreenFader.FadeIn cũ.
    public void Open(float duration, Action onComplete = null) => StartSlide(true, duration, onComplete);

    // Nhắm mắt — 2 panel trượt về giữa, phủ kín lại. Tương đương ScreenFader.FadeOut cũ.
    public void Close(float duration, Action onComplete = null) => StartSlide(false, duration, onComplete);

    private void StartSlide(bool opening, float duration, Action onComplete)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(SlideRoutine(opening, duration, onComplete));
    }

    private IEnumerator SlideRoutine(bool opening, float duration, Action onComplete)
    {
        float topOpenY = topPanel != null ? topPanel.rect.height : 0f;
        float botOpenY = bottomPanel != null ? -bottomPanel.rect.height : 0f;

        float fromTop = topPanel != null ? topPanel.anchoredPosition.y : 0f;
        float fromBot = bottomPanel != null ? bottomPanel.anchoredPosition.y : 0f;
        float toTop = opening ? topOpenY : 0f;
        float toBot = opening ? botOpenY : 0f;

        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            if (topPanel != null) topPanel.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromTop, toTop, k));
            if (bottomPanel != null) bottomPanel.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromBot, toBot, k));
            yield return null;
        }
        if (topPanel != null) topPanel.anchoredPosition = new Vector2(0f, toTop);
        if (bottomPanel != null) bottomPanel.anchoredPosition = new Vector2(0f, toBot);
        onComplete?.Invoke();
    }
}
