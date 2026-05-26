using UnityEngine;

[CreateAssetMenu(menuName = "AI/Seek Config", fileName = "Seek Config", order = 6)]
public class SeekConfigSO : ScriptableObject
{
    public float sensorRadius = 10f;
    public float lookAroundDelay = 1f;
    public float moveDelay = 1f;
    public int seekCost = 1;
    public int lookAroundCost = 1;
}
