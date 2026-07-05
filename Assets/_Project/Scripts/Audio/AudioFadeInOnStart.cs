using System.Collections;
using UnityEngine;

/// <summary>
/// Fade-in âm lượng của AudioSource từ 0 lên giá trị gốc trong fadeInDuration giây, chỉ chạy 1 LẦN lúc
/// GameObject khởi động (Start). AudioSource.loop=true tự lặp lại bên trong engine, không gọi lại Play()
/// nên fade-in này không tính vào các lần lặp sau.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioFadeInOnStart : MonoBehaviour
{
    [SerializeField] private float _fadeInDuration = 3f;

    private AudioSource _source;
    private float _targetVolume;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _targetVolume = _source.volume;
        _source.volume = 0f;
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < _fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            _source.volume = Mathf.Lerp(0f, _targetVolume, t / _fadeInDuration);
            yield return null;
        }
        _source.volume = _targetVolume;
    }
}
