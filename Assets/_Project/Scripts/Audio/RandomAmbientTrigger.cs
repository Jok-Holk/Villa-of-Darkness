using UnityEngine;

public class RandomAmbientTrigger : MonoBehaviour
{
    [Header("Danh sách SFX random trong phòng")]
    [SerializeField] private AudioClip[] _sfxClips;

    [Header("Thời gian giữa 2 lần phát (giây)")]
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 20f;

    private bool _playerInside = false;
    private float _timer = 0f;
    private float _nextTriggerTime = 0f;

    private void Start()
    {
        // Random thời gian phát đầu tiên
        _nextTriggerTime = Random.Range(_minInterval, _maxInterval);
    }

    private void Update()
    {
        if (!_playerInside) return;
        if (_sfxClips == null || _sfxClips.Length == 0) return;

        _timer += Time.deltaTime;

        if (_timer >= _nextTriggerTime)
        {
            PlayRandomSFX();
            _timer = 0f;
            _nextTriggerTime = Random.Range(_minInterval, _maxInterval);
        }
    }

    private void PlayRandomSFX()
    {
        int index = Random.Range(0, _sfxClips.Length);
        AudioClip clip = _sfxClips[index];
        if (clip != null)
            AudioManager.Instance.PlaySFX(clip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        _timer = 0f;
        // Random thời gian phát đầu tiên khi vào phòng
        _nextTriggerTime = Random.Range(2f, 6f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
    }
}