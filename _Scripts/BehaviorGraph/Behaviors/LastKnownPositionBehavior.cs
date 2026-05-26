using Unity.Netcode;
using UnityEngine;

public class LastKnownPositionBehavior : NetworkBehaviour
{
    [SerializeField] private FieldOfViewSensor _fieldOfViewSensor;

    public Vector3 LastKnownPosition { get; private set; }
    public bool HasLastKnownPosition { get; private set; }
    // public bool IsVisitedLastPosition { get; set; }

    private void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            enabled = true;
    }

    private void Update()
    {
        RefreshLastKnownPosition();
    }

    public bool TryGetLastKnownPosition(out Vector3 lastKnownPosition)
    {
        RefreshLastKnownPosition();
        lastKnownPosition = LastKnownPosition;
        return HasLastKnownPosition;
    }

    public void Clear()
    {
        LastKnownPosition = default;
        HasLastKnownPosition = false;
    }

    private void RefreshLastKnownPosition()
    {
        if (_fieldOfViewSensor == null)
            return;

        Vector3 sensorLastKnownPosition = _fieldOfViewSensor.GetLastKnownPosition();
        if (sensorLastKnownPosition == Vector3.zero)
            return;

        LastKnownPosition = sensorLastKnownPosition;
        HasLastKnownPosition = true;
    }
}
