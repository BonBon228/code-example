using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wave Spell Cast", story: "[Self] casts wave spell at [Target]", category: "Action", id: "4b91e67c4fa6408498305ef3139d6b2e")]
public partial class WaveSpellCastAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private WaveSpellCastBehavior _spellCastBehavior;
    private NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        if (Self.Value == null || Target.Value == null)
            return Status.Failure;

        if (!Self.Value.TryGetComponent(out _spellCastBehavior))
            return Status.Failure;

        if (!_spellCastBehavior.TryStartCast(Target.Value))
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
        if (Self.Value == null || Target.Value == null || _spellCastBehavior == null)
            return Status.Failure;

        return _spellCastBehavior.IsCastCompleted ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_spellCastBehavior != null)
            _spellCastBehavior.CancelCast();

        if (_navMeshAgent != null)
        {
            _navMeshAgent.updateRotation = true;
            _navMeshAgent.isStopped = false;
        }
    }
}
