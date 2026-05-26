using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public class HandTweenMover
{
    private Transform _hand;
    private TweenerCore<Vector3, Vector3, VectorOptions> _tween;

    public HandTweenMover(Transform hand)
    {
        _hand = hand;
    }

    public void MoveHandLocally(Vector3 endLocalPosition, float duration)
    {
        _tween?.Kill();
        _tween = _hand.DOLocalMove(endLocalPosition, duration).OnComplete(() => _tween = null);
    }

    public bool IsMoving()
    {
        return _tween.IsActive();
    }
}