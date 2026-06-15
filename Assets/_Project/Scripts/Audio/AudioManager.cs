using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private float _bgmVolume = 1f; 

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        
        // --- PHẦN FIX MỚI Ở ĐÂY ---
        if (_bgmSource == null) _bgmSource = GetComponent<AudioSource>();
        
        // Nếu designer quên kéo _sfxSource, dùng tạm _bgmSource để phát tiếng
        if (_sfxSource == null) _sfxSource = _bgmSource; 
        // --------------------------
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || _bgmSource == null) return;
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    public void StopBGM()
    {
        if (_bgmSource != null) _bgmSource.Stop();
    }

    public void SetBGMVolume(float v)
    {
        _bgmVolume = Mathf.Clamp01(v);
        if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}