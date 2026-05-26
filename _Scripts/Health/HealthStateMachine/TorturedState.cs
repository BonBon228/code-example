using System.Collections;
using UnityEngine;

public class TorturedState : IState
{
    private Health _health;
    private ITorture _torture;
    private Timer _timer;
    private CharacterVolumeEffects _characterVolumeEffects;
    private UIPanelsDisplayer _panelsDisplayer;
    private float _remainingTimeToSave;
    private SoundEmitter _torturedSoundEmitter;

    public TorturedState(Health health)
    {
        _health = health;
        _characterVolumeEffects = _health.Player.CharacterVolumeEffects;
        _panelsDisplayer = _health.Player.PanelsDisplayer;
    }

    public void Enter()
    {
        _timer = new Timer(_health, TimerMode.Countdown);
        _timer.OnUpdated += OnTimerUpdated;
        _timer.Set(_health.TimeToSave);
        _timer.StartCounting();

        _torturedSoundEmitter = SoundManager.Instance.CreateSound()
            .WithSoundData(_health.playerSounds.torturedSoundData)
            .WithPosition(_health.transform.position)
            .PlayAndReturn();
        
        _torturedSoundEmitter.PlayWithFadeIn(15f);

        _characterVolumeEffects.EyeBlinkVolumeEffect.OpenEyes();
        _characterVolumeEffects.FearTentaclesVolumeEffect.Activate();
        
        _panelsDisplayer.GetPanel<TorturePanel>().Initialize(_timer);
        _panelsDisplayer.ShowPanel<TorturePanel>();

        OccupyTorture();
        _health.CharacterAnimator.ChangeIKWeightToHead();
        LimitCameraRotation();
    }

    public void Execute()
    {
        if(_torture.IsSavedFromTorture.Value && _remainingTimeToSave > 0)
        {
            _health.IsAwaitingRestoreHealth = true;
            _health.HealthStateMachine.TransitionTo(_health.HealthStateMachine.AliveState);
            _health.Player.HandInventory.Enable();
            _health.Player.CharacterCamera.SetFollowTransform(_health.Player.Character.CameraFollowPoint);
            _health.RestoreMaxHealthServerRpc();
        }

        if(_remainingTimeToSave <= 0)
            _health.HealthStateMachine.TransitionTo(_health.HealthStateMachine.DeadState);
    }

    public void Exit()
    {
        _characterVolumeEffects.FearTentaclesVolumeEffect.Deactivate();
        _panelsDisplayer.HidePanel<TorturePanel>();

        _timer.StopCounting();
        _timer.OnUpdated -= OnTimerUpdated;
        _timer = null;

        _torture.LiberateServerRpc();
        _torture = null;

        _torturedSoundEmitter.Stop();
    }

    private void OccupyTorture()
    {
        _torture = _health.Player.TortureSelector.Select();
        if(_torture != null)
        {
            _torture.OccupyServerRpc(_health.NetworkObject);

            _health.Player.CharacterCamera.SetFollowTransform(_torture.CameraPlacement);
            Vector3 eulerAngles = _torture.PlayerPlacementRotation.eulerAngles;
            _health.Player.CharacterCamera.SetRotation(eulerAngles.y, eulerAngles.x);
        }
    }

    private void LimitCameraRotation()
    {
        _health.Player.LockCameraRotation = false;
        _health.Player.LimitCameraRotation = true;
    }

    private void OnTimerUpdated(float value)
    {
        _remainingTimeToSave = value;
    }
}
