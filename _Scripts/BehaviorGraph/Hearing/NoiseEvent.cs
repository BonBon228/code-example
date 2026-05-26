using UnityEngine;

public readonly struct NoiseEvent
{
    public GameObject Source { get; }
    public Vector3 Position { get; }
    public float Radius { get; }
    public float SoundIntensity { get; }
    public NoiseType Type { get; }
    public float Time { get; }

    public NoiseEvent(GameObject source, Vector3 position, NoiseData noiseData, float time)
    {
        Source = source;
        Position = position;
        Radius = noiseData != null ? noiseData.Radius * noiseData.SoundIntensity : 0f;
        SoundIntensity = noiseData != null ? noiseData.SoundIntensity : 0f;
        Type = noiseData != null ? noiseData.Type : default;
        Time = time;
    }
}
