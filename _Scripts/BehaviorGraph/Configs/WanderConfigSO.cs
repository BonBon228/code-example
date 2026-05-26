using UnityEngine;

[CreateAssetMenu(menuName = "AI/Wander Config", fileName = "Wander Config", order = 1)]
public class WanderConfigSO : ScriptableObject
{
    public float wanderSensorRadius = 5f;
    public float wanderDelay = 1f;
    public float wanderDistanceThreshold = 1f;
    public int wanderCost = 1;
}
