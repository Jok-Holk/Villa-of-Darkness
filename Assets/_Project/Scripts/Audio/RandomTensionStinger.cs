using System.Collections;
using UnityEngine;

// Phát ngẫu nhiên 1 trong các tiếng rợn (VD Amb_CH1_Day_01/02) xen kẽ suốt thời gian chơi để tạo căng
// thẳng nền -- khác RandomAmbientTrigger (cần Player đi vào trigger zone của 1 phòng cụ thể), cái này
// chạy toàn cục ngay khi scene bắt đầu, không cần trigger. Mỗi lần chỉ phát 1 lần, không loop.
//
// Dùng AudioSource riêng (không qua AudioManager.PlaySFX/PlayOneShot) để tự fade volume từ 0 lên --
// PlayOneShot phát full volume ngay lập tức nghe như jumpscare/giật mình, ngược với mục đích "tạo căng
// thẳng từ từ". Vẫn tự nhân theo AudioManager.SfxVolumeScale để tôn trọng thanh trượt SFX trong Settings.
[RequireComponent(typeof(AudioSource))]
public class RandomTensionStinger : MonoBehaviour
{
    [Header("Danh sách tiếng rợn -- random chọn 1 mỗi lần phát")]
    [SerializeField] private AudioClip[] _clips;

    [Header("Thời gian giữa 2 lần phát (giây)")]
    [SerializeField] private float _minInterval = 60f;
    [SerializeField] private float _maxInterval = 150f;

    [Range(0f, 1f)]
    [SerializeField] private float _volume = 0.65f;

    [Tooltip("Thời gian fade từ im lặng lên đúng volume -- tránh phát đột ngột full volume nghe như giật mình")]
    [SerializeField] private float _fadeInDuration = 2.5f;

    private AudioSource _source;
    private float _nextTriggerTime;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f;
    }

    private void Start()
    {
        ScheduleNext();
    }

    private void Update()
    {
        if (_clips == null || _clips.Length == 0) return;

        _nextTriggerTime -= Time.deltaTime;
        if (_nextTriggerTime <= 0f)
        {
            PlayRandomClip();
            ScheduleNext();
        }
    }

    private void PlayRandomClip()
    {
        AudioClip clip = _clips[Random.Range(0, _clips.Length)];
        if (clip == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeInPlay(clip));
    }

    private IEnumerator FadeInPlay(AudioClip clip)
    {
        float scale = AudioManager.Instance != null ? AudioManager.Instance.SfxVolumeScale : 1f;
        float target = _volume * scale;

        _source.clip = clip;
        _source.volume = 0f;
        _source.Play();

        float t = 0f;
        while (t < _fadeInDuration)
        {
            t += Time.deltaTime;
            _source.volume = Mathf.Lerp(0f, target, t / _fadeInDuration);
            yield return null;
        }
        _source.volume = target;
    }

    private void ScheduleNext()
    {
        _nextTriggerTime = Random.Range(_minInterval, _maxInterval);
    }
}
