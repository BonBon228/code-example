using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Position", story: "[Self] moves to [TargetPosition]", category: "Action", id: "e8d4a2e86e5047d38f6b9b6fdb2d19f1")]
public partial class MoveToPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance = new(0.6f);
    [SerializeReference] public BlackboardVariable<bool> SetRunningMode = new(true);

    private AgentMoveBehavior _agentMoveBehavior;
    private float _originalStoppingDistance;
    private bool _hasOriginalStoppingDistance;

    protected override Status OnStart()
    {
        if (Self.Value == null || TargetPosition == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _agentMoveBehavior))
            return Status.Failure;

        _originalStoppingDistance = _agentMoveBehavior.CurrentStoppingDistance;
        _hasOriginalStoppingDistance = true;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || TargetPosition == null)
            return Status.Failure;

        _agentMoveBehavior.SetMoveTarget(TargetPosition.Value, StoppingDistance.Value);
        _agentMoveBehavior.SetRunningMode(SetRunningMode.Value);
        return _agentMoveBehavior.IsAtTarget ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_hasOriginalStoppingDistance && _agentMoveBehavior != null)
            _agentMoveBehavior.SetStoppingDistance(_originalStoppingDistance);

        _hasOriginalStoppingDistance = false;
    }
}
