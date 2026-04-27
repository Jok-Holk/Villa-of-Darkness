using UnityEngine;
using UnityEngine.Events;

public class GazeTrigger : MonoBehaviour
{
    [SerializeField] private float _gazeThreshold = 3f;
    private float _gazeTimer = 0f;

    public UnityEvent OnGazeComplete;
    public UnityEvent OnGazeWarning;

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                _gazeTimer += Time.deltaTime;

                if (_gazeTimer >= 1f && _gazeTimer < _gazeThreshold)
                {
                    OnGazeWarning?.Invoke();
                }

                if (_gazeTimer >= _gazeThreshold)
                {
                    OnGazeComplete?.Invoke();
                    _gazeTimer = 0f; // reset sau khi hoàn thành
                }
                return;
            }
        }

        // Nếu không nhìn vào object thì reset timer
        _gazeTimer = 0f;
    }
}
