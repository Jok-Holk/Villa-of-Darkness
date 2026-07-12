using UnityEngine;

/// <summary>
/// Rung camera theo nấc sanity hiện tại (traumaTarget/shakeIntensity từ SanityData).
/// PHẢI gắn lên object có Camera THẬT (VD PlayerCamera), không phải object con rỗng — rotate 1 object
/// con không có Camera sẽ không có tác dụng thị giác gì cả (bug cũ: gắn trên "CameraShake", con rỗng
/// của PlayerCamera).
///
/// Chạy ở LateUpdate() và CỘNG THÊM (nhân Quaternion) lên rotation hiện tại thay vì ghi đè tuyệt đối —
/// PlayerController.Update() set _cameraTransform.localRotation mỗi frame cho mouse-look, ghi đè tuyệt
/// đối ở đây sẽ giật/snap góc nhìn y hệt bug đã gặp với HeadbobSystem trước đó.
/// </summary>
public class SanityShake : MonoBehaviour
{
    [SerializeField] private SanitySystem _sanitySystem;
    [SerializeField] private SanityData _data;

    private float _traumaCurrent;
    private float _swayTimer;
    private float _swayDuration;
    private float _peakX, _peakY;
    private int _currentIndex = 0;

    private void Start()
    {
        // Start() (không phải Awake()) để SanitySystem.Instance đã kịp gán xong trước khi fallback —
        // giống pattern SanityPostProcess, tránh race condition thứ tự Awake giữa các script.
        if (_sanitySystem == null) _sanitySystem = SanitySystem.Instance;

        if (_sanitySystem != null)
            _sanitySystem.OnLevelChanged.AddListener(index => _currentIndex = index);

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

    private void LateUpdate()
    {
        if (_data == null || _data.levels.Length <= _currentIndex) return;

        float targetTrauma = _data.levels[_currentIndex].traumaTarget;
        _traumaCurrent = Mathf.Lerp(_traumaCurrent, targetTrauma, Time.deltaTime * 2f);

        _swayTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_swayTimer / _swayDuration);
        float curve = Mathf.SmoothStep(0f, 1f, t < 0.3f ? t / 0.3f : 1f - (t - 0.3f) / 0.7f);

        // CỘNG THÊM lên rotation PlayerController vừa set xong ở Update() — không ghi đè tuyệt đối.
        transform.localRotation = transform.localRotation * Quaternion.Euler(_peakX * curve, _peakY * curve, 0f);

        if (_swayTimer >= _swayDuration) ScheduleNext();
    }
}
