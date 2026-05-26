using Unity.Netcode;
using UnityEngine;

public class TargetSpellCastBehavior : SpellCastBehaviorBase
{
    protected override void CastSpell(Vector3 spawnPoint, Vector3 forward)
    {
        GameObject spell = Instantiate(_spellConfig.spellPrefab, spawnPoint, Quaternion.LookRotation(forward));
        spell.GetComponent<NetworkObject>().Spawn(true);
    }
}
