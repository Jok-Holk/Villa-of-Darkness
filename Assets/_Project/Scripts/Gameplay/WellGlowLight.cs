using UnityEngine;

// "Quả ánh sáng trên miệng giếng" (Jok yêu cầu 2026-07-31) -- ánh sáng ma quái quanh giếng. Light THẬT
// trong world, độc lập hoàn toàn với WellDeathSequence.cs (đó là overlay toàn MÀN HÌNH lúc jumpscare) -- 2
// lớp hiệu ứng khác nhau, không đụng nhau.
//
// SỬA 2026-07-31 (Jok chỉnh: "ban đầu cảnh 2 thì well vô hại lắm... cutscene mà, đâu phải gaze nữa"; "glow
// light là đổi intensity sau khi activate chứ không phải tắt bật"): Light LUÔN enabled=true (KHÔNG bật/tắt
// component) -- trước khi Activate() thì giữ intensity ở mức DORMANT (gần như không nhận ra, cảnh 2 vô hại
// tuyệt đối), sau khi Activate() thì mới đổi sang mức nhấp nháy nền rõ rệt. Activate() do WalkToWellCutscene
// gọi lúc Player được ép đi bộ ra tới nơi (KHÔNG phải GazeTrigger/tự do gaze -- đây là đoạn cutscene kết
// Chapter 1, phải chắc chắn xảy ra). Intensify() (sáng rực + đổi màu hẳn) là bước RIÊNG, mạnh hơn, gọi tiếp
// sau đó lúc WellDeathSequence bắt đầu jumpscare thật.
public class WellGlowLight : MonoBehaviour
{
    [Tooltip("Light đặt tại miệng giếng -- để trống thì tự tìm Light trên chính GameObject này.")]
    [SerializeField] private Light _glowLight;

    [Tooltip("Cường độ TRƯỚC KHI Activate() -- gần như không nhận ra, giữ giếng vô hại tuyệt đối ở cảnh 2.")]
    [SerializeField] private float _dormantIntensity = 0.05f;

    [Header("Nhấp nháy nền (SAU KHI Activate() -- xem WalkToWellCutscene.OnArrivedAtWell)")]
    [SerializeField] private float _minIntensity = 0.4f;
    [SerializeField] private float _maxIntensity = 1.2f;
    [SerializeField] private float _flickerSpeed = 1.5f;
    [Tooltip("Màu ánh sáng mặc định -- xanh lục/lam nhạt kiểu ma quái, không phải trắng thường.")]
    [SerializeField] private Color _glowColor = new Color(0.25f, 0.55f, 0.4f);

    [Header("Lúc Intensify() được gọi (VD từ WellDeathSequence lúc bắt đầu jumpscare)")]
    [SerializeField] private float _intensifiedMultiplier = 2.5f;
    [SerializeField] private Color _intensifiedColor = new Color(0.1f, 0.9f, 0.5f);

    private bool _activated = false;
    private bool _intensified = false;
    private float _noiseOffset;

    private void Awake()
    {
        if (_glowLight == null) _glowLight = GetComponent<Light>();
        // Mỗi giếng/đèn (nếu có nhiều object dùng chung script) nhấp nháy LỆCH PHA nhau -- tránh đồng bộ
        // giả tạo nếu chẳng may đặt nhiều nguồn sáng cùng lúc trong tầm nhìn.
        _noiseOffset = Random.Range(0f, 100f);

        if (_glowLight != null) _glowLight.intensity = _dormantIntensity;
    }

    private void Update()
    {
        if (_glowLight == null) return;

        if (!_activated)
        {
            _glowLight.intensity = _dormantIntensity;
            return;
        }

        float noise = Mathf.PerlinNoise(Time.time * _flickerSpeed, _noiseOffset);
        float baseIntensity = Mathf.Lerp(_minIntensity, _maxIntensity, noise);

        _glowLight.intensity = _intensified ? baseIntensity * _intensifiedMultiplier : baseIntensity;
        _glowLight.color = _intensified ? _intensifiedColor : _glowColor;
    }

    /// <summary>Chuyển từ dormant sang nhấp nháy nền -- gọi từ WalkToWellCutscene.OnArrivedAtWell
    /// (Inspector), KHÔNG phải gaze tự do -- giếng chỉ "sống dậy" đúng lúc cutscene kết Chapter 1 diễn ra.</summary>
    public void Activate() => _activated = true;

    /// <summary>Gọi lúc bắt đầu jumpscare thật (VD từ WellDeathSequence) -- sáng rực + đổi màu, KHÔNG tự tắt
    /// lại (giếng "đã lộ" thì cứ để vậy tới khi Player chết/scene reload, không cần logic tắt riêng).</summary>
    public void Intensify()
    {
        _activated = true; // phòng trường hợp gọi Intensify() trực tiếp mà quên gọi Activate() trước
        _intensified = true;
    }
}
