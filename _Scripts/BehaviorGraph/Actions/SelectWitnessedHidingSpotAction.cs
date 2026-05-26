using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Select Witnessed Hiding Spot", story: "[Self] selects witnessed [HidingSpotTarget]", category: "Action", id: "a12f92e0e69d44f28f4c996d6df28a60")]
public partial class SelectWitnessedHidingSpotAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || HidingSpotTarget == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out HidingSpotSensor hidingSpotSensor))
            return Status.Failure;

        if (hidingSpotSensor.TryGetWitnessedHidingSpotTarget(out GameObject hidingSpotTarget))
        {
            HidingSpotTarget.Value = hidingSpotTarget;
            return Status.Success;
        }

        HidingSpotTarget.Value = null;
        return Status.Failure;
    }
}
