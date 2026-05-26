using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Config", fileName = "Attack Config", order = 2)]
public class AttackConfigSO : ScriptableObject
{
    public int damage = 25;
    public float sensorRadius = 10f;
    public float meleeAttackRadius = 1f;
    public float attackPointRadius = 0.5f;
    public float meleeAttackDelay = 1f;
    public LayerMask attackableLayerMask;
    public SoundData attackSoundData;
    public SoundData yellSoundData;
    public ShakeData cameraShakeData;
}
