using Unity.Netcode;

public interface IInventoryItem
{
    ItemData Data { get; }
    NetworkVariable<bool> CanBeDropped { get; set; }
    int AnimatorTriggerHash { get; }
    bool IsInInventory { get; }
    bool IsTwoHanded { get; }

    bool TryCollect(NetworkObjectReference player);
    bool TryDrop();

    void Select();
    void Deselect();
}
