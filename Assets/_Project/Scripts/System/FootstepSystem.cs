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

        [Tooltip("List âm thanh bước chân cho surface này — phát TUẦN HOÀN (round-robin) chứ không random,\nđể tránh lặp trùng clip 2 bước liên tiếp (random thuần với 2 clip có 50% khả năng lặp).")]
        public AudioClip[] clips;

        private int _cycleIndex; // không serialize — chỉ là state runtime để tuần hoàn

        public AudioClip NextClip()
        {
            if (clips == null || clips.Length == 0) return null;
            var clip = clips[_cycleIndex % clips.Length];
            _cycleIndex++;
            return clip;
        }
    }

    [Header("Surface Profiles — thứ tự ưu tiên: khớp tên trước, fallback sau")]
    [SerializeField] private SurfaceProfile[] _surfaces;

    [Header("Fallback — dùng khi không nhận ra material")]
    [SerializeField] private AudioClip[] _defaultClips;

    [Header("Step Settings")]
    [Tooltip("Volume bước chân lúc đi bộ (tốc độ = Walk Speed Ref)")]
    [Range(0f, 1f)]
    [SerializeField] private float _walkVolume = 0.085f;
    [Tooltip("Volume bước chân lúc chạy (tốc độ = Run Speed Ref)")]
    [Range(0f, 1f)]
    [SerializeField] private float _runVolume = 0.125f;
    [Tooltip("Tốc độ tương ứng Walk Volume — khớp _walkSpeed trên PlayerController")]
    [SerializeField] private float _walkSpeedRef = 1.5f;
    [Tooltip("Tốc độ tương ứng Run Volume — khớp _runSpeed trên PlayerController")]
    [SerializeField] private float _runSpeedRef = 3f;

    [Header("Raycast")]
    [Tooltip("Độ dài raycast xuống đất — nên bằng khoảng cách từ pivot xuống đất + 0.1")]
    [SerializeField] private float _rayLength = 1.2f;
    [SerializeField] private LayerMask _groundLayer = ~0; // mặc định all layers

    [Header("References")]
    [Tooltip("Để trống → tự GetComponent<CharacterController>()")]
    [SerializeField] private CharacterController _cc;

    // ─── Private State ────────────────────────────────────────────────────────
    private int _defaultClipIndex = 0; // tuần hoàn riêng cho _defaultClips (không nằm trong SurfaceProfile)

    // ─── INIT ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_cc == null) _cc = GetComponent<CharacterController>();
    }

    // Không còn tự đếm timer riêng nữa — HeadbobSystem (cùng gốc Player) là nguồn nhịp duy nhất,
    // tự gọi PlayFootstepNow() đúng lúc sóng bob chạm đáy. Trước đây 2 timer độc lập (bob theo
    // Time.time, footstep theo interval riêng) chạy lệch nhau, đôi lúc còn dính đôi tiếng liên tiếp.

    // ─── PLAY ─────────────────────────────────────────────────────────────────
    /// <summary>Gọi từ HeadbobSystem đúng lúc camera chạm đáy sóng bob — đảm bảo audio và hình luôn khớp.
    /// speed = tốc độ thật lúc đó (dùng nội suy Walk Volume ↔ Run Volume theo Walk/Run Speed Ref).</summary>
    public void PlayFootstepNow(float speed = -1f)
    {
        AudioClip clip = GetClipForCurrentSurface();
        if (clip == null) return;
        if (AudioManager.Instance == null) return;

        float volume = _walkVolume;
        if (speed >= 0f)
        {
            float t = Mathf.InverseLerp(_walkSpeedRef, _runSpeedRef, speed);
            volume = Mathf.Lerp(_walkVolume, _runVolume, t);
        }

        AudioManager.Instance.PlaySFX(clip, volume);
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
                        return profile.NextClip();
                    }
                }
            }
        }

        // Không khớp material nào → fallback, cũng tuần hoàn thay vì random
        return PickCyclic(_defaultClips);
    }

    // ─── HELPER ───────────────────────────────────────────────────────────────
    private AudioClip PickCyclic(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        var clip = clips[_defaultClipIndex % clips.Length];
        _defaultClipIndex++;
        return clip;
    }

    // ─── EDITOR DEBUG ─────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        Gizmos.DrawLine(origin, origin + Vector3.down * _rayLength);
    }
}