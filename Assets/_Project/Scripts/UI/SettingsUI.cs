using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sensitivitySlider;
    public Slider musicSlider;
    public Slider volumeSlider;
    public Slider voiceVolumeSlider;
    public Slider sfxSlider;

    void Start()
    {
        sensitivitySlider.value  = PlayerPrefs.GetFloat("Sensitivity", 1f);
        musicSlider.value        = PlayerPrefs.GetFloat("Music", 1f);
        volumeSlider.value       = PlayerPrefs.GetFloat("Volume", 1f);
        voiceVolumeSlider.value  = PlayerPrefs.GetFloat("VoiceVolume", 1f);
        sfxSlider.value          = PlayerPrefs.GetFloat("SFX", 1f);
    }

    public void OnSensitivity(float v)   => SettingsManager.Instance?.SetSensitivity(v);
    public void OnMusic(float v)         => SettingsManager.Instance?.SetMusicVolume(v);
    public void OnVolume(float v)        => SettingsManager.Instance?.SetVolume(v);
    public void OnVoiceVolume(float v)   => SettingsManager.Instance?.SetVoiceVolume(v);
    public void OnSFX(float v)           => SettingsManager.Instance?.SetSFXVolume(v);
}