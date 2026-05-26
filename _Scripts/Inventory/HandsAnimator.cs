using UnityEngine;
using System.Collections;

public class HandsAnimator : MonoBehaviour
{
    [SerializeField] private HandIdleAnimation _handIdleAnimation;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _handTopTransform;
    [SerializeField] private Transform _handBottomTransform;
    
    private HandTweenMover _rightHandMover;
    private Coroutine _rightHandAnimationCoroutine;
    private Inventory _handInventory;
    private int _latestTriggerHash;

    private Animator RightHandAnimator => _rightHand.TryGetComponent(out Animator animator) ? animator : null;

    public void Initialize(Inventory handInventory)
    {
        _handInventory = handInventory;
        _rightHandMover = new HandTweenMover(_rightHand);
        
        _handInventory.OnItemSelected += OnItemSelected;
        _handInventory.OnItemDeselected += OnItemDeselected;
        _handInventory.OnItemCollected += OnItemCollected;
        _handInventory.OnItemDropped += OnItemDropped;
    }

    public void Deinitialize()
    {
        if (_handInventory == null)
            return;

        _handInventory.OnItemSelected -= OnItemSelected;
        _handInventory.OnItemDeselected -= OnItemDeselected;
        _handInventory.OnItemCollected -= OnItemCollected;
        _handInventory.OnItemDropped -= OnItemDropped;
    }

    private void OnItemSelected(IInventoryItem item)
    {
        PlayAppearAnimation(item);
    }

    private void OnItemDeselected()
    {
        HideHandImmediate();
    }

    private void OnItemCollected(IInventoryItem item)
    {
        if (_handInventory.ActiveSlot?.Item == item)
            PlayAppearAnimation(item);
    }

    private void OnItemDropped()
    {
        HideHandImmediate();
    }

    private void PlayAppearAnimation(IInventoryItem item)
    {
        StopCurrentAnimation();
        _rightHandAnimationCoroutine = StartCoroutine(AnimateHandAppear(item));
    }

    private void StopCurrentAnimation()
    {
        if (_rightHandAnimationCoroutine != null)
        {
            StopCoroutine(_rightHandAnimationCoroutine);
            _rightHandAnimationCoroutine = null;
        }

        if(_handIdleAnimation != null)
            _handIdleAnimation.StopIdleAnimation();
    }

    private void HideHandImmediate()
    {
        StopCurrentAnimation();
        _rightHand.gameObject.SetActive(false);
    }

    private IEnumerator AnimateHandAppear(IInventoryItem item)
    {
        Animator handAnimator = RightHandAnimator;

        if (handAnimator == null)
            yield break;

        _rightHand.transform.localPosition = _handBottomTransform.localPosition;
        _rightHand.gameObject.SetActive(true);

        if (handAnimator.HasParameter(_latestTriggerHash))
            handAnimator.ResetTrigger(_latestTriggerHash);

        int triggerHash = item.AnimatorTriggerHash;
        _latestTriggerHash = triggerHash;

        if (handAnimator.HasParameter(triggerHash))
            handAnimator.SetTrigger(triggerHash);

        _rightHandMover.MoveHandLocally(_handTopTransform.localPosition, 0.35f);
        yield return new WaitUntil(() => !_rightHandMover.IsMoving());
        _handIdleAnimation.StartIdleAnimation();
        
        _rightHandAnimationCoroutine = null;
    }
}
