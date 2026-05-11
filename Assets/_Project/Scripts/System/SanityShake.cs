using UnityEngine;

public class SanityShake : MonoBehaviour
{
    [SerializeField] private SanitySystem _sanitySystem;
    [SerializeField] private SanityData _data;

    private float _traumaCurrent;
    private float _swayTimer;
    private float _swayDuration;
    private float _peakX, _peakY;
    private int _currentIndex = 0;

    private void Awake()
    {
        if (_sanitySystem != null)
            _sanitySystem.OnLevelChanged.AddListener(level => _currentIndex = (int)level);
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        if (_data == null || _data.levels.Length <= _currentIndex) return;
        var s = _data.levels[_currentIndex];

        _swayDuration = Random.Range(1.0f, 2.0f);
        _swayTimer = 0f;

        _peakX = Random.Range(s.shakeIntensityX * 0.8f, s.shakeIntensityX) * _traumaCurrent * (Random.value > 0.5f ? 1 : -1);
        _peakY = Random.Range(s.shakeIntensityY * 0.8f, s.shakeIntensityY) * _traumaCurrent * (Random.value > 0.5f ? 1 : -1);
    }

    private void Update()
    {
        if (_data == null || _data.levels.Length <= _currentIndex) return;
        
        float targetTrauma = _data.levels[_currentIndex].traumaTarget;
        _traumaCurrent = Mathf.Lerp(_traumaCurrent, targetTrauma, Time.deltaTime * 2f);

        _swayTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_swayTimer / _swayDuration);
        float curve = Mathf.SmoothStep(0f, 1f, t < 0.3f ? t / 0.3f : 1f - (t - 0.3f) / 0.7f);

        transform.localRotation = Quaternion.Euler(_peakX * curve, _peakY * curve, 0);

        if (_swayTimer >= _swayDuration) ScheduleNext();
    }
}