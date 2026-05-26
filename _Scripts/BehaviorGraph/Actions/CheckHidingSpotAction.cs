using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Hiding Spot", story: "[Self] checks [HidingSpotTarget]", category: "Action", id: "8778bc7d1e704c42ab71ba4b21e3e8c0")]
public partial class CheckHidingSpotAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;

    private HidingSpotBehavior _hidingSpotBehavior;

    protected override Status OnStart()
    {
        if (Self.Value == null || HidingSpotTarget?.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _hidingSpotBehavior))
            return Status.Failure;

        if (!_hidingSpotBehavior.StartHidingSpotCheck(HidingSpotTarget.Value))
            return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || HidingSpotTarget?.Value == null || _hidingSpotBehavior == null)
            return Status.Failure;

        return _hidingSpotBehavior.IsCheckAnimationEnded ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_hidingSpotBehavior != null)
            _hidingSpotBehavior.ResetHidingSpotCheck();
    }
}
