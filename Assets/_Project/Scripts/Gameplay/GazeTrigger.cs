using UnityEngine;
using UnityEngine.Events;

public class GazeTrigger : MonoBehaviour
{
    [SerializeField] private GazeSettings _settings; // Kéo file GazeSettings vào đây
    private float _gazeTimer = 0f;

    [Header("Debug")]
    [Tooltip("Bật để in log chi tiết mỗi frame: raycast trúng gì, có settings chưa, v.v. Tắt đi khi đã chạy ổn để đỡ spam Console.")]
    [SerializeField] private bool _verboseDebug = true;

    public UnityEvent OnGazeComplete;
    public UnityEvent OnGazeWarning;

    private void Awake()
    {
        // Cảnh báo sớm ngay khi scene load nếu quên gán Settings, thay vì im lặng suốt Update()
        if (_settings == null)
            Debug.LogWarning($"[Gaze Debug] '{gameObject.name}': field Settings (GazeSettings) đang để trống! Kéo asset GazeSettings vào Inspector.");
    }

    private void Update()
    {
        // Kiểm tra an toàn nếu chưa gán settings
        if (_settings == null) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[Gaze Debug] KHÔNG tìm thấy Camera nào mang Tag MainCamera! Kiểm tra Tag trên object có Component Camera (thường là camera con bên trong Player).");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        
        // Vẽ tia chỉ màu đỏ trong cửa sổ Scene để Phúc dễ quan sát hướng nhìn của Player
        Debug.DrawRay(ray.origin, ray.direction * _settings.maxDistance, Color.red);

        // Sử dụng _settings.maxDistance để giới hạn tầm nhìn
        if (Physics.Raycast(ray, out RaycastHit hit, _settings.maxDistance))
        {
            if (_verboseDebug)
            {
                // In ra chính xác raycast đang trúng object nào — chỗ này giúp phát hiện ngay
                // nếu tia bị vật khác (VD: thành giếng đá) che mất trước khi chạm tới điểm cần nhìn.
                Debug.Log($"[Gaze Debug] Raycast HIT: '{hit.collider.gameObject.name}' cách {hit.distance:F2}m. " +
                          $"(GazeTrigger này đang gắn trên object '{gameObject.name}')");
            }

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

                    // 🎯 SỬA LỖI 3: KHÔNG gọi GameManager.Instance.PlayerDead() ở đây nữa.
                    // Lý do: PlayerDead() set Time.timeScale = 0f ngay lập tức, làm Time.deltaTime = 0
                    // vĩnh viễn cho tới khi resume — khiến mọi coroutine dùng scaled time bên trong
                    // WellDeathSequence (fade overlay, jumpscare, fade đen...) bị đứng khựng giữa chừng,
                    // không bao giờ chạy tới bước phát âm thanh/hiện DeathScreen.
                    // WellDeathSequence.PlayDeathSequence() đã tự gọi PlayerDead() đúng lúc, ở cuối chuỗi.
                    // Nếu GazeTrigger này KHÔNG có WellDeathSequence lắng nghe (dùng cho case khác, VD gương),
                    // hãy tự gọi GameManager.Instance?.PlayerDead() trong hàm nghe OnGazeComplete tương ứng.
                    OnGazeComplete?.Invoke();
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