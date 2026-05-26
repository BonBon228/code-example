using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Calculate Distance To Target", story: "[Self] calculates distance to [Target] and stores [DistanceToTarget]", category: "Action", id: "d12660c57c5844ff9eaccf6d1c180569")]
public partial class CalculateDistanceToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget;

    protected override Status OnUpdate()
    {
        if (DistanceToTarget == null)
            return Status.Failure;

        if (Self.Value == null || Target.Value == null)
        {
            DistanceToTarget.Value = float.PositiveInfinity;
            return Status.Failure;
        }

        DistanceToTarget.Value = Vector3.Distance(Self.Value.transform.position, Target.Value.transform.position);
        return Status.Success;
    }
}
