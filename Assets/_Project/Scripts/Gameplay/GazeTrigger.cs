using UnityEngine;
using UnityEngine.Events;

public class GazeTrigger : MonoBehaviour
{
    [SerializeField] private float _gazeThreshold = 3f;
    private float _gazeTimer = 0f;
    private bool _isGazing = false;

    public UnityEvent OnGazeComplete;
    public UnityEvent OnGazeWarning;

    // Property để test kiểm tra trạng thái
    public bool IsGazing => _isGazing;

    // Hàm để test hoặc gameplay gọi bắt đầu gaze
    public void StartGaze()
    {
        _isGazing = true;
        _gazeTimer = 0f;
    }

    // Hàm để test hoặc gameplay gọi dừng gaze
    public void StopGaze()
    {
        _isGazing = false;
        _gazeTimer = 0f;
    }

    private void Update()
    {
        if (!_isGazing) return; // chỉ tính giờ khi đang gaze

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
