using UnityEngine;
using System.Collections;
using KinematicCharacterController.Examples;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Item : NetworkBehaviour, IInventoryItem
{
    [SerializeField] private ItemData _data;
    [SerializeField] private NetworkTransform _networkTransform;
    [SerializeField] private Renderer _meshRenderer;
    [SerializeField] private Collider _itemCollider;
    [SerializeField] protected Vector3 _inHandLocalPosition;
    [SerializeField] protected Vector3 _inHandLocalRotation;
    [SerializeField] protected Vector3 _inHandLocalScale;
    [SerializeField] protected Vector3 _inHandNetworkPosition;
    [SerializeField] protected Vector3 _inHandNetworkRotation;
    [SerializeField] private LayerMask _ignoreLayers;
    [SerializeField] private RenderingLayerMask _ownerLightLayer;
    [SerializeField] private float _dropSpeed = 2f;
    [SerializeField] private float _maxDropDistance = 10f;
    private NetworkVariable<NetworkObjectReference> _currentPlayerHolder = new NetworkVariable<NetworkObjectReference>();
    private NetworkVariable<bool> _isInInventory = new NetworkVariable<bool>();
    private NetworkVariable<bool> _isInInventorySelected = new NetworkVariable<bool>();
    private GameObject _localItem;
    protected Transform _networkHandTransform;
    private Coroutine _dropCoroutine;

    public ItemData Data => _data;
    public NetworkVariable<bool> CanBeDropped { get; set; } = new NetworkVariable<bool>(true);
    public virtual int AnimatorTriggerHash { get; }
    public bool IsInInventory => _isInInventory.Value;
    public virtual bool IsTwoHanded => false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _isInInventory.OnValueChanged += OnIsInInventoryChanged;
        _isInInventorySelected.OnValueChanged += OnIsInInventorySelectedChanged;

        UpdateColliderState();

        if (_isInInventory.Value && _isInInventorySelected.Value)
            EnableItem();
        else if (_isInInventory.Value && !_isInInventorySelected.Value)
            DisableItem();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _isInInventory.OnValueChanged -= OnIsInInventoryChanged;
        _isInInventorySelected.OnValueChanged -= OnIsInInventorySelectedChanged;
    }

    private void LateUpdate()
    {
        if (!IsSpawned || !_isInInventory.Value)
            return;

        if (_currentPlayerHolder.Value.TryGet(out NetworkObject playerObj))
        {
            if (_networkHandTransform == null)
            {
                var player = playerObj.GetComponent<Player>();
                _networkHandTransform = player.PlayerVisuals.HandsVisualsData.NetworkHandTransform;
            }
            else
            {
                SyncNetworkTransform(false);
                transform.position = _networkHandTransform.position + _networkHandTransform.rotation * _inHandNetworkPosition;
                transform.rotation = _networkHandTransform.rotation * Quaternion.Euler(_inHandNetworkRotation);
            }

        }
    }

    private void OnIsInInventoryChanged(bool previousValue, bool newValue)
    {
        UpdateColliderState();

        if (!newValue)
        {
            UnsetReferences();
        }
    }

    private void OnIsInInventorySelectedChanged(bool previousValue, bool newValue)
    {
        if(_isInInventory.Value)
        {
            if(newValue)
                EnableItem();
            else
                DisableItem();
        }
    }

    private void SyncNetworkTransform(bool isSyncing)
    {
        _networkTransform.Interpolate = isSyncing;
        _networkTransform.SyncPositionX = isSyncing;
        _networkTransform.SyncPositionY = isSyncing;
        _networkTransform.SyncPositionZ = isSyncing;
        _networkTransform.SyncRotAngleX = isSyncing;
        _networkTransform.SyncRotAngleY = isSyncing;
        _networkTransform.SyncRotAngleZ = isSyncing;
    }

    private void UpdateColliderState()
    {
        if (_itemCollider != null)
        {
            _itemCollider.enabled = !_isInInventory.Value;
        }
    }

    public void Select()
    {
        SelectServerRpc();

        if (_localItem != null)
            _localItem.SetActive(true);
    }

    public void Deselect()
    {
        DeselectServerRpc();

        if(_localItem != null)
            _localItem.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc()
    {
        _isInInventorySelected.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeselectServerRpc()
    {
        _isInInventorySelected.Value = false;
    }

    protected virtual void EnableItem()
    {
        gameObject.SetActive(true);
    }
    
    protected virtual void DisableItem()
    {
        gameObject.SetActive(false);
    }

    public virtual bool TryCollect(NetworkObjectReference player)
    {
        if(!_isInInventory.Value)
        {
            RequestCollectServerRpc(player);
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(NetworkObjectReference player)
    {
        if(!_isInInventory.Value && player.TryGet(out NetworkObject playerObj) && playerObj != null)
        {
            var playerComponent = playerObj.GetComponentInChildren<Player>();
            if(playerComponent != null)
            {
                NetworkObject.ChangeOwnership(playerObj.OwnerClientId);
                _isInInventory.Value = true;
                _isInInventorySelected.Value = true;
                _currentPlayerHolder.Value = player;
                _networkHandTransform = playerComponent.PlayerVisuals.HandsVisualsData.NetworkHandTransform;
                transform.SetParent(playerComponent.transform);
                
                CollectClientRpc(player);
            }
        }
    }

    [ClientRpc]
    protected void CollectClientRpc(NetworkObjectReference player)
    {
        if(_dropCoroutine != null)
        {
            StopCoroutine(_dropCoroutine);
            _dropCoroutine = null;
        }

        if(player.TryGet(out NetworkObject playerObj) && playerObj != null)
        {
            var playerComponent = playerObj.GetComponentInChildren<Player>();
            if(playerComponent != null)
            {
                if(playerObj.IsOwner)
                {
                    Transform fpsHandTransform = playerComponent.PlayerVisuals.HandsVisualsData.FPSHandTransform;
                    _localItem = Instantiate(_data.localPrefab, fpsHandTransform.position, fpsHandTransform.rotation, fpsHandTransform);
                    _localItem.transform.SetLocalPositionAndRotation(_inHandLocalPosition, Quaternion.Euler(_inHandLocalRotation));
                    _localItem.transform.localScale = _inHandLocalScale;
                    OnCollected();
                }
            }
        }
    }

    protected virtual void OnCollected()
    {
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        _meshRenderer.renderingLayerMask = _ownerLightLayer;
    }

    public virtual bool TryDrop()
    {
        if(_isInInventory.Value && CanBeDropped.Value && gameObject.activeSelf)
        {
            if(_localItem != null)
                Destroy(_localItem);

            OnDropped();
            RequestDropServerRpc();
            return true;
        }
        
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDropServerRpc()
    {
        _isInInventory.Value = false;
        _isInInventorySelected.Value = false;
        _currentPlayerHolder.Value = new NetworkObjectReference();
        transform.SetParent(null);
        StartCoroutine(DropItem());
    }

    protected virtual void OnDropped()
    {
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        _meshRenderer.renderingLayerMask = RenderingLayerMask.defaultRenderingLayerMask;
    }

    private void UnsetReferences()
    {   
        SyncNetworkTransform(true);
        _networkHandTransform = null;
    }

    private IEnumerator DropItem()
    {
        Vector3 targetPos;
        float originalYRotation = transform.eulerAngles.y;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit surfaceHit, _maxDropDistance, ~_ignoreLayers))
            targetPos = surfaceHit.point;
        else
            yield break;

        float moveProgress = 0;
        Vector3 startingPos = transform.position;
        
        while (moveProgress < 1)
        {
            moveProgress += Time.deltaTime * _dropSpeed;
            transform.position = Vector3.Lerp(startingPos, targetPos, moveProgress);
            yield return null;
        }

        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, surfaceHit.normal);
        Vector3 newRotation = surfaceRotation.eulerAngles;
        newRotation.y = originalYRotation;
        transform.rotation = Quaternion.Euler(newRotation);

        _dropCoroutine = null;
    }
}
