using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HidingSpotSensor : NetworkBehaviour
{
    [SerializeField] private FieldOfViewSensor _fieldOfViewSensor;
    [SerializeField] private HidingSpotConfigSO _hidingSpotConfig;
    [SerializeField, Min(0f)] private float _recentlySeenMemoryDuration = 0.5f;
    [SerializeField, Min(1)] private int _maxHidingSpotsToDetect = 20;
    [SerializeField, Min(1)] private int _maxHidingSpotTriggersToDetect = 16;

    private readonly Dictionary<GameObject, float> _lastSeenTimes = new Dictionary<GameObject, float>();
    private readonly List<GameObject> _playersToRemove = new List<GameObject>();
    private HidingSpot[] _hidingSpots;
    private HidingSpot _witnessedHidingSpot;
    private Collider[] _hidingSpotResults;
    private Collider[] _hidingSpotTriggerResults;

    public bool HasWitnessedHidingSpot => _witnessedHidingSpot != null;

    private void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        enabled = true;
        CacheHidingSpots();
        Subscribe();
    }

    public override void OnNetworkDespawn()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (!IsServer || _fieldOfViewSensor == null)
            return;

        RememberVisiblePlayers();
        RemoveExpiredPlayers();
    }

    private void Subscribe()
    {
        if (_hidingSpots == null || _hidingSpots.Length == 0)
            return;

        foreach (HidingSpot hidingSpot in _hidingSpots)
        {
            if (hidingSpot != null)
                hidingSpot.PlayerHidden += OnPlayerHidden;
        }
    }

    private void Unsubscribe()
    {
        if (_hidingSpots != null)
        {
            foreach (HidingSpot hidingSpot in _hidingSpots)
            {
                if (hidingSpot != null)
                    hidingSpot.PlayerHidden -= OnPlayerHidden;
            }
        }
    }

    private void OnPlayerHidden(HidingSpot hidingSpot, GameObject player)
    {
        if (hidingSpot == null || player == null)
            return;

        if (_fieldOfViewSensor == null)
            return;

        if (!HasRecentlySeen(player))
            return;

        _witnessedHidingSpot = hidingSpot;
    }

    public bool TryGetWitnessedHidingSpotTarget(out GameObject hidingSpotTarget)
    {
        hidingSpotTarget = null;

        if (_witnessedHidingSpot == null)
            return false;

        hidingSpotTarget = _witnessedHidingSpot.gameObject;
        return true;
    }

    public void ClearWitnessedHidingSpot()
    {
        _witnessedHidingSpot = null;
    }

    public bool TryGetHidingSpotFromHeardPosition(Vector3 heardPosition, float positionCheckRadius, out GameObject hidingSpotTarget)
    {
        hidingSpotTarget = null;

        if (_hidingSpotConfig == null)
            return false;

        EnsureTriggerResultsBuffer();

        float checkRadius = Mathf.Max(0.01f, positionCheckRadius);
        int hitCount = Physics.OverlapSphereNonAlloc(
            heardPosition,
            checkRadius,
            _hidingSpotTriggerResults,
            _hidingSpotConfig.hidingSpotTriggerLayerMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hidingSpotTrigger = _hidingSpotTriggerResults[i];
            if (hidingSpotTrigger == null)
                continue;

            HidingSpot hidingSpot = hidingSpotTrigger.GetComponentInParent<HidingSpot>();
            if (hidingSpot == null || !hidingSpot.ContainsPosition(heardPosition))
                continue;

            hidingSpotTarget = hidingSpot.gameObject;
            return true;
        }

        return false;
    }

    public bool TryGetRandomHidingSpotInArea(Vector3 origin, out GameObject hidingSpotTarget)
    {
        hidingSpotTarget = null;

        if (_hidingSpotConfig == null)
            return false;

        EnsureHidingSpotResultsBuffer();

        int numColliders = Physics.OverlapSphereNonAlloc(
            origin,
            _hidingSpotConfig.sensorRadius,
            _hidingSpotResults,
            _hidingSpotConfig.checkableLayerMask
        );

        HidingSpot selectedHidingSpot = SelectRandomHidingSpot(numColliders);
        if (selectedHidingSpot == null)
            return false;

        hidingSpotTarget = selectedHidingSpot.gameObject;
        return true;
    }

    private void CacheHidingSpots()
    {
        _hidingSpots = FindObjectsByType<HidingSpot>(FindObjectsSortMode.None);
    }

    private void RememberVisiblePlayers()
    {
        foreach (GameObject visibleObject in _fieldOfViewSensor.Objects)
        {
            if (visibleObject == null)
                continue;

            _lastSeenTimes[visibleObject] = Time.time;
        }
    }

    private void RemoveExpiredPlayers()
    {
        _playersToRemove.Clear();

        foreach (KeyValuePair<GameObject, float> lastSeenTime in _lastSeenTimes)
        {
            if (lastSeenTime.Key == null || Time.time - lastSeenTime.Value > _recentlySeenMemoryDuration)
                _playersToRemove.Add(lastSeenTime.Key);
        }

        foreach (GameObject player in _playersToRemove)
            _lastSeenTimes.Remove(player);
    }

    private bool HasRecentlySeen(GameObject player)
    {
        if (player == null)
            return false;

        if (_fieldOfViewSensor != null && _fieldOfViewSensor.IsInSight(player))
        {
            _lastSeenTimes[player] = Time.time;
            return true;
        }

        return _lastSeenTimes.TryGetValue(player, out float lastSeenTime)
            && Time.time - lastSeenTime <= _recentlySeenMemoryDuration;
    }

    private void EnsureHidingSpotResultsBuffer()
    {
        int maxResults = Mathf.Max(1, _maxHidingSpotsToDetect);

        if (_hidingSpotResults == null || _hidingSpotResults.Length != maxResults)
            _hidingSpotResults = new Collider[maxResults];
    }

    private void EnsureTriggerResultsBuffer()
    {
        int maxResults = Mathf.Max(1, _maxHidingSpotTriggersToDetect);

        if (_hidingSpotTriggerResults == null || _hidingSpotTriggerResults.Length != maxResults)
            _hidingSpotTriggerResults = new Collider[maxResults];
    }

    private HidingSpot SelectRandomHidingSpot(int numColliders)
    {
        HidingSpot selectedHidingSpot = null;
        int validHidingSpotCount = 0;

        for (int i = 0; i < numColliders; i++)
        {
            Collider hit = _hidingSpotResults[i];
            if (hit == null)
                continue;

            HidingSpot hidingSpot = hit.GetComponentInParent<HidingSpot>();
            if (hidingSpot == null)
                continue;

            validHidingSpotCount++;
            if (Random.Range(0, validHidingSpotCount) == 0)
                selectedHidingSpot = hidingSpot;
        }

        return selectedHidingSpot;
    }
}
