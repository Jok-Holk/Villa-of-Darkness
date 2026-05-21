using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanityPostProcess : MonoBehaviour
{
    [SerializeField] private SanitySystem _sanitySystem;
    [SerializeField] private SanityData _data; // Kéo file ScriptableObject vào đây
    [SerializeField] private Volume _volume;
    [SerializeField] private float _lerpSpeed = 2f;

    private FilmGrain _grain;
    private ChromaticAberration _chromatic;
    private ColorAdjustments _colorAdj;
    private Vignette _vignette;
    private LensDistortion _lensDistort;

    private int _currentIndex = 0;

    private void Awake()
    {
        _volume.profile.TryGet(out _grain);
        _volume.profile.TryGet(out _chromatic);
        _volume.profile.TryGet(out _colorAdj);
        _volume.profile.TryGet(out _vignette);
        _volume.profile.TryGet(out _lensDistort);

        if (_sanitySystem != null)
            _sanitySystem.OnLevelChanged.AddListener(OnLevelChanged);
    }

    private void OnLevelChanged(SanitySystem.SanityLevel level)
    {
        // Chuyển đổi Enum cũ sang index (0, 1, 2, 3...) để đọc Array trong Data
        _currentIndex = (int)level; 
    }

    private void Update()
    {
        if (_data == null || _data.levels.Length <= _currentIndex) return;

        var settings = _data.levels[_currentIndex];
        float dt = Time.deltaTime * _lerpSpeed;

        if (_grain != null) _grain.intensity.value = Mathf.Lerp(_grain.intensity.value, settings.grain, dt);
        if (_chromatic != null) _chromatic.intensity.value = Mathf.Lerp(_chromatic.intensity.value, settings.chromatic, dt);
        if (_colorAdj != null)
        {
            _colorAdj.saturation.value = Mathf.Lerp(_colorAdj.saturation.value, settings.saturation, dt);
            _colorAdj.postExposure.value = Mathf.Lerp(_colorAdj.postExposure.value, settings.exposure, dt);
        }
        if (_vignette != null) _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, settings.vignette, dt);
        if (_lensDistort != null) _lensDistort.intensity.value = Mathf.Lerp(_lensDistort.intensity.value, settings.lensDistort, dt);
    }
}