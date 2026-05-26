using Unity.Netcode;
using UnityEngine;

public class FlashlightItem : Item
{
    [SerializeField] private Light _flashlightLight;
    private static readonly int FLASHLIGHT_SELECTED = Animator.StringToHash("FlashlightSelected");

    public override int AnimatorTriggerHash => FLASHLIGHT_SELECTED;

    protected override void OnCollected()
    {
        base.OnCollected();
        _flashlightLight.enabled = false;
    }
}
