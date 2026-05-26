using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Clear Hiding Spot Target", story: "Clear [HidingSpotTarget], [HidingSpotCheckPosition], [HidingSpotLookAtPosition]", category: "Action", id: "74ba95f086c34fe89b58f196030f98da")]
public partial class ClearHidingSpotTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> HidingSpotTarget;
    [SerializeReference] public BlackboardVariable<Vector3> HidingSpotCheckPosition;
    [SerializeReference] public BlackboardVariable<Vector3> HidingSpotLookAtPosition;

    protected override Status OnStart()
    {
        if (HidingSpotTarget != null)
            HidingSpotTarget.Value = null;

        if (HidingSpotCheckPosition != null)
            HidingSpotCheckPosition.Value = default;

        if (HidingSpotLookAtPosition != null)
            HidingSpotLookAtPosition.Value = default;

        return Status.Success;
    }
}
