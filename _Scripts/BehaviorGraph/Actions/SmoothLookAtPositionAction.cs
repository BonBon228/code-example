using System;
using DG.Tweening;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Smooth Look At Position", story: "[Self] smooth looks at [LookAtPosition]", category: "Action", id: "b730f58e2a994c39b6a5f18ecdb8df04")]
public partial class SmoothLookAtPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> LookAtPosition;
    [SerializeReference] public BlackboardVariable<float> RotationDuration = new(0.5f);

    private NavMeshAgent _navMeshAgent;
    private Tween _lookAtTween;
    private bool _isLookCompleted;

    protected override Status OnStart()
    {
        if (Self?.Value == null || LookAtPosition == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _navMeshAgent))
            return Status.Failure;

        _navMeshAgent.updateRotation = false;
        _navMeshAgent.isStopped = true;
        _isLookCompleted = false;

        _lookAtTween = Self.Value.transform
            .DOLookAt(LookAtPosition.Value, RotationDuration.Value, AxisConstraint.Y)
            .OnComplete(() => _isLookCompleted = true);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || LookAtPosition == null)
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
