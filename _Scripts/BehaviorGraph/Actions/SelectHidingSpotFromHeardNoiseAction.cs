using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Select Hiding Spot From Heard Noise", story: "[Self] selects [HidingSpotTarget] from heard noise", category: "Action", id: "55f6da2a1a1640b981ce5c9ffec86212")]
public partial class SelectHidingSpotFromHeardNoiseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;
    [SerializeReference] public BlackboardVariable<float> PositionCheckRadius = new(0.05f);

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || HidingSpotTarget == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out HeardNoiseMemory heardNoiseMemory))
            return Status.Failure;

        if (!heardNoiseMemory.TryGetHeardPosition(out Vector3 heardPosition))
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out HidingSpotSensor hidingSpotSensor))
            return Status.Failure;

        if (hidingSpotSensor.TryGetHidingSpotFromHeardPosition(heardPosition, PositionCheckRadius.Value, out GameObject hidingSpotTarget))
        {
            HidingSpotTarget.Value = hidingSpotTarget;
            return Status.Success;
        }

        HidingSpotTarget.Value = null;
        return Status.Failure;
    }
}
