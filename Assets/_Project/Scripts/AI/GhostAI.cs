using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// XOÁ [RequireComponent(typeof(Animator))] 2026-07-30 (Jok phát hiện -- "GhostAI trên ghost cube không
// phải, lộn object rồi. Chưa hề có cái của Thuận spawn ra bao giờ"): setup CŨ bắt Animator phải nằm THẲNG
// trên đúng GameObject có GhostAI -- dẫn tới việc gắn GhostAI lên 1 object placeholder ("GhostCube") không
// đúng model thật, hoặc phải tự tay Instantiate model đúng chỗ rồi kéo lại reference (dễ lộn/quên). Giờ
// GhostAI TỰ SPAWN model thật bằng code (xem _visualPrefab bên dưới) -- không còn phụ thuộc Animator có sẵn
// sẵn trên object hay không, không cần Jok tự Instantiate/kéo tay gì cả.
[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI : MonoBehaviour
{
    public enum State { Patrol, Investigate, Chase, Kill }

    // Chưa thoát khỏi sự bám đuổi — chặn mở Inventory trong lúc này (xem InventoryUI.Open()).
    public static bool AnyGhostChasing { get; private set; }

    // Bắn 1 lần đúng lúc chuyển sang Chase (không spam mỗi frame) — InventoryUI lắng nghe để tự đóng ngay nếu đang mở.
    public static event System.Action OnPlayerSpotted;

    [Header("Model thật -- TỰ SPAWN bằng code lúc Awake(), không cần Instantiate/kéo tay trong scene")]
    [Tooltip("Kéo model THẬT vào đây (VD Assets/_Project/Animations/Monster/Animation/Thuan.fbx). Để trống " +
             "thì fallback về hành vi CŨ: GhostAI tự tìm Animator trên CHÍNH GameObject này (setup thủ công).")]
    [SerializeField] private GameObject _visualPrefab;
    [Tooltip("Animator Controller gán cho model vừa spawn (VD MonsterAnimator.controller). Bỏ trống thì giữ " +
             "nguyên Controller mặc định (nếu có) của chính prefab.")]
    [SerializeField] private RuntimeAnimatorController _animatorController;

    [Header("Waypoints")]
    [SerializeField] private Transform[] _waypoints;
    [Tooltip("BẬT (mặc định) -- đi tuần THEO ĐÚNG THỨ TỰ mảng _waypoints (0,1,2,3... rồi quay lại 0), khớp " +
             "\"lộ trình cố định\" cảnh 3 (Phòng Tiếp Khách -> Hành Lang -> Phòng Ăn -> Salon -> Hành Lang -> " +
             "Tiền Sảnh). TẮT thì quay lại hành vi CŨ (chọn ngẫu nhiên mỗi lần tới điểm).")]
    [SerializeField] private bool _sequentialPatrol = true;

    [Header("Detection")]
    [SerializeField] private float _hearRadius    = 8f;
    [SerializeField] private float _detectRadius  = 12f;
    [SerializeField] private float _fovAngle      = 120f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Speed")]
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _chaseSpeed  = 4f;
    private float _patrolSpeedOriginal;
    private float _hearRadiusOriginal;
    private bool _isAlerted = false;

    [Header("Kill")]
    [SerializeField] private float _killDelay = 0.5f;

    [Header("Jumpscare lúc bắt được Player (sau khi đuổi) -- Game Over cơ bản, KHÔNG phải kết Chapter 1")]
    [SerializeField] private Sprite _jumpscareImage;
    [Tooltip("SFX_Ghost_Jumpscare_Scream_01/02 -- mặc định phát TUẦN TỰ xen kẽ (Jok yêu cầu 2026-07-30), " +
             "không random nữa. Tắt _sequentialScreams bên dưới nếu muốn quay lại random.")]
    [SerializeField] private AudioClip[] _catchScreams;
    [Tooltip("BẬT (mặc định) -- phát _catchScreams TUẦN TỰ (01, 02, 01, 02...). TẮT thì random như bản cũ.")]
    [SerializeField] private bool _sequentialScreams = true;
    private int _screamIndex = 0;

    [Header("Cinematic jumpscare MỚI (freeze + camera shake + đèn chớp) -- chạy TRƯỚC JumpscareGameOverUI")]
    [Tooltip("Ánh sáng jumpscare đặt ở object enemy (VD ngay trước mặt) -- chớp tắt để lộ mặt lúc jumpscare. Để trống thì bỏ qua bước này.")]
    [SerializeField] private Light _jumpscareLight;
    [SerializeField] private int _lightFlickerCount = 4;
    [SerializeField] private float _lightFlickerInterval = 0.08f;
    [Tooltip("Tên Trigger param trong Animator để chuyển sang animation tấn công (để trống thì bỏ qua, chỉ freeze Speed).")]
    [SerializeField] private string _attackTriggerName = "Attack";
    [SerializeField] private float _cameraShakeDuration = 0.6f;
    [SerializeField] private float _cameraShakeMagnitude = 0.25f;
    [Tooltip("Chờ bao lâu sau khi trigger animation tấn công rồi mới freeze cứng -- canh đủ để animation kịp " +
             "chuyển sang đúng tư thế tấn công trước khi đóng băng, không freeze ngay lúc còn đang transition.")]
    [SerializeField] private float _attackFreezeDelay = 0.3f;

    private NavMeshAgent _agent;
    private Animator     _anim;           // THÊM: Biến điều khiển Animator
    private float        _speedParameter; // THÊM: Biến làm mượt chuyển động Blend Tree

    private State        _currentState = State.Patrol;
    private int          _waypointIndex;
    private Transform    _player;
    private Vector3      _lastKnownPosition;
    private float        _investigateTimer;
    private float        _killTimer = 0f;
    private bool         _hasKilled = false; // THÊM: chặn gọi PlayerDead nhiều lần
    private bool         _isOpeningDoor = false; // THÊM: đang dừng lại mở cửa (GhostDoorway) -- chặn path bị ghi đè giữa chừng

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        SpawnVisual();
        _player = GameObject.FindWithTag("Player")?.transform;

        // Lưu giá trị ban đầu để dùng SetAlertMode()
        _patrolSpeedOriginal = _patrolSpeed;
        _hearRadiusOriginal = _hearRadius;
    }

    // THÊM 2026-07-30 (Jok yêu cầu -- "tự add prefab bằng code, chưa hề có cái của Thuận spawn ra bao giờ"):
    // TỰ Instantiate model thật làm con của GhostAI, tự tìm/gán Animator + Controller -- không còn phụ thuộc
    // Animator có sẵn thủ công đúng trên GameObject này (nguồn gốc lỗi "gắn nhầm GhostAI lên GhostCube").
    // _visualPrefab để trống thì fallback nguyên hành vi CŨ (tìm Animator ngay trên chính object này).
    private void SpawnVisual()
    {
        if (_visualPrefab == null)
        {
            _anim = GetComponent<Animator>(); // fallback setup thủ công kiểu cũ
            return;
        }

        var visual = Instantiate(_visualPrefab, transform);
        visual.name = _visualPrefab.name + "_Spawned";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        _anim = visual.GetComponentInChildren<Animator>();
        if (_anim == null) _anim = visual.AddComponent<Animator>();
        if (_animatorController != null) _anim.runtimeAnimatorController = _animatorController;
    }

    // THÊM: reset flag khi scene reload (OnEnable chạy lại)
    private void OnEnable()
    {
        _hasKilled = false;
    }

    private void OnDisable()
    {
        // Tránh treo cờ AnyGhostChasing=true mãi mãi nếu ghost bị tắt/destroy giữa lúc đang Chase.
        if (_currentState == State.Chase) AnyGhostChasing = false;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol:      UpdatePatrol();      break;
            case State.Investigate: UpdateInvestigate(); break;
            case State.Chase:       UpdateChase();       break;
            case State.Kill:        UpdateKill();        break; // THÊM: Cập nhật trạng thái Kill nếu cần
        }

        // Cập nhật hoạt ảnh dựa trên trạng thái và vận tốc thực tế
        UpdateAnimationTransitions();
    }

    private void UpdatePatrol()
    {
        if (_agent == null) return;
        _agent.speed = _patrolSpeed;

        if (_waypoints == null || _waypoints.Length == 0) return;
        if (!_agent.isOnNavMesh) return;

        if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
        {
            if (_sequentialPatrol)
            {
                // Đi tới ĐÚNG waypoint hiện tại trước, RỒI MỚI tăng chỉ số cho lần kế tiếp -- lần đầu tiên
                // (_waypointIndex mặc định 0) sẽ đi thẳng waypoint[0], không bị nhảy cóc qua waypoint[1].
                _agent.SetDestination(_waypoints[_waypointIndex].position);
                _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
            }
            else
            {
                _waypointIndex = Random.Range(0, _waypoints.Length);
                _agent.SetDestination(_waypoints[_waypointIndex].position);
            }
        }

        if (CanDetectPlayer())    EnterChase();
        else if (CanHearPlayer()) EnterInvestigate(_lastKnownPosition);
    }

    private void UpdateInvestigate()
    {
        if (_agent == null) return;

        // Khi đi tới điểm nghi vấn, nó sẽ đứng im tìm kiếm
        if (_agent.remainingDistance < 0.5f)
        {
            _investigateTimer += Time.deltaTime;
            if (_investigateTimer > 8f) EnterPatrol();
        }

        if (CanDetectPlayer()) EnterChase();
    }

    private void UpdateChase()
    {
        if (_agent == null) return;

        if (!CanDetectPlayer() && !CanHearPlayer())
        {
            // Khi mất dấu, gọi hàm chuyển trạng thái có sẵn trong code của bạn
            EnterInvestigate(_lastKnownPosition);
            return;
        }

        _agent.speed = _chaseSpeed;
        if (_agent.isOnNavMesh)
            _agent.SetDestination(_player.position);
    }

    private void UpdateKill()
    {
        // Khi đang thực hiện Jumpscare/Kill, bắt con ma đứng im tại chỗ
        if (_agent != null && _agent.isOnNavMesh)
            _agent.ResetPath();
    }

    private void EnterPatrol()
    {
        _currentState    = State.Patrol;
        AnyGhostChasing  = false;
        if (_agent != null) _agent.speed = _patrolSpeed;
    }

    private void EnterInvestigate(Vector3 pos)
    {
        _currentState       = State.Investigate;
        AnyGhostChasing     = false; // mất dấu, chỉ còn nghi ngờ — chưa hẳn "bám đuổi" nữa
        _lastKnownPosition  = pos;
        _investigateTimer   = 0f;
        if (_agent != null && _agent.isOnNavMesh)
            _agent.SetDestination(pos);
    }

    private void EnterChase()
    {
        bool justSpotted = _currentState != State.Chase;
        _currentState   = State.Chase;
        AnyGhostChasing = true;
        if (_agent != null) _agent.speed = _chaseSpeed;

        if (justSpotted) OnPlayerSpotted?.Invoke();
    }

    /// <summary>
    /// THÊM: Tính toán và cập nhật giá trị Speed truyền vào Blend Tree của Mixamo
    /// </summary>
    private void UpdateAnimationTransitions()
    {
        if (_anim == null) return;

        float targetSpeedValue = 0f;

        // Kiểm tra xem con ma thực tế có đang di chuyển tịnh tiến không
        bool isMoving = _agent != null && _agent.velocity.magnitude > 0.1f && _agent.remainingDistance > _agent.stoppingDistance;

        if (_currentState == State.Kill)
        {
            targetSpeedValue = 0f; // Bắt buộc về Idle khi đang giết Player
        }
        else if (isMoving)
        {
            // Nếu ở trạng thái Đuổi theo (Chase) -> Ép lên mức 2 (Chạy điên cuồng)
            if (_currentState == State.Chase)
            {
                targetSpeedValue = 2f;
            }
            // Nếu đang đi tuần (Patrol) hoặc đang đi bộ tới điểm kiểm tra (Investigate) -> Mức 1 (Đi lê lết)
            else
            {
                targetSpeedValue = 1f;
            }
        }
        else
        {
            // Trường hợp đứng im (Hết điểm tuần / Đang đứng ngó nghiêng ở điểm điều tra) -> Mức 0 (Idle ngó nghiêng)
            targetSpeedValue = 0f;
        }

        // Tạo nội suy mượt mà để con ma đổi từ đi sang chạy không bị giật khung xương
        _speedParameter = Mathf.Lerp(_speedParameter, targetSpeedValue, Time.deltaTime * 5f);
        
        // Đẩy thông số vào Blend Tree
        _anim.SetFloat("Speed", _speedParameter);
    }

    // THÊM 2026-07-30 (Jok yêu cầu -- "trốn tủ thì hiện hint tắt đèn pin, nhưng còn bật đèn pin thì enemy
    // có quyền mở tủ jumpscare luôn"): trước đây IsPlayerHiding CHE HOÀN TOÀN mọi phát hiện bất kể đèn pin
    // bật hay tắt -- trốn xong bật đèn pin vẫn tuyệt đối an toàn, sai thiết kế. Giờ trốn CHỈ thật sự an toàn
    // khi đèn pin đang TẮT -- bật đèn pin trong lúc trốn thì ghost vẫn phát hiện được bình thường.
    private bool IsPlayerHidingSafely()
    {
        var hideComponent = _player.GetComponent<HideSpot>();
        if (hideComponent == null || !hideComponent.IsPlayerHiding) return false;

        var flashlight = Object.FindFirstObjectByType<FlashlightController>();
        bool flashlightOn = flashlight != null && flashlight.IsOn;
        return !flashlightOn; // trốn + đèn TẮT = an toàn. Trốn + đèn BẬT = KHÔNG an toàn nữa.
    }

    private bool CanDetectPlayer()
    {
        if (_player == null) return false;

        if (IsPlayerHidingSafely()) return false;

        Vector3 dirToPlayer = _player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist > _detectRadius) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _fovAngle * 0.5f) return false;

        _lastKnownPosition = _player.position;
        return true;
    }

    private bool CanHearPlayer()
    {
        if (_player == null) return false;

        if (IsPlayerHidingSafely()) return false;

        Rigidbody playerRb = _player.GetComponent<Rigidbody>();
        float playerVelocity = (playerRb != null) ? playerRb.linearVelocity.magnitude : 0f;

        float walkThreshold = 2.0f;

        if (playerVelocity > walkThreshold)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            
            if (dist <= _hearRadius)
            {
                _lastKnownPosition = _player.position;
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _killTimer = 0f;
            return;
        }

        // THÊM (Jok yêu cầu -- "dừng lại mở cửa chứ không phải đi xuyên qua"): trigger đặt tay gần 1 cửa
        // (GhostDoorway) -- ghost dừng lại, tự mở khoá + mở cửa, rồi đi tiếp đúng hướng cũ.
        var doorway = other.GetComponent<GhostDoorway>();
        if (doorway != null && !_isOpeningDoor)
            StartCoroutine(OpenDoorwayRoutine(doorway));
    }

    private IEnumerator OpenDoorwayRoutine(GhostDoorway doorway)
    {
        var door = doorway.Door;
        if (door == null) yield break;
        if (door.IsOpen && !door.IsLocked) yield break; // đã mở sẵn, không cần dừng lại làm gì

        _isOpeningDoor = true;
        Vector3 keepDestination = _agent.hasPath ? _agent.destination : transform.position;
        if (_agent.isOnNavMesh) _agent.isStopped = true;

        door.SetLocked(false);
        door.Open();

        yield return new WaitForSeconds(doorway.OpenDelay);

        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.SetDestination(keepDestination);
        }
        _isOpeningDoor = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_hasKilled) return; // Đã kill rồi thì bỏ qua

        if (HideSpot.AnyPlayerHiding)
        {
            _killTimer = 0f;
            return;
        }

        _killTimer += Time.deltaTime;
        if (_killTimer >= _killDelay)
        {
            Debug.Log("[GhostAI] KILL!");
            _hasKilled    = true; // Đánh dấu đã kill
            _killTimer    = 0f;
            _currentState = State.Kill;

            AudioClip scream = PickCatchScream();
            // Cinematic mới (tiếng hét + freeze attack + camera shake + đèn chớp lộ mặt) chạy TRƯỚC, xong
            // mới tới JumpscareGameOverUI (fade đen + "BẠN ĐÃ CHẾT") -- xem GhostCinematicJumpscare.cs.
            // Scream truyền thẳng vào đây (phát NGAY lúc bắt đầu cinematic) -- KHÔNG truyền lại lần 2 vào
            // JumpscareGameOverUI bên dưới (scream=null) để tránh phát trùng 2 lần.
            GhostCinematicJumpscare.Trigger(_anim, scream, _jumpscareLight, _attackTriggerName, _attackFreezeDelay,
                _lightFlickerCount, _lightFlickerInterval, _cameraShakeDuration, _cameraShakeMagnitude,
                () => JumpscareGameOverUI.Trigger(_jumpscareImage, null, 1.5f, 3f));
        }
    }

    // SỬA (Jok yêu cầu 2026-07-30 -- "chọn tuần tự 2 audio này cho việc trigger jumpscare"): mặc định phát
    // xen kẽ 01/02/01/02... thay vì random như bản cũ. Đặt _sequentialScreams=false để quay lại random.
    private AudioClip PickCatchScream()
    {
        if (_catchScreams == null || _catchScreams.Length == 0) return null;

        if (!_sequentialScreams)
            return _catchScreams[Random.Range(0, _catchScreams.Length)];

        AudioClip clip = _catchScreams[_screamIndex % _catchScreams.Length];
        _screamIndex = (_screamIndex + 1) % _catchScreams.Length;
        return clip;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _killTimer = 0f;
    }

    /// <summary>
    /// Gọi khi piano giải xong (Ch.1) để tăng tốc độ và bán kính nghe.
    /// Được wire từ PianoInteractable.OnSequenceComplete UnityEvent.
    /// </summary>
    public void SetAlertMode()
    {
        if (_isAlerted) return; // Chỉ alert 1 lần

        _isAlerted = true;
        _patrolSpeed = _patrolSpeedOriginal * 1.1f;  // +10% tốc độ patrol
        _hearRadius = _hearRadiusOriginal * 1.25f;  // +25% bán kính nghe

        Debug.Log($"[GhostAI] SetAlertMode() - Patrol speed: {_patrolSpeed:F2}, Hearing radius: {_hearRadius:F2}");
    }
}