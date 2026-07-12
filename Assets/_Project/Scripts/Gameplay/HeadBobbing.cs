using UnityEngine;

/// <summary>
/// Head bob "dằn xuống mỗi bước" (double-dip, kiểu FPS chuẩn) — pha tăng theo QUÃNG ĐƯỜNG ĐÃ ĐI
/// (tốc độ ĐÃ LÀM MƯỢT * deltaTime), không theo thời gian thực.
///
/// TOÁN HỌC:
///   phase = tBob * BobFrequency
///   pos.y = -|sin(phase)| * Amplitude   ← luôn dằn XUỐNG, không nảy lên trên baseline
///   pos.x =  cos(phase)    * Amplitude * 0.5   ← 1 lần lắc trái-phải trọn 1 sải chân (2 bước)
///   |sin(phase)| có 2 cực đại (2 "chạm đáy" = 2 bước trái/phải) mỗi chu kỳ 2π.
///
/// CHỈ 1 BIẾN LÀM MƯỢT DUY NHẤT — _smoothedSpeed (MoveTowards từ speed thật). Bản trước dùng 2 cơ
/// chế tách rời (_blend làm mượt biên độ + _tBob chỉ cộng dồn khi speed>0 nên "đứng khựng"/"chạy lại"
/// đột ngột theo cờ _isMoving) — 2 cái này desync nhau khi bấm-nhả WASD dồn dập, nhìn như 2 chuyển
/// động chồng lên nhau. Giờ MỘT biến duy nhất lái CẢ pha lẫn biên độ, không thể desync được nữa.
///
/// HeadbobSystem là nguồn nhịp duy nhất cho cả hình lẫn tiếng — tự phát hiện đúng lúc |sin(phase)|
/// đạt đỉnh (đáy bob) rồi gọi thẳng FootstepSystem.PlayFootstepNow().
///
/// CHỈ đụng localPosition, KHÔNG đụng rotation — PlayerController.Update() ghi đè hoàn toàn
/// _cameraTransform.localRotation mỗi frame cho mouse-look, script khác chạm rotation sẽ gây giật/snap.
///
/// SETUP: gắn lên Camera (con của Player, cùng gốc với CharacterController + FootstepSystem) — tự
/// GetComponentInParent cho cả 2, không cần wire tay gì trong Inspector.
/// </summary>
public class HeadbobSystem : MonoBehaviour
{
    [Header("Sóng bob")]
    [Tooltip("Tần số bob — 2 = mặc định, khớp gần đúng nhịp bước gốc")]
    [Range(0.5f, 6f)]
    public float BobFrequency = 2f;
    [Tooltip("Biên độ dằn xuống theo trục Y")]
    [Range(0.005f, 0.15f)]
    public float BobAmplitude = 0.08f;
    [Tooltip("Tốc độ làm mượt speed thật → speed dùng cho bob — càng thấp càng \"trễ\"/mượt, càng cao càng bám sát input")]
    [Range(1f, 20f)]
    public float SmoothRate = 6f;
    [Tooltip("Ngưỡng speed (đã làm mượt) tối thiểu để còn tính là đang bob — dưới ngưỡng này biên độ ép về 0 hẳn")]
    [Range(0f, 0.5f)]
    public float MinSpeedThreshold = 0.1f;

    [Header("Debug")]
    [Tooltip("Bật lên để in log velocity/isGrounded/smoothedSpeed mỗi 0.5s lúc Play — xem Console")]
    [SerializeField] private bool _debugLog = false;
    private float _debugLogTimer;

    private Vector3 _startLocalPos;
    private CharacterController _cc;
    private FootstepSystem _footstepSystem;
    private float _tBob;
    private float _smoothedSpeed; // DUY NHẤT lái cả pha (_tBob) lẫn biên độ — không còn state tách rời
    private float _prevAbsSin;
    private bool  _prevRising;
    private Vector3 _prevPosition;

    private const float TwoPi = Mathf.PI * 2f;

