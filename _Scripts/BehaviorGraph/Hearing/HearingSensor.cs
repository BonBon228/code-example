using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(HeardNoiseMemory))]
public class HearingSensor : NetworkBehaviour
{
    [SerializeField] private float _hearingMultiplier = 1f;
    [SerializeField] private float _baseUncertaintyRadius = 0.5f;
    [SerializeField] private float _distanceUncertaintyMultiplier = 0.08f;
    [SerializeField] private float _minimumPerceivedSoundIntensityToRemember = 0.05f;
    [SerializeField] private LayerMask _occlusionMask;
    [SerializeField, Range(0f, 1f)] private float _occlusionSoundIntensityMultiplier = 0.35f;
    [SerializeField] private Color _noiseSourceGizmoColor = Color.yellow;
    [SerializeField] private Color _heardPositionGizmoColor = Color.blue;
    [SerializeField, Min(0f)] private float _heardPositionGizmoRadius = 0.2f;

    private HeardNoiseMemory _memory;
    private NoiseOcclusionEvaluator _occlusionEvaluator;

    private void Awake()
    {
        _memory = GetComponent<HeardNoiseMemory>();
        _occlusionEvaluator = new NoiseOcclusionEvaluator(_occlusionMask, _occlusionSoundIntensityMultiplier);
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        enabled = true;
        NoiseSystem.Instance.NoiseEmitted += OnNoiseEmitted;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        NoiseSystem.Instance.NoiseEmitted -= OnNoiseEmitted;
    }

    private void OnNoiseEmitted(NoiseEvent noiseEvent)
    {
        if (noiseEvent.Source == gameObject)
            return;

        Vector3 listenerPosition = transform.position;
        float distance = Vector3.Distance(listenerPosition, noiseEvent.Position);
        float effectiveRadius = noiseEvent.Radius * Mathf.Max(0f, _hearingMultiplier);
        if (distance > effectiveRadius)
            return;

        float occlusionMultiplier = _occlusionEvaluator.Evaluate(listenerPosition, noiseEvent.Position);
        float distanceFalloffMultiplier = Mathf.Clamp01(1f - distance / Mathf.Max(effectiveRadius, 0.0001f));
        float perceivedSoundIntensity = distanceFalloffMultiplier * noiseEvent.SoundIntensity * occlusionMultiplier;
        if (perceivedSoundIntensity < _minimumPerceivedSoundIntensityToRemember)
            return;

        float uncertaintyRadius = _baseUncertaintyRadius + distance * _distanceUncertaintyMultiplier;
        if (occlusionMultiplier < 1f)
            uncertaintyRadius /= Mathf.Max(occlusionMultiplier, 0.0001f);

        Vector2 randomOffset = Random.insideUnitCircle * uncertaintyRadius;
        Vector3 heardPosition = noiseEvent.Position + new Vector3(randomOffset.x, 0f, randomOffset.y);
        _memory.Remember(noiseEvent, heardPosition, uncertaintyRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (_memory == null)
            _memory = GetComponent<HeardNoiseMemory>();

        if (_memory == null || !_memory.HasHeardNoise)
            return;

        Gizmos.color = _noiseSourceGizmoColor;
        Gizmos.DrawWireSphere(_memory.NoisePosition, _memory.UncertaintyRadius);

        Gizmos.color = _heardPositionGizmoColor;
        Gizmos.DrawWireSphere(_memory.HeardPosition, _heardPositionGizmoRadius);
    }
}
