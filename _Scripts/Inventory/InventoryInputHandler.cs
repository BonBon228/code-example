using System;

public class InventoryInputHandler
{
    private Inventory _handInventory;
    private PlayerLegacyInput _playerInput;
    private DetectorData _detectorData;
    private ObjectRaycastDetector<IInventoryItem> _itemRaycastDetector;
    private Action _handleAlpha1;
    private Action _handleAlpha2;

    public InventoryInputHandler(
        Inventory handInventory,
        PlayerLegacyInput playerInput,
        DetectorData detectorData)
    {
        _handInventory = handInventory;
        _playerInput = playerInput;
        _detectorData = detectorData;
        _itemRaycastDetector = new ObjectRaycastDetector<IInventoryItem>();

        _handleAlpha1 = () => _handInventory.TryChangeActiveSlot(0);
        _handleAlpha2 = () => _handInventory.TryChangeActiveSlot(1);
    }

    public void Initialize()
    {
        _playerInput.OnInteracted += Collect;
        _playerInput.OnDropped += Drop;
        _playerInput.OnSlot1 += _handleAlpha1;
        _playerInput.OnSlot2 += _handleAlpha2;
    }

    public void Deinitialize()
    {
        _playerInput.OnInteracted -= Collect;
        _playerInput.OnDropped -= Drop;
        _playerInput.OnSlot1 -= _handleAlpha1;
        _playerInput.OnSlot2 -= _handleAlpha2;
    }

    private void Collect()
    {
        if (_itemRaycastDetector.TryGetDetectedObject(_playerInput.MainCameraRay, _detectorData.maxDistance, out IInventoryItem item))
            _handInventory.TryCollectToActiveSlot(item);
    }

    private void Drop()
    {
        _handInventory.TryDropFromActiveSlot();
    }
}
