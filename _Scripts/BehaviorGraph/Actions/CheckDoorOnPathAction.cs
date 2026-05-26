using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Door On Path", story: "[DoorOnPathDetector] assigns [DoorOpeningTarget], [DoorOpenPosition], [DoorLookAtPosition]", category: "Action", id: "e686ee351d62900be9d949c21256d3dc")]
public partial class CheckDoorOnPathAction : Action
{
    [SerializeReference] public BlackboardVariable<DoorOnPathDetector> DoorOnPathDetector;
    [SerializeReference] public BlackboardVariable<GameObject> DoorOpeningTarget;
    [SerializeReference] public BlackboardVariable<Vector3> DoorOpenPosition;
    [SerializeReference] public BlackboardVariable<Vector3> DoorLookAtPosition;

    protected override Status OnUpdate()
    {
        if (DoorOnPathDetector == null || DoorOnPathDetector.Value == null)
            return Status.Failure;

        if (DoorOnPathDetector.Value.TryGetDoorOnPath(out DoorOpening doorOpening))
        {
            DoorOpenPosition.Value = doorOpening.GetAIOpenTransform(DoorOnPathDetector.Value.transform.position).position;
            DoorLookAtPosition.Value = doorOpening.CenterTransform.position;
            DoorOpeningTarget.Value = doorOpening.gameObject;
            return Status.Success;
        }
        else
        {
            DoorOpenPosition.Value = default;
            DoorLookAtPosition.Value = default;
            DoorOpeningTarget.Value = null;
            return Status.Failure;
        }
    }
}
