using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Clear Last Known Position", story: "Clear [LastKnownPosition], [HasLastKnownPosition]", category: "Action", id: "0d53db12d35448f5a43a151332178e47")]
public partial class ClearLastKnownPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPosition;

    protected override Status OnStart()
    {
        if (LastKnownPosition != null)
            LastKnownPosition.Value = default;

        if (HasLastKnownPosition != null)
            HasLastKnownPosition.Value = false;

        return Status.Success;
    }
}
