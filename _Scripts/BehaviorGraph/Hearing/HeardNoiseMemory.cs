using Unity.Netcode;
using UnityEngine;

public class HeardNoiseMemory : NetworkBehaviour
{
    [SerializeField] private float _memoryDuration = 8f;

    public Vector3 NoisePosition { get; private set; }
    public Vector3 HeardPosition { get; private set; }
    public float UncertaintyRadius { get; private set; }
    public NoiseType NoiseType { get; private set; }
    public GameObject Source { get; private set; }
    public float LastHeardTime { get; private set; } = float.NegativeInfinity;
    public bool HasHeardNoise => Time.time - LastHeardTime <= _memoryDuration;
    public float HeardNoiseAge => HasHeardNoise ? Time.time - LastHeardTime : float.PositiveInfinity;

    private void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            enabled = true;
    }

    public void Remember(NoiseEvent noiseEvent, Vector3 heardPosition, float uncertaintyRadius)
    {
        NoisePosition = noiseEvent.Position;
        HeardPosition = heardPosition;
        UncertaintyRadius = Mathf.Max(0f, uncertaintyRadius);
        NoiseType = noiseEvent.Type;
        Source = noiseEvent.Source;
        LastHeardTime = Time.time;
    }

    public bool TryGetHeardPosition(out Vector3 heardPosition)
    {
        if (!HasHeardNoise)
        {
            heardPosition = default;
            return false;
        }

        heardPosition = HeardPosition;
        return true;
    }

    public void Clear()
    {
        NoisePosition = default;
        HeardPosition = default;
        UncertaintyRadius = 0f;
        NoiseType = default;
        Source = null;
        LastHeardTime = float.NegativeInfinity;
    }
}
