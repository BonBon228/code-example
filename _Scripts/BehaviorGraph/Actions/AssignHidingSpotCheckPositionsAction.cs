using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Assign Hiding Spot Check Positions", story: "Assign [HidingSpotCheckPosition], [HidingSpotLookAtPosition] from [HidingSpotTarget]", category: "Action", id: "bcab4f451dcf4e55ad24ccbee229e2c7")]
public partial class AssignHidingSpotCheckPositionsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;
    [SerializeReference] public BlackboardVariable<Vector3> HidingSpotCheckPosition;
    [SerializeReference] public BlackboardVariable<Vector3> HidingSpotLookAtPosition;

    protected override Status OnUpdate()
    {
        if (HidingSpotTarget?.Value == null || HidingSpotCheckPosition == null || HidingSpotLookAtPosition == null)
            return Status.Failure;

        if (!HidingSpotTarget.Value.TryGetComponent(out HidingSpot hidingSpot))
            return Status.Failure;

        HidingSpotCheckPosition.Value = hidingSpot.AILookTransform.position;
        HidingSpotLookAtPosition.Value = hidingSpot.LookAtTransform.position;
        return Status.Success;
    }
}
