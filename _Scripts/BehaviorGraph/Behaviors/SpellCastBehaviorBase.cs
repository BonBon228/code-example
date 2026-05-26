using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkAnimator))]
public abstract class SpellCastBehaviorBase : NetworkBehaviour
{
    [SerializeField] protected SpellConfigSO _spellConfig;
    [SerializeField] protected Transform _spawnPoint;

    private static readonly int TARGET_SPELL_ATTACK = Animator.StringToHash("TargetSpellAttack");

    private float _spellTimer;
    
    public float Cooldown { get; set; }
    public bool IsCastInProgress { get; private set; }
    public bool IsCastCompleted { get; private set; }
    public float SpellAttackRadius => _spellConfig != null ? _spellConfig.spellAttackRadius : 0f;

    public event Action<Vector3, Vector3> OnCastBegan;

    private NetworkAnimator NetworkAnimator => GetComponent<NetworkAnimator>();

    private void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
            enabled = true;
    }

    private void Update()
    {
        Cooldown = Mathf.Clamp(Cooldown - Time.deltaTime, 0, _spellConfig.spellCooldown);

        if (!IsCastInProgress)
            return;

        _spellTimer -= Time.deltaTime;
        if (_spellTimer <= 0f)
            CompleteCast();
    }

    public bool TryStartCast(GameObject target)
    {
        if (!IsServer || _spellConfig == null || _spawnPoint == null || target == null || IsCastInProgress || Cooldown > 0f)
            return false;

        if (Vector3.Distance(transform.position, target.transform.position) > _spellConfig.spellAttackRadius)
            return false;

        Cooldown = _spellConfig.spellCooldown;
        _spellTimer = _spellConfig.spellDelay;
        IsCastCompleted = false;
        IsCastInProgress = true;

        NetworkAnimator.SetTrigger(TARGET_SPELL_ATTACK);

        return true;
    }

    public void CancelCast()
    {
        _spellTimer = 0f;
        IsCastInProgress = false;
        IsCastCompleted = false;
    }

    private void CompleteCast()
    {
        IsCastInProgress = false;
        IsCastCompleted = true;
    }

    //animation event
    public void BeginCast(int _)
    {
        if (!IsCastInProgress)
            return;

        if (OnCastBegan != null)
            OnCastBegan.Invoke(_spawnPoint.position, transform.forward);
        else
            CastSpell(_spawnPoint.position, transform.forward);
    }

    protected abstract void CastSpell(Vector3 spawnPoint, Vector3 forward);
}
