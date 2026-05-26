public class AliveState : IState
{
    private Health _health;
    private UIPanelsDisplayer _panelsDisplayer;

    public AliveState(Health health)
    {
        _health = health;
        _panelsDisplayer = _health.Player.PanelsDisplayer;
    }

    public void Enter()
    {
        _health.Player.ResetControlsLimitation();
        _health.EnableCapsuleForAllServerRpc();
        _health.Player.Character.Motor.enabled = true;
        _health.CharacterAnimator.ChangeIKWeightToDefault();
        _health.Player.CharacterCamera.SetHeadBobEnabled(true);

        if (!_panelsDisplayer.GetPanel<HUDPanel>().IsActive)
            _panelsDisplayer.ShowPanel<HUDPanel>();
    }

    public void Execute()
    {
        if (_health.CurrentHealthNetwork <= 0 && !_health.IsAwaitingRestoreHealth)
        {
            _health.Player.CharacterCamera.SetHeadBobEnabled(false);
            _health.HealthStateMachine.TransitionTo(_health.HealthStateMachine.DissolveState);
        }
    }

    public void Exit()
    {
        
    }
}
