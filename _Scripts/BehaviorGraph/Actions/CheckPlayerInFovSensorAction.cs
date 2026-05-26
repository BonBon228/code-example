using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Player In FOVSensor", story: "[FieldOfViewSensor] detects and assigns [Target], [TargetPosition], [HasLastKnownPosition]", category: "Action", id: "d22987941d2310ba4ed3b5eb401fd2ea")]
public partial class CheckPlayerInFovSensorAction : Action
{
    [SerializeReference] public BlackboardVariable<FieldOfViewSensor> FieldOfViewSensor;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPosition;

    private readonly GameObject[] _playersBuffer = new GameObject[8];

    protected override Status OnUpdate()
    {
        if (FieldOfViewSensor?.Value == null || Target == null)
            return Status.Failure;

        AssignLastKnownPosition();

        int playersCount = FieldOfViewSensor.Value.Filter(_playersBuffer, "Player");
        if (playersCount == 0)
        {
            Target.Value = null;
            return Status.Failure;
        }

        Vector3 sensorPosition = FieldOfViewSensor.Value.transform.position;
        GameObject nearestPlayer = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < playersCount; i++)
        {
            GameObject player = _playersBuffer[i];
            if (player == null)
                continue;

            float distanceSqr = (player.transform.position - sensorPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestPlayer = player;
            }
        }

        if (nearestPlayer == null)
        {
            Target.Value = null;
            return Status.Failure;
        }

        Target.Value = nearestPlayer;
        return Status.Success;
    }

    private void AssignLastKnownPosition()
    {
        LastKnownPositionBehavior lastKnownPositionBehavior = GetLastKnownPositionBehavior();
        if (lastKnownPositionBehavior == null || !lastKnownPositionBehavior.TryGetLastKnownPosition(out Vector3 lastKnownPosition))
        {
            if (HasLastKnownPosition != null)
                HasLastKnownPosition.Value = false;

            return;
        }

        if (TargetPosition != null)
            TargetPosition.Value = lastKnownPosition;

        if (HasLastKnownPosition != null)
            HasLastKnownPosition.Value = true;
    }

    private LastKnownPositionBehavior GetLastKnownPositionBehavior()
    {
        return FieldOfViewSensor.Value.GetComponentInParent<LastKnownPositionBehavior>();
    }
}
