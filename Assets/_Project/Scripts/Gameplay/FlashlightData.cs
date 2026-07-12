using UnityEngine;

/// <summary>
/// ScriptableObject chứa toàn bộ thông số đèn pin.
/// Tạo: Assets → chuột phải → Create → Flashlight → Flashlight Data
/// </summary>
[CreateAssetMenu(fileName = "FlashlightData", menuName = "Flashlight/Flashlight Data")]
public class FlashlightData : ScriptableObject
{
    [Header("Battery")]
    [Tooltip("Tốc độ hao pin mỗi giây khi đèn bật")]
    public float drainRate = 0.005f;
    [Range(0f, 1f)] [Tooltip("Mức pin lúc bắt đầu game (1 = đầy)")]
    public float startBatteryLevel = 1f;

    [Header("Light")]
    [Tooltip("Độ sáng tối đa")]
    public float maxIntensity    = 1.425f;
    [Tooltip("Góc chùm sáng ngoài — càng lớn tỏa càng rộng")]
    public float spotAngle       = 90f;
    [Tooltip("Góc chùm sáng trong — gần bằng spotAngle thì viền càng mờ")]
    public float innerSpotAngle  = 80f;
    [Tooltip("Tầm chiếu xa")]
    public float range           = 30f;
    [Tooltip("Màu ánh sáng đèn pin (vàng ấm/trắng lạnh...)")]
    public Color lightColor      = new Color(1f, 0.97f, 0.88f);

    [Header("Shadow")]
    public LightShadows shadowType   = LightShadows.Soft;
    [Range(0f, 1f)] public float shadowStrength = 1f;
    [Tooltip("Bias chống shadow acne — để quá thấp dễ bị rỗ, quá cao dễ bị tách bóng khỏi vật")]
    public float shadowBias       = 0.05f;
    public float shadowNormalBias = 0.4f;

    [Header("Shake to Recover")]
    [Tooltip("Lượng pin phục hồi mỗi lần lắc (0.1 = 10%)")]
    public float shakeRecoverAmount  = 0.1f;
    [Tooltip("Thời gian chờ giữa 2 lần lắc (giây)")]
    public float shakeCooldown       = 5f;
    [Tooltip("Tạm dừng drain bao lâu sau khi lắc (giây)")]
    public float shakeDrainPause     = 3f;
    [Tooltip("Chỉ lắc được khi pin dưới mức này (0.5 = 50%)")]
    public float shakeBatteryThresh  = 0.5f;

    [Header("Flicker Thresholds")]
    [Tooltip("Dưới mức này bắt đầu nhấp nháy nhẹ")]
    public float flickerMediumThresh   = 0.5f;
    [Tooltip("Dưới mức này nhấp nháy mạnh")]
    public float flickerLowThresh      = 0.3f;
    [Tooltip("Dưới mức này rất yếu, nhấp nháy liên tục")]
    public float flickerCriticalThresh = 0.15f;

    [Header("Độ sáng theo mốc pin (nhân với Max Intensity)")]
    [Tooltip("Mốc ổn định (trên Medium Thresh) — sáng lerp từ giá trị này lên 1.0 (full) theo % pin còn lại")]
    [Range(0f, 1f)] public float stableIntensityMultMin = 0.7f;
    [Range(0f, 1f)] public float mediumIntensityMult    = 0.65f;
    [Range(0f, 1f)] public float lowIntensityMult       = 0.4f;
    [Range(0f, 1f)] public float criticalIntensityMult  = 0.2f;

    [Header("Flicker ngẫu nhiên — xác suất mỗi frame & kiểu chớp tắt")]
    [Tooltip("Xác suất mỗi frame kích hoạt chớp (ở mốc Medium)")]
    public float mediumFlickerChance   = 0.003f;
    public int   mediumFlickerCount    = 2;
    public float mediumFlickerDuration = 0.05f;

    [Tooltip("Xác suất mỗi frame kích hoạt chớp (ở mốc Low)")]
    public float lowFlickerChance      = 0.005f;
    public int   lowFlickerCount       = 1;
    public float lowFlickerDuration    = 0.15f;

    [Tooltip("Xác suất mỗi frame kích hoạt chớp (ở mốc Critical)")]
    public float criticalFlickerChance   = 0.01f;
    public int   criticalFlickerCount    = 2;
    public float criticalFlickerDuration = 0.2f;

    [Header("Model — giơ lên/hạ xuống khi bật/tắt")]
    public Vector3 raisedLocalPos  = new Vector3(0.35f, -0.25f, 0.5f);
    public Vector3 loweredLocalPos = new Vector3(0.35f, -0.7f, 0.5f);
    [Tooltip("Thời gian lerp giơ lên/hạ xuống (giây)")]
    public float raiseLowerDuration = 0.25f;

    [Header("Hội tụ tia sáng vào giữa màn hình")]
    [Tooltip("Chỉ dùng khi raycast phía trước không trúng gì (đứng giữa khoảng trống) — bình thường tia đèn tự hội tụ vào đúng điểm player đang nhìn")]
    public float aimConvergeDistance = 3f;
    [Tooltip("Độ mượt khi camera lắc (head bob, mouse look) — càng NHỎ càng mượt/trễ, càng LỚN càng bám sát/nhạy. 0 = tắt smoothing, bám cứng theo camera")]
    public float aimSmoothSpeed = 10f;
}