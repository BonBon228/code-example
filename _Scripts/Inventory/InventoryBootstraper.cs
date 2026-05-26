using System.Collections.Generic;
using KinematicCharacterController.Examples;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class InventoryBootstraper : NetworkBehaviour
{
    [SerializeField] private GameObject _defaultFirstSlotPrefab;
    [SerializeField] private DetectorData _inputHandlerDetectorData;
    [SerializeField] private int _inventorySize = 2;
    
    private Inventory _handInventory;
    private HandSlotsPresenter _presenter;
    private InventoryInputHandler _inputHandler;

    public Inventory SetupHandInventory(Player player, UIPanelsDisplayer panelsDisplayer, PlayerLegacyInput playerInput)
    {
        _handInventory = new Inventory(_inventorySize, player);
        _handInventory.InitializeSlots();

        HUDPanel hudPanel = panelsDisplayer.GetPanel<HUDPanel>();
        hudPanel.Initialize();

        _presenter = new HandSlotsPresenter(
            hudPanel.HandSlotViews, 
            _handInventory
        );
        _presenter.Initialize();

        _inputHandler = new InventoryInputHandler(_handInventory, playerInput, _inputHandlerDetectorData);
        _inputHandler.Initialize();
        
        return _handInventory;
    }

    public void Deinitialize()
    {
        _inputHandler?.Deinitialize();
    }

    [ServerRpc]
    public void SpawnDefaultItemServerRpc()
    {
        GameObject defaultItem = Instantiate(_defaultFirstSlotPrefab);
        NetworkObject networkObj = defaultItem.GetComponent<NetworkObject>();
        networkObj.Spawn();

        if (defaultItem.TryGetComponent(out Item item))
        {
            item.CanBeDropped.Value = false;
            AssignDefaultItemClientRpc(networkObj);
        }
    }

    [ClientRpc]
    private void AssignDefaultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (IsOwner && itemRef.TryGet(out NetworkObject itemObj))
        {
            if (itemObj.TryGetComponent(out IInventoryItem item))
                _handInventory.TryCollectToSlot(_handInventory.DefaultSlot, item);
        }
    }
}