using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Clear Witnessed Hiding Spot", story: "[Self] clears witnessed [HidingSpotTarget]", category: "Action", id: "b7e222e42c684360ace16509c18712e8")]
public partial class ClearWitnessedHidingSpotAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;

    protected override Status OnStart()
    {
        if (Self?.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out HidingSpotSensor hidingSpotSensor))
            return Status.Failure;

        hidingSpotSensor.ClearWitnessedHidingSpot();

        if (HidingSpotTarget != null)
            HidingSpotTarget.Value = null;

        return Status.Success;
    }
}
