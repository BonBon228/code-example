using System;
using Unity.Behavior;
using UnityEngine;
using DG.Tweening;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Smooth Look At", story: "[Self] smooth looks at [LookAtTarget]", category: "Action", id: "319e39c72327de7c7939b96cf9ae415d")]
public partial class SmoothLookAtAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> LookAtTarget;
    [SerializeReference] public BlackboardVariable<float> RotationDuration = new(0.5f);

    private NavMeshAgent _navMeshAgent;
    private Tween _lookAtTween;
    private bool _isLookCompleted;

    protected override Status OnStart()
    {
        if (Self.Value == null || LookAtTarget.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _navMeshAgent))
            return Status.Failure;

        _navMeshAgent.updateRotation = false;
        _navMeshAgent.isStopped = true;
        _isLookCompleted = false;

        _lookAtTween = Self.Value.transform
            .DOLookAt(LookAtTarget.Value.transform.position, RotationDuration.Value)
            .OnComplete(() => _isLookCompleted = true);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || LookAtTarget.Value == null)
            return Status.Failure;

        return _isLookCompleted ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_lookAtTween != null && _lookAtTween.IsActive())
            _lookAtTween.Kill(false);

        if (_navMeshAgent != null)
        {
            _navMeshAgent.updateRotation = true;
            _navMeshAgent.isStopped = false;
        }
    }
}

