public class DeadState : IState
{
    private Health _health;
    private CharacterVolumeEffects _characterVolumeEffects;

    public DeadState(Health health)
    {
        _health = health;
        _characterVolumeEffects = _health.Player.CharacterVolumeEffects;
    }

    public void Enter()
    {
        _health.Player.CharacterCamera.SetFollowTransform(_health.Player.Character.CameraHeadFollowPoint);
        LockCameraRotation();
        EnableRagdoll();
        _characterVolumeEffects.EyeBlinkVolumeEffect.CloseEyes();
        _health.PlayDeathSound();
    }

    public void Execute()
    {
        if (_health.CurrentHealthNetwork > 0)
        {
            _health.Player.CharacterCamera.SetFollowTransform(_health.Player.Character.CameraFollowPoint);
            _health.Player.HandInventory.Enable();
            _characterVolumeEffects.EyeBlinkVolumeEffect.OpenEyes();
            _health.HealthStateMachine.TransitionTo(_health.HealthStateMachine.AliveState);
        }
    }

    public void Exit()
    {
        DisableRagdoll();
    }

    private void EnableRagdoll()
    {
        _health.CharacterAnimator.DisableIKWeight();
        _health.CharacterAnimator.RagdollEnabler.EnableRagdollLocally();
        _health.CharacterAnimator.RagdollEnabler.RequestRagdollEnablingServerRpc();
    }

    private void DisableRagdoll()
    {
        _health.CharacterAnimator.RagdollEnabler.EnableAnimatorLocally();
        _health.CharacterAnimator.RagdollEnabler.RequestAnimatorEnablingServerRpc();
    }

    private void LockCameraRotation()
    {
        _health.Player.LockCameraRotation = true;
        _health.Player.LimitCameraRotation = false;
    }
}
