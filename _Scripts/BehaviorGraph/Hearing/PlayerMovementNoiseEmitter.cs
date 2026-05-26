using KinematicCharacterController.Examples;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovementNoiseEmitter : NetworkBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private NoiseData _walkFootstepNoiseData = new(NoiseType.Footstep, 8f, 1f);
    [SerializeField] private NoiseData _sprintFootstepNoiseData = new(NoiseType.Footstep, 8f, 1.5f);
    [SerializeField] private NoiseData _crouchFootstepNoiseData = new(NoiseType.Footstep, 8f, 0.45f);
    [SerializeField] private NoiseData _proneFootstepNoiseData = new(NoiseType.Footstep, 8f, 0.2f);
    [SerializeField] private NoiseData _jumpNoiseData = new(NoiseType.Jump, 10f, 0.8f);
    [SerializeField] private NoiseData _landingNoiseData = new(NoiseType.Landing, 12f, 1.25f);

    private NoiseEmitter _noiseEmitter;
    private bool _wasGrounded;

    private bool IsGrounded => _playerController != null
        && _playerController.Motor != null
        && _playerController.Motor.GroundingStatus.IsStableOnGround;

    private void OnEnable()
    {
        _wasGrounded = IsGrounded;
    }

    private void Awake()
    {
        if (_playerController == null)
            _playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (!IsServer || !EnsureNoiseEmitter() || _playerController == null || _playerController.Motor == null)
            return;

        bool isGrounded = IsGrounded;

        if (_wasGrounded && !isGrounded)
        {
            _noiseEmitter.Emit(gameObject, _jumpNoiseData);
        }
        else if (!_wasGrounded && isGrounded)
        {
            _noiseEmitter.Emit(gameObject, _landingNoiseData);
        }

        _wasGrounded = isGrounded;
    }

    public void EmitFootstep()
    {
        if (!IsServer || !EnsureNoiseEmitter())
            return;

        _noiseEmitter.Emit(gameObject, GetFootstepNoiseData());
    }

    private NoiseData GetFootstepNoiseData()
    {
        if (_playerController == null)
            return _walkFootstepNoiseData;

        if (_playerController.IsProne)
            return _proneFootstepNoiseData;

        if (_playerController.IsCrouching)
            return _crouchFootstepNoiseData;

        return _playerController.IsSprinting ? _sprintFootstepNoiseData : _walkFootstepNoiseData;
    }

    private bool EnsureNoiseEmitter()
    {
        if (_noiseEmitter != null)
            return true;

        if (NoiseSystem.Instance == null)
            return false;

        _noiseEmitter = new NoiseEmitter(NoiseSystem.Instance);
        return true;
    }
}
