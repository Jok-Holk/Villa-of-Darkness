using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private float _bgmVolume = 1f;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    public void PlaySFX(AudioClip clip) { if (clip == null) return; }
    public void SetBGMVolume(float v) { _bgmVolume = Mathf.Clamp(v, 0f, 1f); }
}
