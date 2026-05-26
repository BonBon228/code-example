using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Heard Noise", story: "[Self] checks heard noise and assigns [HeardPosition], [HasHeardPosition], [HeardNoiseAge]", category: "Action", id: "feb93cf35ee44b22a91cae0f5c50009d")]
public partial class CheckHeardNoiseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> HeardPosition;
    [SerializeReference] public BlackboardVariable<bool> HasHeardPosition;
    [SerializeReference] public BlackboardVariable<float> HeardNoiseAge;

    protected override Status OnUpdate()
    {
        if (Self.Value == null)
            return Fail();

        if (!Self.Value.TryGetComponent(out HeardNoiseMemory heardNoiseMemory))
            return Fail();

        if (!heardNoiseMemory.TryGetHeardPosition(out Vector3 heardPosition))
            return Fail();

        if (HeardPosition != null)
            HeardPosition.Value = heardPosition;

        if (HasHeardPosition != null)
            HasHeardPosition.Value = true;

        if (HeardNoiseAge != null)
            HeardNoiseAge.Value = heardNoiseMemory.HeardNoiseAge;

        return Status.Success;
    }

    private Status Fail()
    {
        if (HasHeardPosition != null)
            HasHeardPosition.Value = false;

        if (HeardNoiseAge != null)
            HeardNoiseAge.Value = float.PositiveInfinity;

        return Status.Failure;
    }
}
