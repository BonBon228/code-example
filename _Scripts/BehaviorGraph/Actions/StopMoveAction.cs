using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopMove", story: "[Agent] stops movement", category: "Action", id: "289f0a0ec6654d58b5f4f757eaee92de")]
public partial class StopMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        AgentMoveBehavior moveBehavior = Agent.Value.GetComponent<AgentMoveBehavior>();
        if (moveBehavior == null)
            return Status.Failure;

        moveBehavior.ClearMoveTarget();
        return Status.Success;
    }
}
