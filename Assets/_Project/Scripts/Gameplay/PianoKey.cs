using UnityEngine;
using System.Collections;

/// <summary>
/// Gắn lên từng phím đàn (7 object).
/// Phím được nhấn bằng KeyCode trên bàn phím (A/S/D/F/G/H/J...), KHÔNG dùng E.
/// Chỉ nhận input khi Piano đang ở piano mode (IsInPianoMode == true).
/// Animation: phím nhún xuống rồi trả về vị trí ban đầu bằng Lerp.
/// </summary>
public class PianoKey : MonoBehaviour
{
    [Header("Note Definition — kéo ScriptableObject vào để dùng dropdown")]
    [SerializeField] private PianoNoteDefinition _noteDefinition;

    [Header("Note của phím này")]
    [SerializeField] private string _note;

    [Header("Phím bàn phím tương ứng (A/S/D/F/G/H/J...)")]
    [SerializeField] private KeyCode _keyCode = KeyCode.A;

    [Header("Piano chứa phím này")]
    [SerializeField] private PianoInteractable _piano;

    [Header("Animation phím")]
    [Tooltip("Phím nhún xuống bao nhiêu đơn vị theo trục Y")]
    [SerializeField] private float _pressDepth  = 0.05f;
    [Tooltip("Tốc độ nhún xuống")]
    [SerializeField] private float _pressSpeed  = 20f;
    [Tooltip("Tốc độ trả lên")]
    [SerializeField] private float _returnSpeed = 8f;

    // Animation state
    private Vector3   _restPosition;
    private Vector3   _pressedPosition;
    private Vector3   _targetPosition;
    private Coroutine _animCoroutine;

    public string Note => _note;

    private void Awake()
    {
        _restPosition    = transform.localPosition;
        _pressedPosition = _restPosition + Vector3.down * _pressDepth;
        _targetPosition  = _restPosition;
    }

    private void Update()
    {
        // Lerp animation
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, _targetPosition,
            Time.deltaTime * (_targetPosition == _pressedPosition ? _pressSpeed : _returnSpeed));

        // Chỉ nhận input khi đang trong piano mode
        if (_piano == null || !_piano.IsInPianoMode) return;

        if (Input.GetKeyDown(_keyCode))
            PressKey();
    }

    private void PressKey()
    {
        // Animation nhún
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(PressAndReturn());

        Debug.Log($"[PianoKey] Nhấn phím: {_note} ({_keyCode})");
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