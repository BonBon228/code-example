using UnityEngine;

public class BookItem : Item
{
    private static readonly int BOOK_SELECTED = Animator.StringToHash("BookSelected");

    public override int AnimatorTriggerHash => BOOK_SELECTED;
}
