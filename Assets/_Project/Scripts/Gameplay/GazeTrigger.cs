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
        if (cam == null)
        {
            Debug.LogError("[Gaze Debug] KHÔNG tìm thấy Camera nào mang Tag MainCamera!");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        
        // Vẽ tia chỉ màu đỏ trong cửa sổ Scene để Phúc dễ quan sát hướng nhìn của Player
        Debug.DrawRay(ray.origin, ray.direction * _settings.maxDistance, Color.red);

        // Sử dụng _settings.maxDistance để giới hạn tầm nhìn
        if (Physics.Raycast(ray, out RaycastHit hit, _settings.maxDistance))
        {
            // 🎯 SỬA LỖI 1: Check xem vật thể bị bắn trúng có phải chính nó HOẶC là con cháu bên trong nó không
            if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
            {
                _gazeTimer += Time.deltaTime;

                // Thêm Log ra Console để Phúc biết cơ chế tích lũy thời gian nhìn đang chạy tốt
                Debug.Log($"[Gaze Debug] Đang nhìn vào Gương! Thời gian tích lũy: {_gazeTimer:F2}/{_settings.gazeThreshold} giây.");

                // Sử dụng warningThreshold từ settings
                if (_gazeTimer >= _settings.warningThreshold && _gazeTimer < _settings.gazeThreshold)
                {
                    OnGazeWarning?.Invoke();
                }

                // Sử dụng gazeThreshold từ settings
                if (_gazeTimer >= _settings.gazeThreshold)
                {
                    Debug.LogWarning("[Gaze Debug] Đã nhìn đủ thời gian! Kích hoạt kịch bản Die.");
                    
                    OnGazeComplete?.Invoke();
                    GameManager.Instance?.PlayerDead();
                    _gazeTimer = 0f; 

                    // 🎯 SỬA LỖI 2: Ngắt script ngay lập tức khi chết để chống spam lặp vô hạn gây đơ máy
                    enabled = false; 
                }
                return;
            }
        }

        // Nếu rời mắt khỏi gương, reset thời gian về 0
        if (_gazeTimer > 0f)
        {
            Debug.Log("[Gaze Debug] Đã rời mắt khỏi Gương! Reset thời gian nhìn.");
            _gazeTimer = 0f;
        }
    }
}