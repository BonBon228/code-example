using UnityEngine;

public class MedkitItem : Item
{
    private static readonly int MEDKIT_SELECTED = Animator.StringToHash("MedkitSelected");

    public override int AnimatorTriggerHash => MEDKIT_SELECTED;
}
