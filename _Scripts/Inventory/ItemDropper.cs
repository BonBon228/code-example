using System;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] private PlayerLegacyInput _playerInput;
    private Inventory _handInventory;

    public event Action OnDropped;

    public void Initialize(Inventory handInventory)
    {
        _handInventory = handInventory;
    }

    private void OnEnable()
    {
        _playerInput.OnDropped += Drop;
    }

    private void OnDisable()
    {
        _playerInput.OnDropped -= Drop;
    }

    private void Drop()
    {   
        if (_handInventory.TryDropFromActiveSlot())
            OnDropped?.Invoke();
    }
}
