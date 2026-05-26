using UnityEngine;

public class NoiseOcclusionEvaluator
{
    private readonly LayerMask _occlusionMask;
    private readonly float _occlusionSoundIntensityMultiplier;

    public NoiseOcclusionEvaluator(LayerMask occlusionMask, float occlusionSoundIntensityMultiplier)
    {
        _occlusionMask = occlusionMask;
        _occlusionSoundIntensityMultiplier = Mathf.Clamp01(occlusionSoundIntensityMultiplier);
    }

    public float Evaluate(Vector3 listenerPosition, Vector3 noisePosition)
    {
        Vector3 direction = noisePosition - listenerPosition;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
            return 1f;

        return Physics.Raycast(listenerPosition, direction / distance, distance, _occlusionMask, QueryTriggerInteraction.Ignore)
            ? _occlusionSoundIntensityMultiplier
            : 1f;
    }
}
