using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders (Âm thanh)")]
    public Slider sensitivitySlider;
    public Slider musicSlider;
    public Slider volumeSlider;
    public Slider voiceVolumeSlider;
    public Slider sfxSlider;

    [Header("Đồ hoạ")]
    public Button lowQualityButton;
    public Button mediumQualityButton;
    public Button highQualityButton;
    public TMP_Dropdown resolutionDropdown;

    [Header("Apply / Xác nhận thoát")]
    [Tooltip("Panel hỏi xác nhận khi bấm Back mà còn thay đổi chưa Apply")]
    public GameObject confirmDiscardPanel;

    private List<Resolution> _resolutions;
    private bool _isDirty;

    private struct SettingsSnapshot
    {
        public float sensitivity, music, volume, voiceVolume, sfx;
        public int qualityLevel, resWidth, resHeight;
    }
    private SettingsSnapshot _lastApplied;

    // OnEnable (không phải Start) vì SettingPanel chỉ SetActive bật/tắt chứ không bị huỷ —
    // phải nạp lại giá trị mỗi lần mở lại Settings, đặc biệt sau khi Back-không-Apply đã revert.
    void OnEnable()
    {
        CaptureSnapshot();
        ApplySnapshotToUI(_lastApplied);

        UpdateGraphicsQualityButtons();
        SetupResolutionDropdown();

        _isDirty = false;
        if (confirmDiscardPanel != null) confirmDiscardPanel.SetActive(false);
    }

    private void CaptureSnapshot()
    {
        _lastApplied = new SettingsSnapshot
        {
            sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f),
            music = PlayerPrefs.GetFloat("Music", 1f),
            volume = PlayerPrefs.GetFloat("Volume", 1f),
            voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f),
            sfx = PlayerPrefs.GetFloat("SFX", 1f),
            qualityLevel = SettingsManager.Instance != null ? SettingsManager.Instance.GetGraphicsQualityLevel() : 0,
            resWidth = SettingsManager.Instance != null ? SettingsManager.Instance.GetSavedResolutionWidth() : Screen.currentResolution.width,
            resHeight = SettingsManager.Instance != null ? SettingsManager.Instance.GetSavedResolutionHeight() : Screen.currentResolution.height,
        };
    }

    private void ApplySnapshotToUI(SettingsSnapshot s)
    {
        sensitivitySlider.value = s.sensitivity;
        musicSlider.value = s.music;
        volumeSlider.value = s.volume;
        voiceVolumeSlider.value = s.voiceVolume;
        sfxSlider.value = s.sfx;
    }

    /// <summary>Đưa engine + PlayerPrefs về đúng giá trị lúc mở Settings (hoặc lúc Apply gần nhất) — dùng khi Back mà không Apply.</summary>
    private void RevertToSnapshot()
    {
        var s = _lastApplied;
        SettingsManager.Instance?.SetSensitivity(s.sensitivity);
        SettingsManager.Instance?.SetMusicVolume(s.music);
        SettingsManager.Instance?.SetVolume(s.volume);
        SettingsManager.Instance?.SetVoiceVolume(s.voiceVolume);
        SettingsManager.Instance?.SetSFXVolume(s.sfx);
        SettingsManager.Instance?.SetGraphicsQualityLevel(s.qualityLevel);
        SettingsManager.Instance?.SetResolution(s.resWidth, s.resHeight);
    }

    private static readonly Color QualityButtonActive = new Color(0.75f, 0.15f, 0.15f, 1f);
    private static readonly Color QualityButtonInactive = new Color(0.10f, 0.06f, 0.06f, 0.85f);

    void UpdateGraphicsQualityButtons()
    {
        int level = SettingsManager.Instance != null ? SettingsManager.Instance.GetGraphicsQualityLevel() : 0;
        SetQualityButtonActive(lowQualityButton, level == 0);
        SetQualityButtonActive(mediumQualityButton, level == 1);
        SetQualityButtonActive(highQualityButton, level == 2);
    }

    private static void SetQualityButtonActive(Button button, bool active)
    {
        if (button == null) return;
        var colors = button.colors;
        colors.normalColor = active ? QualityButtonActive : QualityButtonInactive;
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
    }

    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        // Lọc bớt độ phân giải vô lý (320x200, 400x300...) — chỉ giữ độ phân giải đủ dùng thực tế.
        const int minWidth = 1024, minHeight = 600;
        _resolutions = new List<Resolution>();
        var seen = new HashSet<string>();
        foreach (var r in Screen.resolutions)
        {
            if (r.width < minWidth || r.height < minHeight) continue;
            string key = $"{r.width}x{r.height}";
            if (!seen.Add(key)) continue; // bỏ trùng (nhiều refresh rate cùng 1 độ phân giải)
            _resolutions.Add(r);
        }
        if (_resolutions.Count == 0) _resolutions.Add(Screen.currentResolution);

        int savedW = SettingsManager.Instance != null ? SettingsManager.Instance.GetSavedResolutionWidth() : Screen.currentResolution.width;
        int savedH = SettingsManager.Instance != null ? SettingsManager.Instance.GetSavedResolutionHeight() : Screen.currentResolution.height;

        var options = new List<string>();
        int currentIndex = 0;
        for (int i = 0; i < _resolutions.Count; i++)
        {
            var r = _resolutions[i];
            options.Add($"{r.width} x {r.height}");
            if (r.width == savedW && r.height == savedH) currentIndex = i;
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();
    }

    public void OnSensitivity(float v)   { SettingsManager.Instance?.SetSensitivity(v); MarkDirty(); }
    public void OnMusic(float v)         { SettingsManager.Instance?.SetMusicVolume(v); MarkDirty(); }
    public void OnVolume(float v)        { SettingsManager.Instance?.SetVolume(v); MarkDirty(); }
    public void OnVoiceVolume(float v)   { SettingsManager.Instance?.SetVoiceVolume(v); MarkDirty(); }
    public void OnSFX(float v)           { SettingsManager.Instance?.SetSFXVolume(v); MarkDirty(); }
    public void OnLowQuality()    { SettingsManager.Instance?.SetGraphicsQualityLevel(0); UpdateGraphicsQualityButtons(); MarkDirty(); }
    public void OnMediumQuality() { SettingsManager.Instance?.SetGraphicsQualityLevel(1); UpdateGraphicsQualityButtons(); MarkDirty(); }
    public void OnHighQuality()   { SettingsManager.Instance?.SetGraphicsQualityLevel(2); UpdateGraphicsQualityButtons(); MarkDirty(); }
    public void OnResolution(int index)
    {
        if (_resolutions == null || index < 0 || index >= _resolutions.Count) return;
        var r = _resolutions[index];
        SettingsManager.Instance?.SetResolution(r.width, r.height);
        MarkDirty();
    }

    private void MarkDirty() => _isDirty = true;

    /// <summary>Gắn vào nút Apply — chốt các thay đổi hiện tại thành trạng thái "đã lưu" mới (mốc revert cho lần Back sau).</summary>
    public void OnApply()
    {
        PlayerPrefs.Save();
        CaptureSnapshot();
        _isDirty = false;
        if (confirmDiscardPanel != null) confirmDiscardPanel.SetActive(false);
    }

    /// <summary>Gắn vào nút Back — nếu còn thay đổi chưa Apply thì hỏi xác nhận trước khi thực sự đóng Settings.</summary>
    public void OnBackPressed()
    {
        if (_isDirty && confirmDiscardPanel != null)
            confirmDiscardPanel.SetActive(true);
        else
            CloseSettingsNow();
    }

    /// <summary>Gắn vào nút "Back" trong popup xác nhận — huỷ mọi thay đổi chưa Apply, revert engine về mốc đã lưu gần nhất rồi thoát.</summary>
    public void ConfirmDiscardAndBack()
    {
        RevertToSnapshot();
        if (confirmDiscardPanel != null) confirmDiscardPanel.SetActive(false);
        CloseSettingsNow();
    }

    /// <summary>Gắn vào nút "Không" trong popup xác nhận — ở lại Settings.</summary>
    public void CancelDiscard()
    {
        if (confirmDiscardPanel != null) confirmDiscardPanel.SetActive(false);
    }

    // Panel này giờ dùng chung ở cả MainMenu lẫn PauseMenu (Chapter1). Ở Chapter1, SettingPanel được đặt
    // làm ANH EM (sibling) với PauseMenu (cùng con của Canvas) — không phải con cháu — nên
    // GetComponentInParent<PauseMenuUI>() không tìm ra được (chỉ tìm tổ tiên, không tìm anh em).
    // Dùng FindFirstObjectByType làm phương án 2 để không phụ thuộc vị trí đặt trong hierarchy.
    private void CloseSettingsNow()
    {
        var mainMenu = GetComponentInParent<MainMenuUI>(true);
        if (mainMenu != null) { mainMenu.CloseSettings(); return; }

        var pauseMenu = GetComponentInParent<PauseMenuUI>(true);
        if (pauseMenu != null) { pauseMenu.CloseSettings(); return; }

        Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include)?.CloseSettings();
    }
}
