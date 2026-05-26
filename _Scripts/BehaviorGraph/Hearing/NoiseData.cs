using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class NoiseData
{
    [SerializeField] private NoiseType _type;
    [SerializeField, Min(0f)] private float _radius = 8f;
    [SerializeField, Min(0f)] private float _soundIntensity = 1f;

    public NoiseType Type => _type;
    public float Radius => _radius;
    public float SoundIntensity => _soundIntensity;

    public NoiseData(NoiseType type, float radius, float soundIntensity)
    {
        _type = type;
        _radius = Mathf.Max(0f, radius);
        _soundIntensity = Mathf.Max(0f, soundIntensity);
    }
}
