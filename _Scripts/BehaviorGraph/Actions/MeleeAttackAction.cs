using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Melee Attack", story: "[Self] melee attacks [Target]", category: "Action", id: "f4374d7e6c9141399f9289d965b2dd1a")]
public partial class MeleeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private MeleeAttackBehavior _meleeAttackBehavior;
    private NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        if (Self.Value == null || Target.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _meleeAttackBehavior))
            return Status.Failure;

        if (!_meleeAttackBehavior.TryStartAttack(Target.Value))
            return Status.Failure;

        Self.Value.TryGetComponent(out _navMeshAgent);
        if (_navMeshAgent != null)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.updateRotation = false;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || Target.Value == null || _meleeAttackBehavior == null)
            return Status.Failure;

        return _meleeAttackBehavior.IsAttackCompleted ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_meleeAttackBehavior != null)
            _meleeAttackBehavior.CancelAttack();

        if (_navMeshAgent != null)
        {
            _navMeshAgent.updateRotation = true;
            _navMeshAgent.isStopped = false;
        }
    }
}
