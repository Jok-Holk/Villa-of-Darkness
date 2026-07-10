using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private Transform[]     _cameraPath;
    [SerializeField] private float           _stepDuration = 1.5f;
    [SerializeField] private PlayerController _player;

    public UnityEvent OnCutsceneStart = new UnityEvent();
    public UnityEvent OnCutsceneEnd   = new UnityEvent();

    private bool _isPlaying = false;
    public bool IsPlaying => _isPlaying;

    private void Start()
    {
        if (_player == null) _player = PlayerController.Instance;
    }

    public void Play()
    {
        if (_isPlaying) return;
        StartCoroutine(Run());
    }

    public void Stop()
    {
        StopAllCoroutines();
        _isPlaying = false;
        SetPlayerLocked(false);
        OnCutsceneEnd?.Invoke();
    }

    private IEnumerator Run()
    {
        _isPlaying = true;
        SetPlayerLocked(true);
        OnCutsceneStart?.Invoke();

        if (_cameraPath != null)
        {
            Camera cam = Camera.main;
            foreach (Transform waypoint in _cameraPath)
            {
                Vector3    startPos = cam.transform.position;
                Quaternion startRot = cam.transform.rotation;
                float t = 0f;
                while (t < _stepDuration)
                {
                    t += Time.deltaTime;
                    float p = Mathf.Clamp01(t / _stepDuration);
                    cam.transform.position = Vector3.Lerp(startPos, waypoint.position, p);
                    cam.transform.rotation = Quaternion.Slerp(startRot, waypoint.rotation, p);
                    yield return null;
                }
            }
        }

        _isPlaying = false;
        SetPlayerLocked(false);
        OnCutsceneEnd?.Invoke();
    }

    private void SetPlayerLocked(bool locked)
    {
        if (_player != null) _player.enabled = !locked;
    }
}
