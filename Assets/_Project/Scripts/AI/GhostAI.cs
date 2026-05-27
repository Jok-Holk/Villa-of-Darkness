using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public enum State { Patrol, Investigate, Chase, Kill }

    [Header("Waypoints")]
    [SerializeField] private Transform[] _waypoints;

    [Header("Detection")]
    [SerializeField] private float _hearingRadius = 8f;
    [SerializeField] private float _sightRadius   = 12f;
    [SerializeField] private float _sightAngle    = 90f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Speed")]
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _chaseSpeed  = 4f;
    private float _patrolSpeedOriginal;
    private float _hearingRadiusOriginal;
    private bool _isAlerted = false;

    [Header("Kill")]
    [SerializeField] private float _killDelay = 0.5f;

    private NavMeshAgent _agent;
    private State        _currentState = State.Patrol;
    private int          _waypointIndex;
    private Transform    _player;
    private Vector3      _lastKnownPosition;
    private float        _investigateTimer;
    private float        _killTimer = 0f;

    private void Awake()
    {
        _agent  = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player")?.transform;
        
        // Lưu giá trị ban đầu để dùng SetAlertMode()
        _patrolSpeedOriginal = _patrolSpeed;
        _hearingRadiusOriginal = _hearingRadius;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol:      UpdatePatrol();      break;
            case State.Investigate: UpdateInvestigate(); break;
            case State.Chase:       UpdateChase();       break;
        }
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

        if (CanDetectPlayer())      EnterChase();
        else if (CanHearPlayer())   EnterInvestigate(_lastKnownPosition);
    }

    private void UpdateInvestigate()
    {
        if (_agent == null) return;

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
            EnterInvestigate(_lastKnownPosition);
            return;
        }

        _agent.speed = _chaseSpeed;
        if (_agent.isOnNavMesh)
            _agent.SetDestination(_player.position);
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

    private bool CanDetectPlayer()
    {
        if (_player == null) return false;
        if (HideSpot.AnyPlayerHiding) return false;

        Vector3 dirToPlayer = _player.position - transform.position;
        float   dist        = dirToPlayer.magnitude;
        if (dist > _sightRadius) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _sightAngle * 0.5f) return false;

        _lastKnownPosition = _player.position;
        return true;
    }

    private bool CanHearPlayer()
    {
        if (_player == null) return false;
        if (HideSpot.AnyPlayerHiding) return false;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > _hearingRadius) return false;

        _lastKnownPosition = _player.position;
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _killTimer = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // DEBUG — in ra mỗi frame ghost đang chạm player
        Debug.Log($"[GhostAI] OnTriggerStay — AnyPlayerHiding={HideSpot.AnyPlayerHiding} | killTimer={_killTimer:F2}");

        if (HideSpot.AnyPlayerHiding)
        {
            _killTimer = 0f;
            return;
        }

        _killTimer += Time.deltaTime;
        if (_killTimer >= _killDelay)
        {
            Debug.Log("[GhostAI] KILL!");
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
        _hearingRadius = _hearingRadiusOriginal * 1.25f;  // +25% bán kính nghe

        Debug.Log($"[GhostAI] SetAlertMode() - Patrol speed: {_patrolSpeed:F2}, Hearing radius: {_hearingRadius:F2}");
    }
}