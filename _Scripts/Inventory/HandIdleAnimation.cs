using UnityEngine;
using DG.Tweening;

public class HandIdleAnimation : MonoBehaviour
{
    [Header("Breathing Animation")]
    [SerializeField] private float breathingAmplitude = 0.1f;
    [SerializeField] private float breathingDuration = 3f;
    
    [Header("Swaying Animation")]
    [SerializeField] private float swayAmplitude = 0.05f;
    [SerializeField] private float swayDuration = 4f;
    
    [Header("Rotation")]
    [SerializeField] private float rotationAmplitude = 1f;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Tween breathingTween;
    private Tween swayTween;
    private Tween rotationTween;

    private void OnDisable()
    {
        breathingTween?.Kill();
        swayTween?.Kill();
        rotationTween?.Kill();
    }

    public void StartIdleAnimation()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        StopIdleAnimation();
        
        breathingTween = transform.DOLocalMoveY(startPosition.y + breathingAmplitude, breathingDuration / 2)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);

        swayTween = transform.DOLocalMoveX(startPosition.x + swayAmplitude, swayDuration / 2)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        rotationTween = transform.DOLocalRotate(new Vector3(
            startRotation.eulerAngles.x + rotationAmplitude,
            startRotation.eulerAngles.y + rotationAmplitude,
            startRotation.eulerAngles.z), swayDuration / 2)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopIdleAnimation()
    {
        breathingTween?.Kill();
        swayTween?.Kill();
        rotationTween?.Kill();
    }
}
