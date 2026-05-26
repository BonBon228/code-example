using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Netcode.Components;

//DoorOpenBehavior could be refactored to ObjectOpenBehavior to be more generic and reusable for other objects. Or instead of using IAIOpenable, we could specify it to be DoorOpening
[RequireComponent(typeof(NavMeshAgent))]
public class DoorOpenBehavior : NetworkBehaviour
{
    [SerializeField] private LayerMask _doorLayerMask;
    [SerializeField] private float _raycastHeightOffset = 1f;
    [SerializeField] private int _kicksRequired = 1;
    [SerializeField] private SoundData _shoutSoundData;
    [SerializeField] private SoundData _kickDoorSoundData;
    [SerializeField] private SoundData _kickDoorOpenSoundData;
    private static readonly int DOOR_KICK_OPEN = Animator.StringToHash("DoorKickOpen");
    private DoorOpening _doorOpening;
    private int _currentKickCount = 0;

    private NavMeshAgent Agent => GetComponent<NavMeshAgent>();
    private NetworkAnimator NetworkAnimator => GetComponent<NetworkAnimator>();
    public bool IsDoorOpenTriggered { get; set; }
    public bool IsDoorKickCompleted { get; private set; }

    private void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
            enabled = true;
    }

    public void OpenDoor(DoorOpening doorOpening)
    {
        _doorOpening = doorOpening;
        _doorOpening.BeingOpened = true;
        IsDoorKickCompleted = false;

        SoundManager.Instance.CreateSound()
            .WithSoundData(_shoutSoundData)
            .WithPosition(transform.position)
            .Play();
        
        NetworkAnimator.SetTrigger(DOOR_KICK_OPEN);
        IsDoorOpenTriggered = true;
    }

    //animation event
    public void BeginDoorKicking(int _)
    {
        if (_doorOpening == null)
            return;

        SoundManager.Instance.CreateSound()
            .WithSoundData(_kickDoorSoundData)
            .WithPosition(transform.position)
            .Play();

        _currentKickCount++;
        if (_currentKickCount < _kicksRequired)
            return;
        
        SoundManager.Instance.CreateSound()
            .WithSoundData(_kickDoorOpenSoundData)
            .WithPosition(transform.position)
            .Play();
        
        if (_doorOpening != null && _doorOpening.TryGetComponent<IKnockable>(out var knockable))
            knockable.KnockOut(transform.position);
        else
            _doorOpening.OpenServerRpc(Agent.transform.position, withSound: false);
        
        _doorOpening.BeingOpened = false;
        IsDoorKickCompleted = true;
        _currentKickCount = 0;
        _doorOpening = null;
    }
}
