using UnityEngine;

namespace Phase1.BuiThanhTan 
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private float _bgmVolume = 1f;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }
            
            if (_bgmSource == null) _bgmSource = GetComponent<AudioSource>();
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
            // Cập nhật trực tiếp vào AudioSource để pass Unit Test
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            // Sử dụng PlayOneShot để không ngắt quãng BGM
            if (_bgmSource != null) _bgmSource.PlayOneShot(clip); 
        }
    }
}