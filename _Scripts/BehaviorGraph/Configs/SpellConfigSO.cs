using UnityEngine;

[CreateAssetMenu(menuName = "AI/Spell Config", fileName = "Spell Config", order = 4)]
public class SpellConfigSO : ScriptableObject
{
    public GameObject spellPrefab;
    public float spellAttackRadius = 5f;
    public float spellDelay = 1;
    public float spellCooldown = 5f;
}
