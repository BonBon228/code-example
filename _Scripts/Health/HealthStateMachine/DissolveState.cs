using UnityEngine;

public class DissolveState : IState
{
    private DissolveConfig _dissolveConfig;
    private Health _health;
    private UIPanelsDisplayer _panelsDisplayer;
    private float _timer;
    private float _delayToDissolve;
    private float _dissolveDuration;
    private bool _dissolveStarted = false;

    public DissolveState(Health health)
    {
        _health = health;
        _dissolveConfig = _health.PlayerDissolveConfig;
        _delayToDissolve = _dissolveConfig.delayToDissolve;
        _dissolveDuration = _dissolveConfig.dissolveDuration;
        _panelsDisplayer = _health.Player.PanelsDisplayer;
    }

    public void Enter()
    {
        _dissolveStarted = false;
        _timer = 0f;
        DisableControls();
        EnableRagdoll();
        _health.Player.HandInventory.Disable();
        _health.Player.CharacterCamera.SetFollowTransform(_health.Player.Character.CameraHeadFollowPoint);
        _health.PlayDeadSoundServerRpc();
        _health.Player.CharacterVolumeEffects.EyeBlinkVolumeEffect.CloseEyes();

        _panelsDisplayer.HidePanel<HUDPanel>();
    }

    public void Execute()
    {
        _timer += Time.deltaTime;

        if(_timer >= _delayToDissolve && !_dissolveStarted)
        {
            _health.StartDissolves();
            _dissolveStarted = true;
            _timer = 0f;
        }

        if(_timer >= _dissolveDuration && _dissolveStarted)
            _health.HealthStateMachine.TransitionTo(_health.HealthStateMachine.TorturedState);
    }

    public void Exit()
    {
        DisableRagdoll();
        _health.StartAppears();
    }

    private void DisableControls()
    {
        _health.Player.DisableInput = true;
        _health.Player.LockCameraRotation = true;
        _health.DisableCapsuleForAllServerRpc();
        _health.Player.Character.Motor.enabled = false;
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
}
