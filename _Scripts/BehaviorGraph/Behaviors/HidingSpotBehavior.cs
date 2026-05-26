using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

public class HidingSpotBehavior : NetworkBehaviour
{
    private static readonly int CHECK_SPOT_TRIGGER = Animator.StringToHash("CheckSpot");
    private static readonly int CHECK_SPOT_STATE = Animator.StringToHash("Check Spot");
    private HidingSpot _currentHidingSpot;
    private Coroutine _hidingSpotCheckCoroutine;

    public bool IsCheckAnimationEnded { get; set; } = false;

    private NavMeshAgent Agent => GetComponent<NavMeshAgent>();
    private Animator Animator => GetComponent<Animator>();
    private NetworkAnimator NetworkAnimator => GetComponent<NetworkAnimator>();
    private bool IsCheckAnimationPlaying => Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == CHECK_SPOT_STATE;

    private void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            enabled = true;
    }

    public bool StartHidingSpotCheck(GameObject hidingSpotTarget)
    {
        if (!TryGetHidingSpot(hidingSpotTarget, out HidingSpot hidingSpot))
            return false;

        _currentHidingSpot = hidingSpot;
        IsCheckAnimationEnded = false;
        _hidingSpotCheckCoroutine ??= StartCoroutine(StartHidingSpotCheckCoroutine());
        return true;
    }

    public void ResetHidingSpotCheck()
    {
        _currentHidingSpot = null;
        IsCheckAnimationEnded = false;
    }

    private IEnumerator StartHidingSpotCheckCoroutine()
    {
        Agent.updateRotation = false;
        Agent.isStopped = true;

        NetworkAnimator.SetTrigger(CHECK_SPOT_TRIGGER);

        while (Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != CHECK_SPOT_STATE)
            yield return null;

        while (IsCheckAnimationPlaying)
            yield return null;
        
        IsCheckAnimationEnded = true;
        Agent.updateRotation = true;
        Agent.isStopped = false;
        _hidingSpotCheckCoroutine = null;
    }

    private bool TryGetHidingSpot(GameObject hidingSpotTarget, out HidingSpot hidingSpot)
    {
        hidingSpot = null;
        return hidingSpotTarget != null && hidingSpotTarget.TryGetComponent(out hidingSpot);
    }

    public void CheckSpot(int _)
    {
        _currentHidingSpot?.KickHidingSpotServerRpc(Agent.transform.position);
    }

    // Animation event in the existing clip still calls LookSpot.
    public void LookSpot(int animationEventParameter)
    {
        CheckSpot(animationEventParameter);
    }
}
