using UnityEngine;

/// <summary>
/// Gắn vào GameObject "LightingManager" trong scene — nơi DUY NHẤT để chỉnh toàn bộ ánh sáng thế giới:
/// Ambient (RenderSettings), Directional Light (mặt trăng), Fog, và Skybox — đổi giá trị là thấy ngay
/// trong Scene view/Game view, không cần vào Window > Rendering > Lighting hay chỉnh rải rác nhiều chỗ.
/// Chạy cả trong Edit Mode (ExecuteAlways) lẫn Play Mode.
/// </summary>
[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    [Header("Ambient (RenderSettings) — ánh sáng môi trường gián tiếp, phủ đều mọi góc khuất")]
    [Tooltip("Nhân với màu Ambient Sky/Equator/Ground bên dưới (hoặc màu Skybox nếu Ambient Mode = Skybox)")]
    public float ambientIntensity = 1f;
    public UnityEngine.Rendering.AmbientMode ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    [Tooltip("Chỉ có tác dụng khi Ambient Mode = Trilight hoặc Flat")]
    public Color ambientSkyColor = new Color(0.212f, 0.227f, 0.259f, 1f);
    public Color ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f, 1f);
    public Color ambientGroundColor = new Color(0.047f, 0.043f, 0.035f, 1f);
    [Tooltip("Độ rõ của phản chiếu môi trường (từ Skybox) lên vật liệu bóng/kim loại")]
    public float reflectionIntensity = 0.3f;

    [Header("Directional Light (Moonlight) — nguồn sáng chính chiếu lên toàn bộ hình học")]
    [Tooltip("Để trống sẽ tự tìm Directional Light đầu tiên trong scene lúc Enable")]
    public Light directionalLight;
    public float directionalLightIntensity = 0.6f;
    public Color directionalLightColor = new Color(0.3f, 0.35f, 0.5f, 1f);

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

    private void OnEnable()
    {
        if (directionalLight == null)
        {
            foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { directionalLight = l; break; }
        }
        Apply();
    }
    private void OnValidate() => Apply();

    private void Apply()
    {
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.reflectionIntensity = reflectionIntensity;

        if (directionalLight != null)
        {
            directionalLight.intensity = directionalLightIntensity;
            directionalLight.color = directionalLightColor;
        }

        // Play Mode LUÔN bật fog (đúng game thật) bất kể checkbox — checkbox chỉ còn tác dụng lúc Edit Mode
        // để tắt tạm cho dễ nhìn map lúc dựng scene.
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
