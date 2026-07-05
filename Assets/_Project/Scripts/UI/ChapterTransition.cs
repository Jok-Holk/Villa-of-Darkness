using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Fade đen + hiện title card "Chương X — Năm" khi chuyển chapter.
/// Dùng chung ScreenFader với Main Menu để tránh trùng 2 hệ thống fade khác nhau.
/// </summary>
public class ChapterTransition : MonoBehaviour
{
    [SerializeField] private bool _isPlaying = false;
    [SerializeField] private float _fadeOutDuration = 1f;
    [SerializeField] private float _holdDuration = 2f;
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private string _chapterName;
    [SerializeField] private string _year;
    public UnityEvent OnTransitionComplete = new UnityEvent();

    public void PlayTransition(string chapterName, string year)
    {
        if (_isPlaying) return;
        _isPlaying = true;
        _chapterName = chapterName;
        _year = year;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        var fader = ScreenFader.Instance;
        if (fader != null)
        {
            bool faded = false;
            fader.FadeOut(_fadeOutDuration, () => faded = true);
            yield return new WaitUntil(() => faded);
        }

        // TODO: hiện text _chapterName/_year lên UI title card ở đây khi có Text component wire vào.
        yield return new WaitForSeconds(_holdDuration);

        if (fader != null)
            fader.FadeIn(_fadeInDuration);

        _isPlaying = false;
        OnTransitionComplete.Invoke();
    }
}
