using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Select Random Hiding Spot In Area", story: "[Self] selects random [HidingSpotTarget] in area", category: "Action", id: "1d6ef488122a4f19a43d8b116d7210c8")]
public partial class SelectRandomHidingSpotInAreaAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || HidingSpotTarget == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out HidingSpotSensor hidingSpotSensor))
            return Status.Failure;

        if (hidingSpotSensor.TryGetRandomHidingSpotInArea(Self.Value.transform.position, out GameObject hidingSpotTarget))
        {
            HidingSpotTarget.Value = hidingSpotTarget;
            return Status.Success;
        }

        HidingSpotTarget.Value = null;
        return Status.Failure;
    }
}
