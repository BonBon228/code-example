using Unity.Netcode;
using UnityEngine;

public class WaveSpellCastBehavior : SpellCastBehaviorBase
{
    protected override void CastSpell(Vector3 spawnPoint, Vector3 forward)
    {
        GameObject spell = Instantiate(_spellConfig.spellPrefab);
        spell.transform.position = spawnPoint;
        spell.transform.forward = forward;
        spell.GetComponent<NetworkObject>().Spawn(true);

        if (spell.TryGetComponent(out HandWave handWave))
            handWave.Cast();
    }
}
