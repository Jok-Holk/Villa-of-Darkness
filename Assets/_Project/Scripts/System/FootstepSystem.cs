using UnityEngine;

/// <summary>
/// Phát âm thanh bước chân khác nhau tùy theo PhysicMaterial của mặt đất.
/// Gắn lên Player (cùng GameObject với CharacterController).
///
/// SETUP TRONG UNITY:
///   1. Gán _playerController (hoặc để trống — script tự GetComponent).
///   2. Tạo các SurfaceProfile trong Inspector: mỗi profile có tên material + list clips.
///   3. Gán _defaultClips để dùng khi không nhận ra material (fallback).
///   4. Điều chỉnh _stepInterval theo walk/run speed.
///
/// HOW IT WORKS:
///   • Mỗi _stepInterval giây (khi player đang di chuyển + grounded) → Raycast xuống.
///   • Lấy PhysicMaterial từ surface → tìm SurfaceProfile khớp tên.
///   • Random 1 clip trong profile → PlayOneShot qua AudioManager.
/// </summary>
public class FootstepSystem : MonoBehaviour
{
    // ─── Surface Profile ──────────────────────────────────────────────────────
    [System.Serializable]
    public class SurfaceProfile
    {
        [Tooltip("Tên PhysicMaterial khớp — phân biệt hoa thường, ví dụ: Wood, Brick, Dirt")]
        public string materialName;

        [Tooltip("List âm thanh bước chân cho surface này — sẽ random mỗi bước")]
        public AudioClip[] clips;
    }

    [Header("Surface Profiles — thứ tự ưu tiên: khớp tên trước, fallback sau")]
    [SerializeField] private SurfaceProfile[] _surfaces;

    [Header("Fallback — dùng khi không nhận ra material")]
    [SerializeField] private AudioClip[] _defaultClips;

    [Header("Step Settings")]
    [Tooltip("Thời gian giữa 2 bước chân khi đi bộ (giây)")]
    [SerializeField] private float _walkStepInterval = 0.5f;
    [Tooltip("Thời gian giữa 2 bước chân khi chạy (giây)")]
    [SerializeField] private float _runStepInterval  = 0.3f;
    [Tooltip("Thời gian giữa 2 bước chân khi khom (giây)")]
    [SerializeField] private float _crouchStepInterval = 0.75f;
    [Tooltip("Ngưỡng vận tốc tối thiểu để phát tiếng bước (tránh phát khi đứng yên)")]
    [SerializeField] private float _minMoveSpeed = 0.1f;
    [Tooltip("Volume của bước chân")]
    [Range(0f, 1f)]
    [SerializeField] private float _footstepVolume = 0.6f;

    [Header("Raycast")]
    [Tooltip("Độ dài raycast xuống đất — nên bằng khoảng cách từ pivot xuống đất + 0.1")]
    [SerializeField] private float _rayLength = 1.2f;
    [SerializeField] private LayerMask _groundLayer = ~0; // mặc định all layers

    [Header("References")]
    [Tooltip("Để trống → tự GetComponent<CharacterController>()")]
    [SerializeField] private CharacterController _cc;

    // ─── Private State ────────────────────────────────────────────────────────
    private float _stepTimer = 0f;
    private bool  _isRunning = false;
    private bool  _isCrouching = false;

    // ─── INIT ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_cc == null) _cc = GetComponent<CharacterController>();
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────
    private void Update()
    {
        if (_cc == null) return;
        if (!_cc.isGrounded)   return;  // đang nhảy / rơi → không phát tiếng

        // Lấy vận tốc ngang để biết có đang di chuyển không
        Vector3 horizontalVel = new Vector3(_cc.velocity.x, 0f, _cc.velocity.z);
        if (horizontalVel.magnitude < _minMoveSpeed) return; // đứng yên

        // Phát hiện trạng thái run / crouch từ input
        _isCrouching = Input.GetKey(KeyCode.C);
        _isRunning   = !_isCrouching && Input.GetKey(KeyCode.LeftShift);

        float interval = _isCrouching ? _crouchStepInterval
                       : _isRunning   ? _runStepInterval
                       : _walkStepInterval;

        _stepTimer += Time.deltaTime;
        if (_stepTimer < interval) return;

        _stepTimer = 0f;
        PlayFootstep();
    }

    // ─── PLAY ─────────────────────────────────────────────────────────────────
    private void PlayFootstep()
    {
        AudioClip clip = GetClipForCurrentSurface();
        if (clip == null) return;
        if (AudioManager.Instance == null) return;

        // Dùng internal PlayOneShot với volume tùy chỉnh nếu có,
        // hoặc gọi PlaySFX (volume do AudioSource quyết định)
        AudioManager.Instance.PlaySFX(clip, _footstepVolume);
    }

    // ─── SURFACE DETECTION ────────────────────────────────────────────────────
    private AudioClip GetClipForCurrentSurface()
    {
        // Raycast xuống để lấy PhysicMaterial
        Ray ray = new Ray(transform.position + Vector3.up * 0.05f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, _rayLength, _groundLayer))
        {
            PhysicsMaterial mat = hit.collider.sharedMaterial;
            string matName = mat != null ? mat.name : string.Empty;

            if (!string.IsNullOrEmpty(matName) && _surfaces != null)
            {
                foreach (var profile in _surfaces)
                {
                    // So sánh không phân biệt hoa thường cho an toàn
                    if (profile.materialName.Equals(matName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return PickRandom(profile.clips);
                    }
                }
            }
        }

        // Không khớp material nào → fallback
        return PickRandom(_defaultClips);
    }

    // ─── HELPER ───────────────────────────────────────────────────────────────
    private AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    // ─── EDITOR DEBUG ─────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        Gizmos.DrawLine(origin, origin + Vector3.down * _rayLength);
    }
}