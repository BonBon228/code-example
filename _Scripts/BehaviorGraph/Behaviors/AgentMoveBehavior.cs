using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AgentMoveBehavior : NetworkBehaviour
{
    [SerializeField] private float _locomotionChangeSpeed = 1f;
    [SerializeField] private float _destinationUpdateThreshold = 0.15f;
    [SerializeField] private float _arriveDistanceOffset = 0.05f;

    private bool _hasMoveTarget;
    private Vector3 _moveTargetPosition;
    private Vector2 _velocity;
    private Vector2 _smoothDeltaPostion;
    private float _locomotionValue = 0f;
    private bool _shouldMove = false;
    private bool _isRunning = false;
    private float _lastRequestedStoppingDistance;

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    private NavMeshAgent NavMeshAgent => _navMeshAgent;
    private Animator Animator => _animator;

    private static readonly int WALK = Animator.StringToHash("IsWalking");
    private static readonly int LOCOMOTION = Animator.StringToHash("Locomotion");

    public bool HasMoveTarget => _hasMoveTarget;
    public Vector3 MoveTargetPosition => _moveTargetPosition;
    public float CurrentStoppingDistance => NavMeshAgent.stoppingDistance;
    public bool IsAtTarget =>
        _hasMoveTarget &&
        !NavMeshAgent.pathPending &&
        NavMeshAgent.remainingDistance <= Mathf.Max(NavMeshAgent.stoppingDistance + _arriveDistanceOffset, _arriveDistanceOffset);

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        Animator.applyRootMotion = false;
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            NavMeshAgent.enabled = true;
            enabled = true;

            Animator.applyRootMotion = true;
            NavMeshAgent.updatePosition = false;
            NavMeshAgent.updateRotation = true;
        }
    }

    public void SetMoveTarget(Vector3 position, float? stoppingDistance = null)
    {
        _hasMoveTarget = true;
        _moveTargetPosition = position;

        if (stoppingDistance.HasValue)
        {
            _lastRequestedStoppingDistance = Mathf.Max(0f, stoppingDistance.Value);
            NavMeshAgent.stoppingDistance = _lastRequestedStoppingDistance;
        }

        RequestDestinationUpdate(force: true);
    }

    public void SetStoppingDistance(float stoppingDistance)
    {
        NavMeshAgent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
    }

    public void ClearMoveTarget()
    {
        _hasMoveTarget = false;
        NavMeshAgent.ResetPath();
    }

    public void SetRunningMode(bool enabled)
    {
        _isRunning = enabled;
    }

    public bool TryGetCurrentPathStatus(out NavMeshPathStatus status)
    {
        status = NavMeshAgent.pathStatus;
        return NavMeshAgent.hasPath;
    }

    private void OnAnimatorMove()
    {
        Vector3 rootPosition = Animator.rootPosition;
        rootPosition.y = NavMeshAgent.nextPosition.y;
        transform.position = rootPosition;
        NavMeshAgent.nextPosition = rootPosition;
    }

    private void Update()
    {
        if (_hasMoveTarget)
            RequestDestinationUpdate(force: false);

        SyncronizeAnimation();
    }

    private void RequestDestinationUpdate(bool force)
    {
        if (!_hasMoveTarget)
            return;

        if (force || !NavMeshAgent.hasPath)
        {
            NavMeshAgent.SetDestination(_moveTargetPosition);
            return;
        }

        if (Vector3.Distance(NavMeshAgent.destination, _moveTargetPosition) > _destinationUpdateThreshold)
            NavMeshAgent.SetDestination(_moveTargetPosition);
    }

    private void SyncronizeAnimation()
    {
        Vector3 worldDeltaPosition = NavMeshAgent.nextPosition - transform.position;
        worldDeltaPosition.y = 0;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.1f);
        _smoothDeltaPostion = Vector2.Lerp(_smoothDeltaPostion, deltaPosition, smooth);

        _velocity = _smoothDeltaPostion / Mathf.Max(Time.deltaTime, 0.0001f);
        if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
        {
            float stoppingDistance = Mathf.Max(NavMeshAgent.stoppingDistance, 0.0001f);
            _velocity = Vector2.Lerp(Vector2.zero, _velocity, NavMeshAgent.remainingDistance / stoppingDistance);
        }

        _shouldMove = _velocity.magnitude > 0.5f && NavMeshAgent.remainingDistance > NavMeshAgent.stoppingDistance;

        UpdateLocomotion();
        Animator.SetBool(WALK, _shouldMove);
        Animator.SetFloat(LOCOMOTION, _locomotionValue);

        float deltaMagnitude = worldDeltaPosition.magnitude;
        if(deltaMagnitude > NavMeshAgent.radius / 2f)
            transform.position = Vector3.Lerp(Animator.rootPosition, NavMeshAgent.nextPosition, smooth);
    }

    private void UpdateLocomotion()
    {
        float targetValue = _isRunning ? 1f : 0f;
        _locomotionValue = Mathf.MoveTowards(_locomotionValue, targetValue, _locomotionChangeSpeed * Time.deltaTime);
    }
}
