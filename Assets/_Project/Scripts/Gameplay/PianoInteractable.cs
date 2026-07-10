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

    [Header("Input — A/D chọn phím, Space chơi phím đang chọn")]
    [Tooltip("Danh sách phím playable theo đúng thứ tự trái→phải trên bàn phím (khớp thứ tự Element trong Correct Sequence nếu muốn dễ nhớ, nhưng không bắt buộc — chỉ cần đúng thứ tự vật lý trái-phải để A/D di chuyển tự nhiên)")]
    [SerializeField] private PianoKey[] _playableKeys;
    private int _selectedKeyIndex = 0;

    [Header("Bark — lời thoại ngắn khi bấm đúng thứ tự (không dùng DialogueUI đầy đủ)")]
    [Tooltip("Random 1 trong các clip này mỗi khi bấm đúng 1 nốt trong sequence")]
    [SerializeField] private AudioClip[] _correctProgressBarks;

    [Header("Note Label — chữ 3D nổi trong không gian, bám theo phím đang chọn")]
    [Tooltip("3D Text (World Space, KHÔNG phải Canvas UI) — kéo object 'Text - TextMeshPro' (3D Object) vào đây. Để trống nếu chưa làm.")]
    [SerializeField] private TMPro.TextMeshPro _noteLabelText;
    [Tooltip("Lệch X/Y/Z so với vị trí phím đang chọn (world space) — CHỈNH TRỰC TIẾP 3 SỐ NÀY để đổi vị trí, không kéo tay object (object sẽ bị code ghi đè mỗi lần chọn phím)")]
    [SerializeField] private Vector3 _noteLabelOffset = new Vector3(0f, 0.15f, 0f);

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

    [Header("Sheet Music Lock")]
    [Tooltip("Kéo InventorySystem vào để kiểm tra SheetMusic trước khi chơi")]
    [SerializeField] private InventorySystem _inventorySystem;
    [Tooltip("Item ID của tờ nhạc — mặc định: SheetMusic")]
    [SerializeField] private string _requiredItemId = "SheetMusic";

    [Header("Đèn pin — tạm tắt khi chơi đàn, khôi phục đúng trạng thái trước đó khi thoát")]
    [SerializeField] private FlashlightController _flashlight;

    // ─── State ────────────────────────────────────────────────────────────────
    private List<string> _inputSequence = new List<string>();
    private bool         _isCompleted   = false;
    private bool         _isInPianoMode = false;
    private bool         _flashlightWasOnBeforeEnter = false;

    // Camera state
    private Camera     _cam;
    private Vector3    _camOriginPos;
    private Quaternion _camOriginRot;
    private Coroutine  _zoomCoroutine;

    // Frame guard cho E để tránh exit ngay frame vừa Enter
    private int _enterFrame = -1;

    // Chỉ true sau khi camera zoom vào xong hẳn — chặn A/D/Space bấm được trong lúc camera còn đang chạy
    private bool _isZoomedIn = false;

    public UnityEvent OnSequenceComplete  = new UnityEvent();
    public UnityEvent OnEnterPianoMode    = new UnityEvent();
    public UnityEvent OnExitPianoMode     = new UnityEvent();
    /// <summary>Fire khi player interact nhưng chưa có SheetMusic — UI dùng để hiện prompt.</summary>
    public UnityEvent OnMissingSheetMusic = new UnityEvent();

    // ─── INIT ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        if (_playerController == null) _playerController = PlayerController.Instance;
        if (_inventorySystem == null)  _inventorySystem  = InventorySystem.Instance;
    }

    // ─── UPDATE — A/D chọn phím, Space chơi, E thoát piano mode ───────────────
    private void Update()
    {
        if (!_isInPianoMode) return;

        // Frame guard: bỏ qua nếu E vừa được dùng để Enter cùng frame
        if (Input.GetKeyDown(KeyCode.E) && Time.frameCount != _enterFrame)
        {
            ExitPianoMode();
            return;
        }

        // Chưa cho bấm chọn/chơi phím trong lúc camera còn đang zoom vào — tránh cảm giác vội
        if (!_isZoomedIn) return;

        if (_playableKeys == null || _playableKeys.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveSelection(1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveSelection(-1);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PianoKey selected = _playableKeys[_selectedKeyIndex];
            if (selected != null) selected.Press();
        }
    }

    private void MoveSelection(int delta)
    {
        if (_playableKeys[_selectedKeyIndex] != null)
            _playableKeys[_selectedKeyIndex].SetHighlighted(false);

        _selectedKeyIndex = (_selectedKeyIndex + delta + _playableKeys.Length) % _playableKeys.Length;

        if (_playableKeys[_selectedKeyIndex] != null)
            _playableKeys[_selectedKeyIndex].SetHighlighted(true);

        UpdateNoteLabel();
    }

    // Tên đầy đủ có dấu — dễ đọc hơn cho người không rành nhạc lý so với chỉ chữ cái C/D/E.
    private static readonly System.Collections.Generic.Dictionary<string, string> DisplayName =
        new System.Collections.Generic.Dictionary<string, string>
        {
            { "Do", "Đô" }, { "Re", "Rê" }, { "Mi", "Mi" }, { "Fa", "Fa" },
            { "Sol", "Sol" }, { "La", "La" }, { "Si", "Si" },
        };

    private void UpdateNoteLabel()
    {
        if (_noteLabelText == null) return;
        if (_playableKeys == null || _selectedKeyIndex >= _playableKeys.Length) return;

        PianoKey key = _playableKeys[_selectedKeyIndex];
        if (key == null) return;

        string note = key.Note;
        _noteLabelText.text = DisplayName.TryGetValue(note, out string display) ? display : note;

        // QUAN TRỌNG: KHÔNG dùng key.transform.position — mọi phím đều có localPosition (0,0,0)
        // (dính chung gốc toạ độ Piano_Body), vị trí thật của từng phím nằm trong dữ liệu mesh
        // (vertices), không phải trong Transform. Phải dùng tâm bounds của Renderer để lấy đúng
        // vị trí hiển thị thật của từng phím riêng biệt.
        Vector3 keyWorldPos = key.transform.position;
        var keyRenderer = key.GetComponent<Renderer>();
        if (keyRenderer != null) keyWorldPos = keyRenderer.bounds.center;

        // Lệch X/Y/Z so với đúng phím đang chọn — chỉnh field "Note Label Offset" trong Inspector
        // của PianoInteractable, KHÔNG kéo tay object trong Scene view vì code tính lại vị trí này
        // mỗi lần chọn phím/vào piano mode, kéo tay sẽ bị ghi đè ngay.
        _noteLabelText.transform.position = keyWorldPos + _noteLabelOffset;

        // Xoay chữ đối diện camera để luôn đọc được rõ dù piano ở góc nào — kiểu "billboard" chỉ xoay
        // quanh trục Y (dọc), KHÔNG dùng hướng camera đầy đủ 3 chiều vì sẽ làm chữ bị nghiêng/xoay lệch
        // (roll/pitch) mỗi khi góc camera hơi khác giữa các lần chọn phím. Chỉ lấy thành phần ngang (X/Z)
        // của hướng nhìn để đảm bảo chữ LUÔN thẳng đứng, không bao giờ bị nghiêng.
        if (_cam != null)
        {
            Vector3 dir = _noteLabelText.transform.position - _cam.transform.position;
            dir.y = 0f; // bỏ thành phần dọc — chỉ xoay ngang (yaw), giữ chữ luôn đứng thẳng
            if (dir.sqrMagnitude > 0.0001f)
                _noteLabelText.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
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

        // Kiểm tra tờ nhạc — phải có trong túi mới được chơi
        if (_inventorySystem != null && !string.IsNullOrEmpty(_requiredItemId))
        {
            if (!_inventorySystem.HasItem(_requiredItemId))
            {
                Debug.Log("[Piano] Cần tìm bản nhạc trước.");
                // Fire event để UI hiện prompt nếu cần (designer wire vào UnityEvent)
                OnMissingSheetMusic?.Invoke();
                return;
            }
        }

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

        // Tạm tắt đèn pin (nếu đang bật) — lưu lại trạng thái để khôi phục đúng khi thoát
        if (_flashlight != null)
        {
            _flashlightWasOnBeforeEnter = _flashlight.IsOn;
            if (_flashlightWasOnBeforeEnter) _flashlight.SetOn(false);
        }

        // Lưu vị trí camera hiện tại để zoom ra sau
        if (_cam != null)
        {
            _camOriginPos = _cam.transform.position;
            _camOriginRot = _cam.transform.rotation;
        }

        // Zoom vào — chỉ cho phép bấm A/D/Space sau khi zoom xong hẳn (onDone)
        _isZoomedIn = false;
        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        if (_cameraZoomTarget != null && _cam != null)
        {
            _zoomCoroutine = StartCoroutine(ZoomCamera(
                _cameraZoomTarget.position,
                _cameraZoomTarget.rotation,
                onDone: () => _isZoomedIn = true));
        }
        else
        {
            // Không có camera zoom target gán sẵn — không có gì để đợi, cho phép bấm ngay
            _isZoomedIn = true;
        }

        // Reset về phím đầu tiên và bật highlight — player thấy ngay phím nào đang chọn
        _selectedKeyIndex = 0;
        if (_playableKeys != null && _playableKeys.Length > 0 && _playableKeys[0] != null)
            _playableKeys[0].SetHighlighted(true);

        if (_noteLabelText != null) _noteLabelText.gameObject.SetActive(true);
        UpdateNoteLabel();

        Debug.Log("[Piano] Vào chế độ chơi đàn. A/D chọn phím, Space chơi, E để thoát.");
        OnEnterPianoMode.Invoke();
    }

    private void ExitPianoMode()
    {
        _isInPianoMode = false;
        _isZoomedIn    = false;

        // Tắt highlight phím đang chọn trước khi thoát
        if (_playableKeys != null && _selectedKeyIndex < _playableKeys.Length && _playableKeys[_selectedKeyIndex] != null)
            _playableKeys[_selectedKeyIndex].SetHighlighted(false);

        if (_noteLabelText != null) _noteLabelText.gameObject.SetActive(false);

        // Khôi phục đúng trạng thái đèn pin trước khi vào (nếu trước đó đang bật thì bật lại)
        if (_flashlight != null && _flashlightWasOnBeforeEnter)
            _flashlight.SetOn(true);

        // Trả phím E về cho InteractionSystem — TRỄ 1 frame để tránh race condition:
        // nếu unblock ngay trong frame này, InteractionSystem.Update() (nếu chạy sau PianoInteractable.Update()
        // trong cùng frame) vẫn thấy phím E đang nhấn + đã hết block -> vào lại piano ngay lập tức.
        StartCoroutine(UnblockInputNextFrame());

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

    private System.Collections.IEnumerator UnblockInputNextFrame()
    {
        yield return null;
        InteractionSystem.IsInputBlocked = false;
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

            // Bark ngắn ngẫu nhiên khi đúng — không dùng DialogueUI đầy đủ, chỉ 1 câu thoại rời
            if (_correctProgressBarks != null && _correctProgressBarks.Length > 0)
                PlaySFX(_correctProgressBarks[Random.Range(0, _correctProgressBarks.Length)]);

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
        float threshold   = 0.01f;
        float maxDuration  = 3f; // an toàn: nếu vì lý do gì đó không hội tụ được, ép hoàn thành sau tối đa 3s
                                  // để KHÔNG BAO GIỜ khoá input của player vĩnh viễn.
        float elapsed = 0f;

        while (Vector3.Distance(_cam.transform.position, targetPos) > threshold && elapsed < maxDuration)
        {
            float dt = Time.deltaTime * _zoomSpeed;
            _cam.transform.position = Vector3.Lerp(_cam.transform.position, targetPos, dt);
            _cam.transform.rotation = Quaternion.Slerp(_cam.transform.rotation, targetRot, dt);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= maxDuration)
            Debug.LogWarning("[Piano] ZoomCamera timeout — ép hoàn thành để tránh kẹt input vĩnh viễn.");

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