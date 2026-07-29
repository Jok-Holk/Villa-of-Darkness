using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _voiceSource;
    [SerializeField] private float _bgmVolume = 1f;
    private float _sfxVolumeScale = 1f;
    private float _voiceVolumeScale = 1f;

    // Lời thoại nhân vật (dialogue) bị nhỏ so với các âm thanh khác -- nhân "cứng" x2, TÁCH RIÊNG khỏi
    // _voiceVolumeScale (thanh trượt Settings 0-1 người chơi tự chỉnh). Set volume Settings vẫn hoạt động
    // như tỉ lệ phần trăm CỦA mức đã nhân 2 này, không phải thay thế nó.
    private const float VoiceVolumeBoost = 2f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; transform.SetParent(null); DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        // --- PHẦN FIX MỚI Ở ĐÂY ---
        if (_bgmSource == null) _bgmSource = GetComponent<AudioSource>();

        // Nếu designer quên kéo _sfxSource, dùng tạm _bgmSource để phát tiếng
        if (_sfxSource == null) _sfxSource = _bgmSource;
        // --------------------------

        // Voice cần AudioSource RIÊNG (không PlayOneShot chung với SFX) -- dialogue có thể cần Stop() giữa
        // chừng lúc CloseDialogue()/skip nhanh qua nhiều dòng, PlayOneShot không cắt được tiếng đang phát.
        if (_voiceSource == null)
        {
            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceSource.loop = false;
            _voiceSource.spatialBlend = 0f;
        }
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
            _sfxSource.PlayOneShot(clip, _sfxVolumeScale);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null && _sfxSource != null)
            // Clamp(0,4) thay vì Clamp01 -- 1 vài SFX (VD jumpscare gương/ma bắt) cố tình cần volume > 1 để
            // "to hơn bình thường" (x2, x3...), Clamp01 cũ vô tình chặn cứng ở 1.0 khiến mọi lời gọi "to
            // hơn" đều vô nghĩa. Trần nâng lên 4x (trước 3x vẫn chưa đủ to theo phản hồi thực tế).
            _sfxSource.PlayOneShot(clip, Mathf.Clamp(volume, 0f, 4f) * _sfxVolumeScale);
    }

    public void SetSFXVolume(float v)
    {
        _sfxVolumeScale = Mathf.Clamp01(v);
    }

    // Phát giọng lồng tiếng dialogue (DialogueLine.voiceClip) -- dùng Play() qua AudioSource riêng thay vì
    // PlayOneShot vì cần Stop() được giữa chừng (đóng hộp thoại/skip nhanh qua dòng đang có giọng chưa dứt).
    public void PlayVoice(AudioClip clip)
    {
        if (clip == null || _voiceSource == null) return;
        _voiceSource.volume = _voiceVolumeScale * VoiceVolumeBoost;
        _voiceSource.clip = clip;
        _voiceSource.Play();
    }

    public void StopVoice()
    {
        if (_voiceSource != null) _voiceSource.Stop();
    }

    public void SetVoiceVolume(float v)
    {
        _voiceVolumeScale = Mathf.Clamp01(v);
        if (_voiceSource != null) _voiceSource.volume = _voiceVolumeScale;
    }

    // Cho các hệ thống tự quản lý AudioSource riêng (VD RandomTensionStinger cần tự fade volume) đọc
    // để nhân vào, chứ không qua PlaySFX/PlayOneShot -- đảm bảo vẫn tôn trọng thanh trượt SFX trong Settings.
    public float SfxVolumeScale => _sfxVolumeScale;
}