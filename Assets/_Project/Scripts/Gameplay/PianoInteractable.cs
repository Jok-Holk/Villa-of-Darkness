using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gắn lên GameObject piano (parent chứa các PianoKey).
/// Interact lần đầu → camera zoom vào piano, player bị lock input.
/// Nhấn E lần nữa → camera zoom ra, player được trả input.
/// Các phím đàn dùng bàn phím máy tính (A/S/D/F/G/H/J), KHÔNG dùng E.
/// </summary>
public class PianoInteractable : MonoBehaviour, IInteractable
{
    [Header("Note Definition — kéo ScriptableObject vào để dùng dropdown")]
    [SerializeField] private PianoNoteDefinition _noteDefinition;

    [Header("Sequence đúng — mỗi element chọn từ dropdown")]
    [SerializeField] private string[] _correctSequence;

    [Header("Sound")]
    [SerializeField] private AudioClip _correctNoteClip;
    [SerializeField] private AudioClip _wrongNoteClip;
    [SerializeField] private AudioClip _sequenceCompleteClip;

    [Header("Ghost Spawn")]
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private GameObject   _ghostPrefab;
    [SerializeField] private Transform    _ghostSpawnPoint;

    [Header("Camera Zoom")]
    [Tooltip("Vị trí camera sẽ zoom đến khi nhìn vào piano")]
    [SerializeField] private Transform _cameraZoomTarget;
    [Tooltip("Kéo PlayerController vào để lock input khi đang chơi piano")]
    [SerializeField] private PlayerController _playerController;
    [Tooltip("Tốc độ zoom vào/ra")]
    [SerializeField] private float _zoomSpeed = 3f;

    // ─── State ────────────────────────────────────────────────────────────────
    private List<string> _inputSequence = new List<string>();
    private bool         _isCompleted   = false;
    private bool         _isInPianoMode = false;

    // Camera state
    private Camera     _cam;
    private Vector3    _camOriginPos;
    private Quaternion _camOriginRot;
    private Coroutine  _zoomCoroutine;

    // Frame guard cho E để tránh exit ngay frame vừa Enter
    private int _enterFrame = -1;

    public UnityEvent OnSequenceComplete = new UnityEvent();
    public UnityEvent OnEnterPianoMode   = new UnityEvent();
    public UnityEvent OnExitPianoMode    = new UnityEvent();

