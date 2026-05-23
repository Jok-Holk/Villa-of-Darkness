using UnityEngine;
using System.Collections;
#pragma warning disable CS0414

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class AmbientZone : MonoBehaviour
{
    [SerializeField] private float  _targetVolume = 0.8f;
    [SerializeField] private float  _fadeDuration = 1.5f;
    [SerializeField] private string _targetTag    = "Player";

    private AudioSource _audioSource;
    private bool        _isActive = false;
    public  bool IsActive => _isActive;

    private void Awake()
    {
        _audioSource        = GetComponent<AudioSource>();
        _audioSource.volume = 0f;
        _audioSource.loop   = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_targetTag)) StartCoroutine(FadeIn());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_targetTag)) StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        _isActive = true;

        if (!_audioSource.isPlaying) _audioSource.Play();

        float startVolume = _audioSource.volume;
        float elapsed     = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed              += Time.deltaTime;
            _audioSource.volume  = Mathf.Lerp(startVolume, _targetVolume, elapsed / _fadeDuration);
            yield return null;
        }

        _audioSource.volume = _targetVolume;
    }

    public IEnumerator FadeOut()
    {
        float startVolume = _audioSource.volume;
        float elapsed     = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed             += Time.deltaTime;
            _audioSource.volume  = Mathf.Lerp(startVolume, 0f, elapsed / _fadeDuration);
            yield return null;
        }

        _audioSource.volume = 0f;
        _audioSource.Stop();
        _isActive = false;
    }
}