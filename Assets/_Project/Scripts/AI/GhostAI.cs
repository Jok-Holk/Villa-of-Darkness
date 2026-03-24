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
    [SerializeField] private float _patrolSpeed  = 1.5f;
    [SerializeField] private float _chaseSpeed   = 4f;

    private NavMeshAgent _agent;
    private State _currentState = State.Patrol;
    private int _waypointIndex;
    private Transform _player;
    private Vector3 _lastKnownPosition;
    private float _investigateTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol:     UpdatePatrol();     break;
            case State.Investigate:UpdateInvestigate();break;
            case State.Chase:      UpdateChase();      break;
        }
    }

    // ── PATROL ───────────────────────────────────────────
    private void UpdatePatrol()
    {
        _agent.speed = _patrolSpeed;

        if (_waypoints == null || _waypoints.Length == 0) return;
        if (!_agent.isOnNavMesh) return; 

        if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
        {
            _waypointIndex = Random.Range(0, _waypoints.Length);
            _agent.SetDestination(_waypoints[_waypointIndex].position);
        }

        if (CanDetectPlayer())
            EnterChase();
        else if (CanHearPlayer())
            EnterInvestigate(_lastKnownPosition);
    }

    // ── INVESTIGATE ──────────────────────────────────────
    private void UpdateInvestigate()
    {
        if (_agent.remainingDistance < 0.5f)
        {
            _investigateTimer += Time.deltaTime;
            if (_investigateTimer > 8f) EnterPatrol();
        }

        if (CanDetectPlayer()) EnterChase();
    }

    // ── CHASE ────────────────────────────────────────────
    private void UpdateChase()
    {
        _agent.speed = _chaseSpeed;
        if (_agent.isOnNavMesh)
            _agent.SetDestination(_player.position);

        if (!CanDetectPlayer() && !CanHearPlayer())
        {
            EnterInvestigate(_player.position);
        }
    }

    // ── TRANSITIONS ──────────────────────────────────────
    private void EnterPatrol()
    {
        _currentState = State.Patrol;
        _agent.speed = _patrolSpeed;
    }

    private void EnterInvestigate(Vector3 pos)
    {
        _currentState = State.Investigate;
        _lastKnownPosition = pos;
        _investigateTimer = 0f;
        if (_agent.isOnNavMesh)
            _agent.SetDestination(pos);
    }

    private void EnterChase()
    {
        _currentState = State.Chase;
        _agent.speed = _chaseSpeed;
    }

    // ── DETECTION ────────────────────────────────────────
    private bool CanDetectPlayer()
    {
        if (_player == null) return false;

        Vector3 dirToPlayer = _player.position - transform.position;
        float dist = dirToPlayer.magnitude;
        if (dist > _sightRadius) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _sightAngle * 0.5f) return false;

        _lastKnownPosition = _player.position;
        return true;
    }

    private bool CanHearPlayer()
    {
        if (_player == null) return false;
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > _hearingRadius) return false;
        _lastKnownPosition = _player.position;
        return true;
    }

    // ── KILL ─────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _currentState = State.Kill;
            GameManager.Instance.PlayerDead();
        }
    }
}