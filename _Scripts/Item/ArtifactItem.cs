using UnityEngine;

public class ArtifactItem : Item
{
    private static readonly int ARTIFACT_SELECTED = Animator.StringToHash("ArtifactSelected");

    public override int AnimatorTriggerHash => ARTIFACT_SELECTED;
}
