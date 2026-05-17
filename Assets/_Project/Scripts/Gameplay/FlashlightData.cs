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

    [Header("Light")]
    [Tooltip("Độ sáng tối đa")]
    public float maxIntensity    = 1.425f;
    [Tooltip("Góc chùm sáng ngoài — càng lớn tỏa càng rộng")]
    public float spotAngle       = 90f;
    [Tooltip("Góc chùm sáng trong — gần bằng spotAngle thì viền càng mờ")]
    public float innerSpotAngle  = 80f;
    [Tooltip("Tầm chiếu xa")]
    public float range           = 30f;

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
}