    // ─── INIT ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _cam = Camera.main;
    }

    // ─── UPDATE — lắng nghe E để thoát piano mode ─────────────────────────────
    private void Update()
    {
        if (!_isInPianoMode) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        // Frame guard: bỏ qua nếu E vừa được dùng để Enter cùng frame
        if (Time.frameCount == _enterFrame) return;

        ExitPianoMode();
    }

    // ─── INTERACT — chỉ dùng để vào piano mode (gọi từ InteractionSystem) ────
    public void Interact()
    {
        if (_isCompleted)
        {
            Debug.Log("[Piano] Đã hoàn thành rồi.");
            return;
        }

        if (_isInPianoMode) return; // đang trong mode → Update() lo việc thoát

        EnterPianoMode();
    }

    // ─── PIANO MODE ───────────────────────────────────────────────────────────
    private void EnterPianoMode()
    {
        _isInPianoMode = true;
        _enterFrame    = Time.frameCount; // ghi lại frame enter để guard E

        // Chặn InteractionSystem xử lý phím E trong lúc đang chơi piano
        InteractionSystem.IsInputBlocked = true;

        // Lock player input
        if (_playerController != null)
            _playerController.SetInputEnabled(false);

        // Lưu vị trí camera hiện tại để zoom ra sau
        if (_cam != null)
        {
            _camOriginPos = _cam.transform.position;
            _camOriginRot = _cam.transform.rotation;
        }

        // Zoom vào
        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        if (_cameraZoomTarget != null && _cam != null)
            _zoomCoroutine = StartCoroutine(ZoomCamera(
                _cameraZoomTarget.position,
                _cameraZoomTarget.rotation));

        Debug.Log("[Piano] Vào chế độ chơi đàn. Nhấn E để thoát.");
        OnEnterPianoMode.Invoke();
    }

    private void ExitPianoMode()
    {
        _isInPianoMode = false;

        // Trả phím E về cho InteractionSystem
        InteractionSystem.IsInputBlocked = false;

        // Reset sequence khi thoát — tránh tiếp tục từ giữa chừng lần sau vào lại
        _inputSequence.Clear();

        // Zoom ra về vị trí cũ
        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        if (_cam != null)
            _zoomCoroutine = StartCoroutine(ZoomCamera(_camOriginPos, _camOriginRot,
                onDone: () =>
                {
                    if (_playerController != null)
                        _playerController.SetInputEnabled(true);

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible   = false;
                }));

        Debug.Log("[Piano] Thoát chế độ đàn.");
        OnExitPianoMode.Invoke();
    }

    // ─── ADD NOTE ─────────────────────────────────────────────────────────────
    public void AddNote(string note)
    {
        if (_isCompleted) return;
        if (string.IsNullOrEmpty(note)) return;
        if (_correctSequence == null || _correctSequence.Length == 0) return;
        if (!_isInPianoMode) return;

        bool noteIsCorrect = _inputSequence.Count < _correctSequence.Length
                             && note == _correctSequence[_inputSequence.Count];

        if (noteIsCorrect)
        {
            _inputSequence.Add(note);
            PlaySFX(_correctNoteClip);

            string progress = string.Join(" → ", _inputSequence);
            Debug.Log($"[Piano] ✔ {note} | {progress} ({_inputSequence.Count}/{_correctSequence.Length})");

            if (_inputSequence.Count == _correctSequence.Length)
                CompleteSequence();
        }
        else
        {
            PlaySFX(_wrongNoteClip);
            Debug.Log($"[Piano] ✘ Sai: [{note}] — reset! Bắt đầu lại từ [{_correctSequence[0]}]");
            _inputSequence.Clear();
        }
    }

    public void PressNote(string note) => AddNote(note);

    // ─── COMPLETE ─────────────────────────────────────────────────────────────
    private void CompleteSequence()
    {
        _isCompleted = true;
        PlaySFX(_sequenceCompleteClip);
        Debug.Log("[Piano] ✔✔✔ PIANO DONE!");
        OnSequenceComplete.Invoke();

        ExitPianoMode();

        if (_spawnManager != null && _ghostPrefab != null)
        {
            if (_ghostSpawnPoint != null)
                _spawnManager.SpawnAtTransform(_ghostPrefab, _ghostSpawnPoint);
            else
                _spawnManager.SpawnAt(_ghostPrefab, transform.position + transform.forward * 2f);
            Debug.Log("[Piano] Ghost spawned!");
        }
    }

    // ─── CAMERA ZOOM (Lerp) ───────────────────────────────────────────────────
    private IEnumerator ZoomCamera(Vector3 targetPos, Quaternion targetRot,
                                   System.Action onDone = null)
    {
        float threshold = 0.01f;
        while (Vector3.Distance(_cam.transform.position, targetPos) > threshold)
        {
            float dt = Time.deltaTime * _zoomSpeed;
            _cam.transform.position = Vector3.Lerp(_cam.transform.position, targetPos, dt);
            _cam.transform.rotation = Quaternion.Slerp(_cam.transform.rotation, targetRot, dt);
            yield return null;
        }

        _cam.transform.position = targetPos;
        _cam.transform.rotation = targetRot;
        _zoomCoroutine = null;

        onDone?.Invoke();
    }

    // ─── SFX ──────────────────────────────────────────────────────────────────
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip);
    }

    // ─── GETTER (dùng cho PianoKey) ───────────────────────────────────────────
    public bool IsInPianoMode => _isInPianoMode;
}