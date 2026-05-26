using System;
using KinematicCharacterController.Examples;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    [SerializeField] private PlayerDamageSoundsConfig _playerSoundsConfig;
    [SerializeField] private DissolveConfig _playerDissolveConfig;
    [SerializeField] private Player _player;
    [SerializeField] private CharacterAnimator _characterAnimator;
    [SerializeField] private MeshDissolver _meshDissolver;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _timeToSave = 120f;
    [SerializeField] private NetworkVariable<float> _currentHealthNetwork = new NetworkVariable<float>();
    private NetworkVariable<bool> _isAliveNetwork = new NetworkVariable<bool>(true, writePerm: NetworkVariableWritePermission.Owner);
    
    private HealthStateMachine _healthStateMachine;
    public bool IsAwaitingRestoreHealth { get; set; } = false;

    public HealthStateMachine HealthStateMachine => _healthStateMachine;
    public DissolveConfig PlayerDissolveConfig => _playerDissolveConfig;
    public float TimeToSave => _timeToSave;
    public PlayerDamageSoundsConfig playerSounds => _playerSoundsConfig;
    public float CurrentHealthNetwork => _currentHealthNetwork.Value;
    public Player Player => _player;
    public CharacterAnimator CharacterAnimator => _characterAnimator;
    public bool IsAliveNetwork => _isAliveNetwork.Value;
    public bool IsAliveLocal => _healthStateMachine.CurrentState is AliveState;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            _currentHealthNetwork.Value = _maxHealth;

        if (IsOwner)
        {
            _healthStateMachine = new HealthStateMachine(this);
            _healthStateMachine.Initialize(_healthStateMachine.AliveState);
            _healthStateMachine.OnStateChanged += OnStateChanged;
        }

        _currentHealthNetwork.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            _healthStateMachine.OnStateChanged -= OnStateChanged;

        _currentHealthNetwork.OnValueChanged -= OnHealthChanged;
    }

    private void OnStateChanged(IState state)
    {
        _isAliveNetwork.Value = state is AliveState;
    }

    private void OnHealthChanged(float previousHealth, float newHealth)
    {
        if (newHealth == _maxHealth)
            IsAwaitingRestoreHealth = false;

        if (IsOwner)
        {
            if (newHealth < previousHealth)
            {
                _player.CharacterVolumeEffects.BloodDistortionVolumeEffect
                    .OnDamageTaken(newHealth, _maxHealth);
            }
            else if (newHealth > previousHealth)
            {
                _player.CharacterVolumeEffects.BloodDistortionVolumeEffect
                    .OnHealthRestored(newHealth, _maxHealth);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        _healthStateMachine.Execute();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(
        float damage,
        ShakeData cameraShakeData = default,
        DamageHitType hitType = DamageHitType.None,
        DamageHurtType hurtType = DamageHurtType.Hurt)
    {
        if (damage < 0)
        {
            Debug.LogError("Damage cannot be negative");
            return;
        }

        Debug.Log($"{gameObject.name} {OwnerClientId} took {damage} damage");

        _currentHealthNetwork.Value -= damage;
        _currentHealthNetwork.Value = Mathf.Clamp(_currentHealthNetwork.Value, 0, _maxHealth);
        
        _characterAnimator.PlayHitAnimation();
        
        if (hitType != DamageHitType.None || hurtType != DamageHurtType.None)
        {
            PlayHitSoundClientRpc(hitType, hurtType);
        }

        if (!cameraShakeData.Equals(default(ShakeData)))
        {
            TriggerCameraShakeClientRpc(cameraShakeData, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            });
        }
    }

    [ClientRpc]
    private void TriggerCameraShakeClientRpc(ShakeData cameraShakeData, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner && _player.CharacterCamera != null)
        {
            _player.CharacterCamera.TriggerCameraShake(cameraShakeData);
        }
    }

    public void PlayDeathSound()
    {
        SoundManager.Instance.CreateSound()
        .WithSoundData(_playerSoundsConfig.deathSoundData)
        .WithPosition(transform.position)
        .Play();
    }

    [ClientRpc]
    public void PlayHitSoundClientRpc(DamageHitType hitType, DamageHurtType hurtType)
    {
        if (hitType != DamageHitType.None)
        {
            SoundData hitSound = _playerSoundsConfig.GetHitSound(hitType);
            
            if (hitSound != null)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(hitSound)
                    .WithPosition(transform.position)
                    .Play();
            }
        }
        
        if (hurtType != DamageHurtType.None)
        {
            SoundData screamSound = _playerSoundsConfig.GetHurtSound(hurtType);
            
            if (screamSound != null)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(screamSound)
                    .WithPosition(transform.position)
                    .Play();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayDeadSoundServerRpc()
    {
        PlayDeadSoundClientRpc();
    }

    [ClientRpc]
    private void PlayDeadSoundClientRpc()
    {
        if (IsOwner)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_playerSoundsConfig.personalDeadSoundData)
                .WithPosition(transform.position)
                .Play();
        }
        else
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_playerSoundsConfig.deadSoundData)
                .WithPosition(transform.position)
                .Play();
        }
    }

    [ServerRpc]
    public void RestoreMaxHealthServerRpc()
    {
        _currentHealthNetwork.Value = _maxHealth;
    }

    public void StartDissolves()
    {
        _meshDissolver.DissolveLocally();
        _meshDissolver.DissolveServerRpc();
    }

    public void StartAppears()
    {
        _meshDissolver.AppearLocally();
        _meshDissolver.AppearServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnableCapsuleForAllServerRpc()
    {
        RequestEnableCapsuleForAllClientsClientRpc();
    }

    [ClientRpc]
    private void RequestEnableCapsuleForAllClientsClientRpc()
    {
        if (Player.Character.Motor.Capsule != null)
        {
            Player.Character.Motor.Capsule.enabled = true;
        }
    }

    [ServerRpc]
    public void DisableCapsuleForAllServerRpc()
    {
        RequestDisableCapsuleForAllClientsClientRpc();
    }

    [ClientRpc]
    private void RequestDisableCapsuleForAllClientsClientRpc()
    {
        if (Player.Character.Motor.Capsule != null)
        {
            Player.Character.Motor.Capsule.enabled = false;
        }
    }
}
