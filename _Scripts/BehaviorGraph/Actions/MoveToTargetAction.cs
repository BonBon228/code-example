using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Target", story: "[Self] moves to [Target]", category: "Action", id: "03c41d85e3b7399ae3b393d4334676f7")]
public partial class MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance = new(1.2f);
    [SerializeReference] public BlackboardVariable<bool> SetRunningMode = new(true);

    private AgentMoveBehavior _agentMoveBehavior;
    private float _originalStoppingDistance;
    private bool _hasOriginalStoppingDistance;

    protected override Status OnStart()
    {
        if (Self.Value == null || Target.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _agentMoveBehavior))
            return Status.Failure;

        _originalStoppingDistance = _agentMoveBehavior.CurrentStoppingDistance;
        _hasOriginalStoppingDistance = true;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || Target.Value == null)
            return Status.Failure;

        _agentMoveBehavior.SetMoveTarget(Target.Value.transform.position, StoppingDistance.Value);
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

