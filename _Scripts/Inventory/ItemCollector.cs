using System;
using KinematicCharacterController.Examples;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    [SerializeField] private DetectorData _detectorData;
    [SerializeField] private Player _player;
    [SerializeField] private PlayerLegacyInput _playerInput;
    private Inventory _handInventory;
    private ObjectRaycastDetector<IInventoryItem> _itemRaycastDetector;

    public event Action<IInventoryItem> OnCollected;

    public void Initialize(Inventory handInventory)
    {
        _handInventory = handInventory;
    }

    private void OnEnable()
    {
        _playerInput.OnInteracted += Collect;
    }

    private void OnDisable() 
    {
        _playerInput.OnInteracted -= Collect;
    }

    private void Start()
    {
        _itemRaycastDetector = new ObjectRaycastDetector<IInventoryItem>();
    }

    private void Collect()
    {
        if (_itemRaycastDetector.TryGetDetectedObject(_playerInput.MainCameraRay, _detectorData.maxDistance, out IInventoryItem item))
        {
            // if (_handInventory.TryCollectToActiveSlot(item, _player))
            // {
            //     OnCollected?.Invoke(item);
            // }
        }
    }

    public bool TryCollect(InventorySlot slot, IInventoryItem item)
    {
        // if (_handInventory.TryCollectToSlot(slot, item, _player))
        // {
        //     OnCollected?.Invoke(item);
        //     return true;
        // }

        return false;
    }
}
