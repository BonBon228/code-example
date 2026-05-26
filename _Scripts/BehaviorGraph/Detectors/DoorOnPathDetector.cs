using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DoorOnPathDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _doorLayerMask;
    [SerializeField] private float _raycastHeightOffset = 1f;
    [SerializeField] private bool _drawDebugRays = true;
    private DoorOpening _doorOpening;
    private NavMeshAgent _agent;

    public DoorOpening DoorOpening => _doorOpening;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (_agent == null)
            return;

        if (_drawDebugRays)
            DrawPathRays(_agent.transform.position, _agent.destination);
    }

    public bool TryGetDoorOnPath(out DoorOpening doorOpening)
    {
        doorOpening = GetDoorOnPath();
        return doorOpening != null;
    }

    public bool TryGetDoorOnPath(Vector3 pathStart, Vector3 pathEnd, out DoorOpening doorOpening)
    {
        doorOpening = GetDoorOnPath(pathStart, pathEnd);
        return doorOpening != null;
    }

    public DoorOpening GetDoorOnPath()
    {
        if (_agent == null)
            return null;

        return GetDoorOnPath(_agent.transform.position, _agent.destination);
    }

    public DoorOpening GetDoorOnPath(Vector3 pathStart, Vector3 pathEnd)
    {
        if (_agent == null)
            return null;

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(pathStart, pathEnd, NavMesh.AllAreas, path))
            return null;

        if (path.corners == null || path.corners.Length < 2)
            return null;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 direction = path.corners[i + 1] - path.corners[i];
            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
                continue;

            Vector3 rayStart = path.corners[i] + Vector3.up * _raycastHeightOffset;
            Ray ray = new Ray(rayStart, direction.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, distance, _doorLayerMask))
            {
                if (hit.collider.TryGetComponent(out DoorOpening doorOpening) && !doorOpening.IsOpen)
                {
                    _doorOpening = doorOpening;
                    return doorOpening;
                }
            }
        }

        _doorOpening = null;
        return null;
    }

    private void DrawPathRays(Vector3 pathStart, Vector3 pathEnd)
    {
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(pathStart, pathEnd, NavMesh.AllAreas, path))
            return;

        if (path.corners == null || path.corners.Length < 2)
            return;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 direction = path.corners[i + 1] - path.corners[i];
            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
                continue;

            Vector3 rayStart = path.corners[i] + Vector3.up * _raycastHeightOffset;
            Debug.DrawRay(rayStart, direction.normalized * distance, Color.green);
        }
    }

    public void Clear()
    {
        _doorOpening = null;
    }
}
