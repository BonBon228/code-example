using System.Collections.Generic;
using UnityEngine;

public class ItemsCollection
{
    private List<IInventoryItem> _items = new List<IInventoryItem>();

    public IReadOnlyList<IInventoryItem> Items => _items;

    public void AddItem(IInventoryItem item)
    {
        _items.Add(item);
    }

    public void RemoveItem(IInventoryItem item)
    {
        _items.Remove(item);
    }
}
