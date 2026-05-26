using UnityEngine;

public class DeadBodyItem : Item
{
    private static readonly int DEADBODY_SELECTED = Animator.StringToHash("DeadBodySelected");

    public override int AnimatorTriggerHash => DEADBODY_SELECTED;
    public override bool IsTwoHanded => true;
}
