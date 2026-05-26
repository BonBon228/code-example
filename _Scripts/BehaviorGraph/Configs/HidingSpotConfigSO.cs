using UnityEngine;

[CreateAssetMenu(menuName = "AI/Hiding Spot Config", fileName = "Hiding Spot Config")]
public class HidingSpotConfigSO : ScriptableObject
{
    public LayerMask checkableLayerMask;
    public LayerMask hidingSpotTriggerLayerMask;
    public float sensorRadius = 10f;
}