    private void Awake()
    {
        _startLocalPos   = transform.localPosition;
        _cc              = GetComponentInParent<CharacterController>();
        _footstepSystem  = GetComponentInParent<FootstepSystem>();
        if (_cc == null)
            Debug.LogWarning("[HeadbobSystem] Không tìm thấy CharacterController ở parent — head bob sẽ không chạy.");
        if (_footstepSystem == null)
            Debug.LogWarning("[HeadbobSystem] Không tìm thấy FootstepSystem ở parent — sẽ không có SFX bước chân.");
        if (_cc != null) _prevPosition = _cc.transform.position;
    }

    private void LateUpdate()
    {
        if (_cc == null) return;

        // ĐO tốc độ thật bằng ĐỘ DỜI VỊ TRÍ mỗi frame thay vì đọc _cc.velocity — log debug cho thấy
        // _cc.velocity đọc = 0 ở phần lớn frame dù đang đi liên tục (tBob vẫn tăng đều), tức bản thân
        // property này nhiễu/không đáng tin ở project này (đúng như comment cũ từng cảnh báo). Độ dời vị
        // trí thật (currentPos - prevPos) là con số CHẮC CHẮN đúng, không phụ thuộc cách CharacterController
        // tính velocity nội bộ — hết hẳn nguồn nhiễu gốc, không cần vá thêm bộ lọc/hệ số nào nữa.
        Vector3 currentPos = _cc.transform.position;
        Vector3 delta = currentPos - _prevPosition;
        _prevPosition = currentPos;
        Vector3 horizontalVel = new Vector3(delta.x, 0f, delta.z) / Mathf.Max(Time.deltaTime, 0.0001f);
        float   rawSpeed      = _cc.isGrounded ? horizontalVel.magnitude : 0f;

        // Vẫn giữ lọc bất đối xứng nhẹ (attack nhanh/release chậm) để hình bob không giật — giờ tín hiệu
        // đầu vào đã sạch nên không cần hệ số attack cực cao như lúc còn vá nhiễu _cc.velocity nữa.
        float rate = rawSpeed > _smoothedSpeed ? SmoothRate * 3f : SmoothRate;
        float smoothFactor = 1f - Mathf.Exp(-rate * Time.deltaTime);
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, rawSpeed, smoothFactor);

        bool  active   = _smoothedSpeed > MinSpeedThreshold;
        // Biên độ giảm dần mượt về 0 quanh ngưỡng thay vì cắt cứng — tránh giật khi lởn vởn sát ngưỡng.
        float ampScale = Mathf.Clamp01(_smoothedSpeed / Mathf.Max(MinSpeedThreshold * 2f, 0.001f));

        _tBob += Time.deltaTime * _smoothedSpeed;

        // Bọc pha về [0, 2π/BobFrequency) — tránh mất độ chính xác float sau phiên chơi dài.
        float period = TwoPi / BobFrequency;
        if (_tBob > period) _tBob -= period * Mathf.Floor(_tBob / period);

        float phase  = _tBob * BobFrequency;
        float rawSin = Mathf.Sin(phase);
        float absSin = Mathf.Abs(rawSin);

        bool rising = absSin > _prevAbsSin;
        if (_prevRising && !rising && active)
            _footstepSystem?.PlayFootstepNow(_smoothedSpeed); // truyền tốc độ thật để volume tự nội suy walk/run
        _prevRising = rising;
        _prevAbsSin = absSin;

        Vector3 pos = Vector3.zero;
        pos.y = -absSin * BobAmplitude * ampScale;
        pos.x =  Mathf.Cos(phase) * BobAmplitude * 0.5f * ampScale;

        transform.localPosition = _startLocalPos + pos;

        if (_debugLog)
        {
            _debugLogTimer += Time.deltaTime;
            if (_debugLogTimer >= 0.5f)
            {
                _debugLogTimer = 0f;
                Debug.Log($"[HeadBob Debug] cc.velocity={_cc.velocity} horizontalMag={horizontalVel.magnitude:F2} isGrounded={_cc.isGrounded} rawSpeed={rawSpeed:F2} smoothedSpeed={_smoothedSpeed:F2} tBob={_tBob:F2} active={active}");
            }
        }
    }
}
