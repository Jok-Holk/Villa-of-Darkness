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

    private Vector3 _startLocalPos;
    private CharacterController _cc;
    private FootstepSystem _footstepSystem;
    private float _tBob;
    private float _smoothedSpeed; // DUY NHẤT lái cả pha (_tBob) lẫn biên độ — không còn state tách rời
    private float _prevAbsSin;
    private bool  _prevRising;

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
    }

    private void LateUpdate()
    {
        if (_cc == null) return;

        Vector3 horizontalVel = new Vector3(_cc.velocity.x, 0f, _cc.velocity.z);
        float   rawSpeed      = _cc.isGrounded ? horizontalVel.magnitude : 0f;

        // 1 biến làm mượt duy nhất — thay thế cả _blend lẫn cờ _isMoving của bản trước.
        _smoothedSpeed = Mathf.MoveTowards(_smoothedSpeed, rawSpeed, SmoothRate * Time.deltaTime * Mathf.Max(rawSpeed, 1f));

        bool active = _smoothedSpeed > MinSpeedThreshold;
        // Biên độ giảm dần mượt về 0 quanh ngưỡng thay vì cắt cứng — tránh giật khi lởn vởn sát ngưỡng.
        float ampScale = Mathf.Clamp01(_smoothedSpeed / Mathf.Max(MinSpeedThreshold * 2f, 0.001f));

        // Pha tăng theo CHÍNH _smoothedSpeed (không phải speed thô) — cùng 1 biến lái cả 2 thứ nên
        // không bao giờ lệch pha so với biên độ nữa.
        _tBob += Time.deltaTime * _smoothedSpeed;

        // Bọc pha về [0, 2π/BobFrequency) — tránh mất độ chính xác float sau phiên chơi dài.
        float period = TwoPi / BobFrequency;
        if (_tBob > period) _tBob -= period * Mathf.Floor(_tBob / period);

        float phase  = _tBob * BobFrequency;
        float rawSin = Mathf.Sin(phase);
        float absSin = Mathf.Abs(rawSin);

        bool rising = absSin > _prevAbsSin;
        if (_prevRising && !rising && active)
            _footstepSystem?.PlayFootstepNow();
        _prevRising = rising;
        _prevAbsSin = absSin;

        Vector3 pos = Vector3.zero;
        pos.y = -absSin * BobAmplitude * ampScale;
        pos.x =  Mathf.Cos(phase) * BobAmplitude * 0.5f * ampScale;

        transform.localPosition = _startLocalPos + pos;
    }
}
