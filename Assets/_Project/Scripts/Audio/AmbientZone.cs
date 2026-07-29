using UnityEngine;
using System.Collections;
#pragma warning disable CS0414

// Ambient dùng chung 2 kiểu:
//   1) TRIGGER ZONE (cần Collider isTrigger) -- player bước vào/ra thì fade lên _targetVolume / về 0.
//   2) PLAY ON START (không cần Collider) -- tự Play() + fade lên _targetVolume ngay khi scene load,
//      dùng cho ambient nền không gắn với vùng không gian nào (VD: intro cinematic).
// Cả 2 kiểu đều dùng chung FadeToVolume() -- script khác (IntroManager...) cũng gọi thẳng hàm này để
// vặn volume tới bất kỳ mức nào, không chỉ 0/_targetVolume.
[RequireComponent(typeof(AudioSource))]
public class AmbientZone : MonoBehaviour
{
    [SerializeField] private float  _targetVolume = 0.8f;
    [SerializeField] private float  _fadeDuration = 1.5f;
    [SerializeField] private string _targetTag    = "Player";

    [Header("Play On Start — bỏ qua Collider/trigger, tự phát ngay khi scene load")]
    [Tooltip("Tick lên: tự Play() + fade lên _targetVolume ngay ở Start(), không cần Collider nào cả.")]
    [SerializeField] private bool _playOnStart = false;

    private AudioSource _audioSource;
    private Collider    _collider; // optional -- chỉ dùng cho kiểu trigger zone
    private bool        _isActive = false;
    public  bool IsActive => _isActive;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        _audioSource        = GetComponent<AudioSource>();
        _collider            = GetComponent<Collider>();
        _audioSource.volume = 0f;
        _audioSource.loop   = true;
    }

    private void Start()
    {
        if (_playOnStart)
        {
            _isActive = true;
            _audioSource.Play();
            FadeToVolume(_targetVolume, _fadeDuration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collider == null) return; // playOnStart mode -- không dùng trigger
        if (other.CompareTag(_targetTag))
        {
            _isActive = true;
            if (!_audioSource.isPlaying) _audioSource.Play();
            FadeToVolume(_targetVolume, _fadeDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_collider == null) return;
        if (other.CompareTag(_targetTag))
            FadeToVolume(0f, _fadeDuration, stopOnComplete: true);
    }

    /// Vặn volume mượt tới TARGET bất kỳ trong DURATION giây -- dùng chung cho trigger zone (0f/
    /// _targetVolume) lẫn gọi tay từ script khác (VD: IntroManager vặn ambient xuống 30% lúc dialogue
    /// mở, giữ nguyên luôn từ đó). duration <= 0 = snap tức thì, không mượt.
    public Coroutine FadeToVolume(float target, float duration, bool stopOnComplete = false)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(target, duration, stopOnComplete));
        return _fadeRoutine;
    }

    private IEnumerator FadeRoutine(float target, float duration, bool stopOnComplete)
    {
        float startVolume = _audioSource.volume;
        float elapsed = 0f;
        duration = Mathf.Max(0f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, target, elapsed / duration);
            yield return null;
        }

        _audioSource.volume = target;
        if (stopOnComplete)
        {
            _audioSource.Stop();
            _isActive = false;
        }
    }
}
