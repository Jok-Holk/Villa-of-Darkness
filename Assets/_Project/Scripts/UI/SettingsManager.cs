using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    public void SetVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("Volume", value);
    }
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("Music", value);
    }
    public void SetVoiceVolume(float value)
    {
        audioMixer.SetFloat("VoiceVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("VoiceVolume", value);
    }
    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("SFX", value);
    }
    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
    }
    public float GetSensitivity() => PlayerPrefs.GetFloat("Sensitivity", 1f);

    void LoadSettings()
    {
        SetVolume(PlayerPrefs.GetFloat("Volume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("Music", 1f));
        SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFX", 1f));
        SetSensitivity(PlayerPrefs.GetFloat("Sensitivity", 1f));
    }
}