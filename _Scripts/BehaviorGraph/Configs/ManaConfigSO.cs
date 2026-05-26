using UnityEngine;

[CreateAssetMenu(menuName = "AI/Mana Config", fileName = "Mana Config", order = 3)]
public class ManaConfigSO : ScriptableObject
{
    public float maxMana = 100f;
    public float manaRestorationRate = 1f;
}
