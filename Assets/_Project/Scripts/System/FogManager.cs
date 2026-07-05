using UnityEngine;

/// <summary>
/// Gắn vào GameObject "LightingManager" trong scene để chỉnh Fog (RenderSettings, theo khoảng cách camera)
/// VÀ Skybox trực tiếp qua Inspector — đổi giá trị là thấy ngay trong Scene view/Game view, không cần vào
/// Window > Rendering > Lighting hay chạy script mỗi lần. Chạy cả trong Edit Mode (ExecuteAlways) lẫn Play Mode.
/// </summary>
[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    [Header("Fog cổ điển (RenderSettings) — theo khoảng cách camera")]
    public bool fogEnabled = true;
    public Color fogColor = new Color(0.06f, 0.07f, 0.09f, 1f);
    public FogMode fogMode = FogMode.ExponentialSquared;
    [Tooltip("Chỉ dùng khi Fog Mode = Exponential hoặc Exponential Squared")]
    public float fogDensity = 0.07f;
    [Tooltip("Chỉ dùng khi Fog Mode = Linear")]
    public float linearFogStart = 0f;
    public float linearFogEnd = 300f;

    [Header("Skybox — công tắc chuyển qua lại giữa 2 bản")]
    [Tooltip("Bật = dùng Dark Night Skybox (tối, đúng tone horror game thật). Tắt = dùng Bright Skybox (sáng, dễ edit/test map).")]
    public bool useDarkNightSkybox = false;
    [Tooltip("Skybox sáng bình thường — dùng lúc edit/test cho dễ nhìn map")]
    public Material brightSkybox;
    [Tooltip("Exposure riêng cho Bright Skybox — không ảnh hưởng Dark Night Skybox")]
    public float brightSkyboxExposure = 2f;
    [Tooltip("Tint riêng cho Bright Skybox")]
    public Color brightSkyboxTint = Color.gray;

    [Space]
    [Tooltip("Skybox tối đêm — đúng tone horror, dùng cho game thật")]
    public Material darkNightSkybox;
    [Tooltip("Exposure riêng cho Dark Night Skybox — không ảnh hưởng Bright Skybox")]
    public float darkSkyboxExposure = 0.1f;
    [Tooltip("Tint riêng cho Dark Night Skybox")]
    public Color darkSkyboxTint = new Color(0.48f, 0.48f, 0.5f, 0.5f);

    private void OnEnable() => Apply();
    private void OnValidate() => Apply();

    private void Apply()
    {
        // Play Mode LUÔN bật fog (đúng game thật), Edit Mode theo đúng checkbox Jok để (tắt được để edit map cho dễ nhìn).
        RenderSettings.fog = Application.isPlaying || fogEnabled;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = linearFogStart;
        RenderSettings.fogEndDistance = linearFogEnd;

        // Play Mode LUÔN dùng bản tối đêm (đúng game thật), Edit Mode theo đúng checkbox Jok để (mặc định sáng cho dễ edit).
        bool effectiveDark = Application.isPlaying || useDarkNightSkybox;
        var skyboxMaterial = effectiveDark ? darkNightSkybox : brightSkybox;
        float exposure = effectiveDark ? darkSkyboxExposure : brightSkyboxExposure;
        Color tint = effectiveDark ? darkSkyboxTint : brightSkyboxTint;

        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            if (skyboxMaterial.HasProperty("_Exposure"))
                skyboxMaterial.SetFloat("_Exposure", exposure);
            if (skyboxMaterial.HasProperty("_Tint"))
                skyboxMaterial.SetColor("_Tint", tint);
        }
    }
}
