using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] // ĐẢM BẢO: Luôn có Animator đi kèm
public class GhostAI : MonoBehaviour
{
    public enum State { Patrol, Investigate, Chase, Kill }

    [Header("Waypoints")]
    [SerializeField] private Transform[] _waypoints;

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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim  = GetComponent<Animator>(); // THÊM: Lấy thành phần Animator
        _player = GameObject.FindWithTag("Player")?.transform;
        
        // Lưu giá trị ban đầu để dùng SetAlertMode()
        _patrolSpeedOriginal = _patrolSpeed;
        _hearRadiusOriginal = _hearRadius;
    }

    // THÊM: reset flag khi scene reload (OnEnable chạy lại)
    private void OnEnable()
    {
        _hasKilled = false;
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
            _waypointIndex = Random.Range(0, _waypoints.Length);
            _agent.SetDestination(_waypoints[_waypointIndex].position);
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
        _currentState = State.Patrol;
        if (_agent != null) _agent.speed = _patrolSpeed;
    }

    private void EnterInvestigate(Vector3 pos)
    {
        _currentState      = State.Investigate;
        _lastKnownPosition = pos;
        _investigateTimer  = 0f;
        if (_agent != null && _agent.isOnNavMesh)
            _agent.SetDestination(pos);
    }

    private void EnterChase()
    {
        _currentState = State.Chase;
        if (_agent != null) _agent.speed = _chaseSpeed;
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

    private bool CanDetectPlayer()
    {
        if (_player == null) return false;

        var hideComponent = _player.GetComponent<HideSpot>();
        if (hideComponent != null && hideComponent.IsPlayerHiding) return false;

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

        var hideComponent = _player.GetComponent<HideSpot>();
        if (hideComponent != null && hideComponent.IsPlayerHiding) return false;

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
            _killTimer = 0f;
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
            GameManager.Instance?.PlayerDead();
        }
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