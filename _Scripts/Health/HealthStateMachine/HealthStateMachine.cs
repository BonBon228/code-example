using System;

public class HealthStateMachine
{
    public IState CurrentState { get; private set; }

    public AliveState AliveState { get; private set; }
    public DissolveState DissolveState { get; private set; }
    public TorturedState TorturedState { get; private set; }
    public DeadState DeadState { get; private set; }

    public event Action<IState> OnStateChanged;

    public HealthStateMachine(Health health)
    {
        AliveState = new AliveState(health);
        DissolveState = new DissolveState(health);
        TorturedState = new TorturedState(health);
        DeadState = new DeadState(health);
    }

    public void Initialize(IState state)
    {
        CurrentState = state;
        state.Enter();

        OnStateChanged?.Invoke(state);
    }

    public void TransitionTo(IState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        nextState.Enter();

        OnStateChanged?.Invoke(nextState);
    }

    public void Execute()
    {
        CurrentState?.Execute();
    }
}
