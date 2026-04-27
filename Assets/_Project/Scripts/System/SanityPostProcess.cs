using UnityEngine;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; // Cần thiết cho Post-processing

namespace Assets._Project.Scripts.System
{
    public class SanityPostProcess : MonoBehaviour
    {
        [SerializeField] private SanitySystem _sanitySystem;
        [SerializeField] private Volume _volume;

        private FilmGrain _grain;
        private Vignette _vignette;
        private ChromaticAberration _chromatic;

        private float _targetGrain, _targetVignette, _targetChromatic;
        [SerializeField] private float _lerpSpeed = 2f;

        private void Awake()
        {
            if (_volume == null) _volume = GetComponent<Volume>();
            if (_volume != null && _volume.profile != null)
            {
                _volume.profile.TryGet(out _grain);
                _volume.profile.TryGet(out _vignette);
                _volume.profile.TryGet(out _chromatic);
            }

            if (_sanitySystem != null)
            {
                _sanitySystem.OnLevelChanged.AddListener(UpdatePostProcessTargets);
                UpdatePostProcessTargets(_sanitySystem.GetLevel());
            }
        }

        private void UpdatePostProcessTargets(SanitySystem.SanityLevel level)
        {
            switch (level)
            {
                case SanitySystem.SanityLevel.High: SetTargets(0f, 0.2f, 0f); break;
                case SanitySystem.SanityLevel.Medium: SetTargets(0.3f, 0.35f, 0.1f); break;
                case SanitySystem.SanityLevel.Low: SetTargets(0.6f, 0.5f, 0.3f); break;
                case SanitySystem.SanityLevel.Critical: SetTargets(1.0f, 0.7f, 0.8f); break;
            }
        }

        private void SetTargets(float g, float v, float c) { _targetGrain = g; _targetVignette = v; _targetChromatic = c; }

        private void Update()
        {
            if (_grain != null) _grain.intensity.value = Mathf.Lerp(_grain.intensity.value, _targetGrain, Time.deltaTime * _lerpSpeed);
            if (_vignette != null) _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, _targetVignette, Time.deltaTime * _lerpSpeed);
            if (_chromatic != null) _chromatic.intensity.value = Mathf.Lerp(_chromatic.intensity.value, _targetChromatic, Time.deltaTime * _lerpSpeed);
        }
    }
}