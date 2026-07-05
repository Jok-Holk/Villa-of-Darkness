using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    // Âm lượng điều khiển trực tiếp qua AudioListener/AudioManager thay vì AudioMixer —
    // 3 mixer riêng lẻ (MainMixer/Music/SFX) trước đó lệch tên tham số với code nên slider không thực sự đổi được gì.
    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("Volume", value);
    }
    public void SetMusicVolume(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        PlayerPrefs.SetFloat("Music", value);
    }
    public void SetVoiceVolume(float value)
    {
        // Game hiện chưa có audio thoại (dialogue chỉ hiện text) — lưu giá trị để
        // hệ thống voice line sau này đọc, chưa có gì để áp dụng ngay bây giờ.
        PlayerPrefs.SetFloat("VoiceVolume", value);
    }
    public void SetSFXVolume(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFX", value);
    }
    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
    }
    public float GetSensitivity() => PlayerPrefs.GetFloat("Sensitivity", 1f);

    // Graphics quality: 3 mức Low/Medium/High tương ứng đúng index 0/1/2 trong ProjectSettings QualitySettings
    // (mỗi mức có RenderPipelineAsset riêng — chênh lệch thật: RenderScale 0.5/0.85/1.5, Shadow Resolution 512/1024/2048,
    // Shadow Distance 15/30/45 — High = 3 lần Low ở RenderScale và ShadowDistance).
    public void SetGraphicsQualityLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 2);
        QualitySettings.SetQualityLevel(level, true);
        PlayerPrefs.SetInt("GraphicsQuality", level);
    }
    public int GetGraphicsQualityLevel() => PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());

    /// <summary>
    /// Đoán cấu hình máy dựa trên VRAM + RAM (heuristic đơn giản, không phải benchmark thật).
    /// </summary>
    public int AutoDetectQualityLevel()
    {
        int vram = SystemInfo.graphicsMemorySize;   // MB
        int ram = SystemInfo.systemMemorySize;       // MB
        int level;
        if (vram >= 4096 && ram >= 16384) level = 2;      // High
        else if (vram >= 2048 && ram >= 8192) level = 1;  // Medium
        else level = 0;                                    // Low
        string[] names = { "Low", "Medium", "High" };
        Debug.Log($"[SettingsManager] Auto-detect cấu hình máy: VRAM={vram}MB, RAM={ram}MB, GPU={SystemInfo.graphicsDeviceName} -> chọn mức {names[level]}.");
        return level;
    }

    // Lưu theo width/height thật thay vì index vào Screen.resolutions — SettingsUI lọc bớt độ phân giải
    // vô lý (320x200...) nên index trong dropdown không còn khớp trực tiếp với index gốc của Screen.resolutions nữa.
    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("ResolutionWidth", width);
        PlayerPrefs.SetInt("ResolutionHeight", height);
    }
    public bool HasSavedResolution() => PlayerPrefs.HasKey("ResolutionWidth");
    public int GetSavedResolutionWidth() => PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
    public int GetSavedResolutionHeight() => PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);

    void LoadSettings()
    {
        SetVolume(PlayerPrefs.GetFloat("Volume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("Music", 1f));
        SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFX", 1f));
        SetSensitivity(PlayerPrefs.GetFloat("Sensitivity", 1f));

        // Lần đầu chạy (chưa có PlayerPrefs) thì auto-detect cấu hình máy để chọn mức mặc định hợp lý,
        // thay vì luôn cứng vào mức Low đã set sẵn trong ProjectSettings. Từ lần sau tôn trọng lựa chọn thủ công của người chơi.
        int level = PlayerPrefs.HasKey("GraphicsQuality")
            ? PlayerPrefs.GetInt("GraphicsQuality")
            : AutoDetectQualityLevel();
        SetGraphicsQualityLevel(level);

        if (HasSavedResolution())
            SetResolution(GetSavedResolutionWidth(), GetSavedResolutionHeight());
    }
}
