using System;
using UnityEngine;

// TODO: Make separate class for handsGameobject, so CanSwitchSlot could be checked by the hands class IsActive state instead of the GameObject activeSelf state
// maybe we can make a boolean like CanInteractWithInventory 
public class HandSlotSwitcher
{
    private PlayerLegacyInput _playerInput;
    private Inventory _handInventory;
    private Action _handleAlpha1;
    private Action _handleAlpha2;
    
    public event Action<InventorySlot> OnSlotSwitched;
    
    public HandSlotSwitcher(Inventory handInventory, PlayerLegacyInput playerInput)
    {
        _handInventory = handInventory;
        _playerInput = playerInput;
        _handleAlpha1 = () => SwitchSlot(0);
        _handleAlpha2 = () => SwitchSlot(1);
    }

    public void SetInputEvents()
    {
        _playerInput.OnSlot1 += _handleAlpha1;
        _playerInput.OnSlot2 += _handleAlpha2;
    }

    public void RemoveInputEvents()
    {
        _playerInput.OnSlot1 -= _handleAlpha1;
        _playerInput.OnSlot2 -= _handleAlpha2;
    }

    private void SwitchSlot(int slotNumber)
    {
        if (_handInventory.TryChangeActiveSlot(slotNumber))
            OnSlotSwitched?.Invoke(_handInventory.ActiveSlot);
    }
}
