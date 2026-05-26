using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Door Kick Action", story: "[Self] kicks out [DoorOpeningTarget]", category: "Action", id: "39350d40d33c8a9df57b4855eb472190")]
public partial class DoorKickAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> DoorOpeningTarget;

    private DoorOpenBehavior _doorOpenBehavior;
    private DoorOpening _doorOpening;
    private NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        if (Self.Value == null || DoorOpeningTarget?.Value == null)
            return Status.Failure;

        if (!DoorOpeningTarget.Value.TryGetComponent(out _doorOpening))
            return Status.Failure;

        if (_doorOpening.IsOpen)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _navMeshAgent) || !Self.Value.TryGetComponent(out _doorOpenBehavior))
            return Status.Failure;

        _navMeshAgent.updateRotation = false;
        _doorOpenBehavior.OpenDoor(_doorOpening);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || _doorOpening == null)
            return Status.Failure;

        return _doorOpenBehavior.IsDoorKickCompleted ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_navMeshAgent != null)
            _navMeshAgent.updateRotation = true;
    }
}

