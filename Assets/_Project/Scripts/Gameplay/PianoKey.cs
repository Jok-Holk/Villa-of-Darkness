using UnityEngine;
using System.Collections;

/// <summary>
/// Gắn lên từng phím đàn playable (7 object: Key_Do..Key_Si).
/// KHÔNG tự nghe input — PianoInteractable điều khiển tập trung (A/D chọn phím, Space chơi)
/// và gọi Highlight()/Press() trên đúng phím đang chọn.
/// Animation: phím nhún xuống rồi trả về vị trí ban đầu bằng Lerp.
/// </summary>
public class PianoKey : MonoBehaviour
{
    [Header("Note Definition — kéo ScriptableObject vào để dùng dropdown")]
    [SerializeField] private PianoNoteDefinition _noteDefinition;

    [Header("Note của phím này")]
    [SerializeField] private string _note;

    [Header("Piano chứa phím này")]
    [SerializeField] private PianoInteractable _piano;

    [Header("Âm thanh phím đàn")]
    [Tooltip("Chỉ cần 1 file mẫu duy nhất (VD nốt Do/C4) gán vào cả 7 phím — code tự tính pitch chính xác theo nửa cung dựa vào _note, không cần 7 file riêng.")]
    [SerializeField] private AudioClip _keyClip;
    private AudioSource _noteAudioSource;

    // Số nửa cung lệch so với Do (C) — dùng để tính pitch chính xác từ 1 sample gốc.
    private static readonly System.Collections.Generic.Dictionary<string, int> SemitoneFromDo =
        new System.Collections.Generic.Dictionary<string, int>
        {
            { "Do", 0 }, { "Re", 2 }, { "Mi", 4 }, { "Fa", 5 },
            { "Sol", 7 }, { "La", 9 }, { "Si", 11 },
        };

    [Header("Animation phím — đơn vị/giây (MoveTowards, không phụ thuộc framerate)")]
    [Tooltip("Phím nhún xuống bao nhiêu đơn vị theo trục Y")]
    [SerializeField] private float _pressDepth  = 0.05f;
    [Tooltip("Tốc độ nhún xuống (đơn vị/giây)")]
    [SerializeField] private float _pressSpeed  = 0.6f;
    [Tooltip("Tốc độ trả lên (đơn vị/giây)")]
    [SerializeField] private float _returnSpeed = 0.25f;

    [Header("Highlight — phím đang được A/D chọn tới")]
    [Tooltip("Renderer sẽ đổi màu emissive khi phím này đang được chọn. Để trống thì tự lấy Renderer trên object này.")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _highlightEmissiveColor = new Color(1f, 0.85f, 0.2f, 1f);
    private MaterialPropertyBlock _mpb;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    // Animation state
    private Vector3   _restPosition;
    private Vector3   _pressedPosition;
    private Vector3   _targetPosition;
    private Coroutine _animCoroutine;
    private bool      _isHighlighted;

    public string Note => _note;

    private void Awake()
    {
        _restPosition    = transform.localPosition;
        _pressedPosition = _restPosition + Vector3.down * _pressDepth;
        _targetPosition  = _restPosition;

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        // AudioSource riêng cho phím này (không dùng chung _sfxSource của AudioManager)
        // để có thể chỉnh pitch độc lập mà không ảnh hưởng âm thanh khác trong game.
        _noteAudioSource = gameObject.AddComponent<AudioSource>();
        _noteAudioSource.playOnAwake = false;
        _noteAudioSource.spatialBlend = 0f; // 2D — piano ở gần camera lúc zoom vào, không cần 3D falloff
    }

    private void Update()
    {
        // MoveTowards (tốc độ cố định m/giây) thay vì Lerp — Lerp dùng hệ số deltaTime*speed
        // sẽ vượt quá 1 khi FPS thấp (deltaTime lớn), khiến Unity clamp về 1 và phím
        // NHẢY TỨC THỜI đến đích trong 1 frame duy nhất, trông y hệt "không có animation".
        float speed = _targetPosition == _pressedPosition ? _pressSpeed : _returnSpeed;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, speed * Time.deltaTime);
    }

    /// <summary>Gọi bởi PianoInteractable khi phím này trở thành/không còn là phím đang chọn (A/D).</summary>
    public void SetHighlighted(bool on)
    {
        if (_isHighlighted == on) return;
        _isHighlighted = on;

        if (_renderer == null) return;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionColorId, on ? _highlightEmissiveColor : Color.black);
        _renderer.SetPropertyBlock(_mpb);
    }

    /// <summary>Gọi bởi PianoInteractable khi phím này đang được chọn và player bấm Space.</summary>
    public void Press()
    {
        if (_keyClip != null && _noteAudioSource != null)
        {
            // Tính pitch chính xác theo nửa cung từ 1 sample gốc (thường gán Do/C4 cho cả 7 phím).
            // 12 nửa cung = 1 quãng 8 → pitch = 2^(n/12).
            int semitone = SemitoneFromDo.TryGetValue(_note, out int st) ? st : 0;
            _noteAudioSource.pitch = Mathf.Pow(2f, semitone / 12f);
            _noteAudioSource.PlayOneShot(_keyClip);
        }

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(PressAndReturn());

        Debug.Log($"[PianoKey] Nhấn phím: {_note}");
        if (_piano != null)
            _piano.AddNote(_note);
    }

    private IEnumerator PressAndReturn()
    {
        _targetPosition = _pressedPosition;

        float threshold = _pressDepth * 0.05f;
        while (Vector3.Distance(transform.localPosition, _pressedPosition) > threshold)
            yield return null;

        _targetPosition = _restPosition;
        _animCoroutine  = null;
    }
}
