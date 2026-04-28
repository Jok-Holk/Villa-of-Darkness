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
        // TODO: if (other.CompareTag(_targetTag)) StartCoroutine(FadeIn());
        throw new System.NotImplementedException();
    }

    private void OnTriggerExit(Collider other)
    {
        // TODO: if (other.CompareTag(_targetTag)) StartCoroutine(FadeOut());
        throw new System.NotImplementedException();
    }

    public IEnumerator FadeIn()
    {
        // TODO: _isActive = true
        //       dùng vòng lặp tăng _audioSource.volume từ giá trị hiện tại lên _targetVolume
        //       trong _fadeDuration giây, yield return null mỗi frame
        throw new System.NotImplementedException();
    }

    public IEnumerator FadeOut()
    {
        // TODO: dùng vòng lặp giảm _audioSource.volume từ giá trị hiện tại xuống 0
        //       trong _fadeDuration giây, yield return null mỗi frame
        //       xong thì _isActive = false
        throw new System.NotImplementedException();
    }
}
