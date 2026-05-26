using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkAnimator))]
public class MeleeAttackBehavior : NetworkBehaviour
{
    [SerializeField] private AttackConfigSO _attackConfig;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private int _maxOverlapColliders = 4;

    private static readonly int ATTACK01 = Animator.StringToHash("Attack01");
    private static readonly int ATTACK02 = Animator.StringToHash("Attack02");

    private readonly HashSet<Collider> _damagedPlayers = new HashSet<Collider>();
    private Collider[] _colliders;
    private float _attackTimer;

    private NetworkAnimator NetworkAnimator => GetComponent<NetworkAnimator>();

    public Transform AttackPoint => _attackPoint;
    public bool IsAttacking { get; private set; } = false;
    public bool IsAttackInProgress { get; private set; }
    public bool IsAttackCompleted { get; private set; }
    public float MeleeAttackRadius => _attackConfig != null ? _attackConfig.meleeAttackRadius : 0f;

    private void Awake()
    {
        _colliders = new Collider[Mathf.Max(1, _maxOverlapColliders)];
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
            enabled = true;
    }

    private void Update()
    {
        if (!IsAttackInProgress)
            return;

        if (IsAttacking)
            DamagePlayersInAttackPoint();

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
            CompleteAttack();
    }

    public bool TryStartAttack(GameObject target)
    {
        if (!IsServer || _attackConfig == null || _attackPoint == null || target == null || IsAttackInProgress)
            return false;

        if (Vector3.Distance(transform.position, target.transform.position) > _attackConfig.meleeAttackRadius)
            return false;

        _attackTimer = _attackConfig.meleeAttackDelay;
        IsAttackCompleted = false;
        IsAttackInProgress = true;
        IsAttacking = false;
        _damagedPlayers.Clear();

        PlaySound(_attackConfig.yellSoundData);

        int randomValue = Random.Range(0, 2);
        NetworkAnimator.SetTrigger(randomValue == 0 ? ATTACK01 : ATTACK02);

        return true;
    }

    public void CancelAttack()
    {
        _attackTimer = 0f;
        IsAttacking = false;
        IsAttackInProgress = false;
        IsAttackCompleted = false;
        _damagedPlayers.Clear();
    }

    private void OnDrawGizmos()
    {
        if(_attackPoint == null || !IsAttacking)
            return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackPoint.position, _attackConfig.attackPointRadius);
    }

    //animation event
    public void BeginMeleeAttack(int _)
    {
        PlaySound(_attackConfig.attackSoundData);
        IsAttacking = true;
    }

    //animation event
    public void EndMeleeAttack(int _)
    {
        IsAttacking = false;
    }

    private void DamagePlayersInAttackPoint()
    {
        int collidersCount = Physics.OverlapSphereNonAlloc(
            _attackPoint.position,
            _attackConfig.attackPointRadius,
            _colliders,
            _attackConfig.attackableLayerMask);

        for (int i = 0; i < collidersCount; i++)
        {
            Collider collider = _colliders[i];
            if (collider == null || _damagedPlayers.Contains(collider))
                continue;

            if (collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamageServerRpc(
                    _attackConfig.damage,
                    _attackConfig.cameraShakeData,
                    DamageHitType.MeleeHit);
            }

            _damagedPlayers.Add(collider);
        }
    }

    private void CompleteAttack()
    {
        IsAttacking = false;
        IsAttackInProgress = false;
        IsAttackCompleted = true;
    }

    private void PlaySound(SoundData soundData)
    {
        SoundManager soundManager = SoundManager.TryGetInstance();
        if (soundData == null || soundManager == null)
            return;

        soundManager.CreateSound()
            .WithSoundData(soundData)
            .WithPosition(transform.position)
            .Play();
    }
}
