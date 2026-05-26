using System;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

public class Inventory
{
    private Player _owner;
    private InventorySlot[] _handInventorySlots;
    private InventorySlot _activeSlot;
    private int _inventorySize;
    private bool _isEnabled = true;

    public event Action OnInventoryEnabled;
    public event Action OnInventoryDisabled;
    public event Action<InventorySlot> OnActiveSlotChanged;
    public event Action<IInventoryItem> OnItemSelected;
    public event Action OnItemDeselected;
    public event Action<IInventoryItem> OnItemCollected;
    public event Action OnItemDropped;

    public InventorySlot ActiveSlot => _activeSlot;
    public InventorySlot DefaultSlot => GetSlot(0);
    public bool IsEnabled => _isEnabled;

    public Inventory(int inventorySize, Player owner)
    {
        _inventorySize = inventorySize;
        _owner = owner;
    }

    public void InitializeSlots()
    {
        _handInventorySlots = new InventorySlot[_inventorySize];

        for (int i = 0; i < _handInventorySlots.Length; i++)
            _handInventorySlots[i] = new InventorySlot(i);

        _activeSlot = DefaultSlot;
    }

    private InventorySlot GetSlot(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= _handInventorySlots.Length)
            throw new ArgumentOutOfRangeException("slotNumber", "Slot number is out of range");

        return _handInventorySlots[slotNumber];
    }

    public bool TryChangeActiveSlot(int slotNumber)
    {
        if (!_isEnabled)
            return false;

        if (_activeSlot?.Item != null && _activeSlot.Item.IsTwoHanded)
            return false;

        if (slotNumber < 0 || slotNumber >= _handInventorySlots.Length)
            throw new ArgumentOutOfRangeException("slotNumber", "Slot number is out of range");

        if (_activeSlot != null && _activeSlot.Index == slotNumber)
            return false;

        DeselectCurrentItem();
        _activeSlot = _handInventorySlots[slotNumber];
        SelectCurrentItem();
        
        OnActiveSlotChanged?.Invoke(_activeSlot);
        return true;
    }

    public bool TryCollectToActiveSlot(IInventoryItem item)
    {
        return TryCollectToSlot(_activeSlot, item);
    }

    public bool TryCollectToSlot(InventorySlot slot, IInventoryItem item)
    {
        if (!_isEnabled)
            return false;

        if (slot == null || !slot.IsEmpty)
            return false;

        if (item.TryCollect(_owner.NetworkObject))
        {
            slot.SetItem(item);

            if (slot == _activeSlot)
            {
                Debug.Log("Selecting collected item in active slot");
                SelectCurrentItem();
            }

            Debug.Log("Selecting collected item in active");
            OnItemCollected?.Invoke(item);
            return true;
        }

        return false;
    }

    public bool TryDropFromActiveSlot()
    {
        if (!_isEnabled)
            return false;

        if (_activeSlot == null || _activeSlot.IsEmpty)
            return false;

        if (_activeSlot.Item.TryDrop())
        {
            _activeSlot.RemoveItem();
            OnItemDropped?.Invoke();
            return true;
        }
        
        return false;
    }

    public void Disable()
    {
        if (!_isEnabled)
            return;
        
        _isEnabled = false;
        DeselectCurrentItem();
        OnInventoryDisabled?.Invoke();
    }

    public void Enable()
    {
        if (_isEnabled)
            return;
        
        _isEnabled = true;
        SelectCurrentItem();
        OnInventoryEnabled?.Invoke();
    }

    private void SelectCurrentItem()
    {
        IInventoryItem item = _activeSlot?.Item;
        if (item != null)
        {
            item.Select();
            OnItemSelected?.Invoke(item);
        }
    }

    private void DeselectCurrentItem()
    {
        IInventoryItem item = _activeSlot?.Item;
        if (item != null)
        {
            item.Deselect();
            OnItemDeselected?.Invoke();
        }
    }
}
