using UnityEngine;

public class NoiseEmitter
{
    private readonly INoiseEventSink _noiseEventSink;

    public NoiseEmitter(INoiseEventSink noiseEventSink)
    {
        _noiseEventSink = noiseEventSink;
    }

    public void Emit(GameObject source, NoiseData noiseData)
    {
        if (source == null || noiseData == null)
            return;

        if (_noiseEventSink == null)
            return;

        _noiseEventSink.Emit(new NoiseEvent(source, source.transform.position, noiseData, Time.time));
    }
}
