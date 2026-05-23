using UnityEngine;
using UnityEngine.Events;

public class GazeTrigger : MonoBehaviour
{
    [SerializeField] private GazeSettings _settings; // Kéo file GazeSettings vào đây
    private float _gazeTimer = 0f;

    public UnityEvent OnGazeComplete;
    public UnityEvent OnGazeWarning;

    private void Update()
    {
        // Kiểm tra an toàn nếu chưa gán settings
        if (_settings == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        
        // Sử dụng _settings.maxDistance để giới hạn tầm nhìn
        if (Physics.Raycast(ray, out RaycastHit hit, _settings.maxDistance))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                _gazeTimer += Time.deltaTime;

                // Sử dụng warningThreshold từ settings
                if (_gazeTimer >= _settings.warningThreshold && _gazeTimer < _settings.gazeThreshold)
                {
                    OnGazeWarning?.Invoke();
                }

                // Sử dụng gazeThreshold từ settings
                if (_gazeTimer >= _settings.gazeThreshold)
                {
                    OnGazeComplete?.Invoke();
                    _gazeTimer = 0f; 
                }
                return;
            }
        }

        _gazeTimer = 0f;
    }
}