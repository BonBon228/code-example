using UnityEngine;

public class SoulStoneItem : Item
{
    private static readonly int SOULSTONE_SELECTED = Animator.StringToHash("SoulStoneSelected");

    public override int AnimatorTriggerHash => SOULSTONE_SELECTED;
    public override bool IsTwoHanded => true;
}
