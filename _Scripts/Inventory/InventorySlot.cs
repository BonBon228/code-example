public class InventorySlot
{
    private IInventoryItem _item;
    private int _index;

    public IInventoryItem Item => _item;
    public int Index => _index;
    public bool IsEmpty => _item == null;

    public InventorySlot(int index)
    {
        _index = index;
    }

    public void SetItem(IInventoryItem item)
    {
        _item = item;
    }

    public void RemoveItem()
    {
        _item = null;
    }
}
