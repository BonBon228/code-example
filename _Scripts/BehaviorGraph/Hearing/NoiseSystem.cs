using System;
using UnityEngine;

public class NoiseSystem : MonoBehaviour, INoiseEventSink
{
    public static NoiseSystem Instance { get; private set; }

    public event Action<NoiseEvent> NoiseEmitted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Emit(NoiseEvent noiseEvent)
    {
        if (noiseEvent.Radius <= 0f || noiseEvent.SoundIntensity <= 0f)
            return;

        NoiseEmitted?.Invoke(noiseEvent);
    }
}
