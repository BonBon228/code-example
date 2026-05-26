using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Clear Heard Noise", story: "[Self] clears heard noise memory, [HeardPosition] and [HasHeardPosition]", category: "Action", id: "4dfb43bd749746248684e1f3d58c8f48")]
public partial class ClearHeardNoiseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> HeardPosition;
    [SerializeReference] public BlackboardVariable<bool> HasHeardPosition;

    protected override Status OnStart()
    {
        if (Self.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out HeardNoiseMemory heardNoiseMemory))
            return Status.Failure;

        heardNoiseMemory.Clear();

        if (HeardPosition != null)
            HeardPosition.Value = default;

        if (HasHeardPosition != null)
            HasHeardPosition.Value = false;

        return Status.Success;
    }
}
