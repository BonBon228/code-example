using System;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController.Examples;
using Unity.Netcode;
using UnityEngine;

[ExecuteInEditMode]
public class FieldOfViewSensor : NetworkBehaviour
{
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _height = 1.5f;
    [SerializeField] private float _horizontalAngle = 30f;
    [SerializeField] private float _verticalAngle = 30f;
    [SerializeField] private Color _color = Color.cyan;
    [SerializeField] private int _scanFrequency = 30;
    [SerializeField] private LayerMask _visibleObjectslayerMask;
    [SerializeField] private LayerMask _obstacleLayerMask;
    private List<GameObject> _objects = new List<GameObject>();
    private GameObject _lastKnownObject;
    private Collider[] _colliders = new Collider[50];
    private Mesh _mesh;
    private int _count;
    private float _scanInterval;
    private float _scanTimer;
    private float HalfHeight => _height / 2;

    private Dictionary<GameObject, Vector3> _lastKnownPositions = new Dictionary<GameObject, Vector3>();

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;

    public GameObject LastKnownObject { get => _lastKnownObject; set => _lastKnownObject = value; }
    public List<GameObject> Objects
    {
        get 
        {
            _objects.RemoveAll(item => item == null); 
            return _objects; 
        }
    }

    private void OnValidate()
    {
        _mesh = CreatePyramidMesh();
    }

    private void Awake()
    {
        _scanInterval = 1f / _scanFrequency;
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
            enabled = true;
    }

    private void Update()
    {
        _scanTimer -= Time.deltaTime;
        if(_scanTimer < 0)
        {
            _scanTimer += _scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        _count = Physics.OverlapSphereNonAlloc(transform.position, _distance, _colliders, _visibleObjectslayerMask, QueryTriggerInteraction.Collide);

        var currentVisibleObjects = new HashSet<GameObject>();
        for (int i = 0; i < _count; ++i)
        {
            if (IsInSight(_colliders[i].gameObject))
            {
                currentVisibleObjects.Add(_colliders[i].gameObject);
                if (!_objects.Contains(_colliders[i].gameObject))
                {
                    _objects.Add(_colliders[i].gameObject);
                    OnPlayerEntered?.Invoke();
                }
            }
        }

        foreach (GameObject obj in _objects.ToList())
        {
            if (!currentVisibleObjects.Contains(obj))
            {
                if (obj != null)
                {
                    _lastKnownPositions[obj] = obj.transform.position;
                    _lastKnownObject = obj;
                }
                _objects.Remove(obj);
                OnPlayerExited?.Invoke();
            }
        }
    }

    public bool IsInSight(GameObject gameObject)
    {
        if (gameObject == null)
            return false;

        PlayerController playerController = gameObject.GetComponentInParent<PlayerController>();
        if (playerController != null && playerController.IsHidden)
            return false;

        Vector3 origin = new Vector3(transform.position.x, transform.position.y - HalfHeight, transform.position.z);
        Vector3 origin2 = new Vector3(transform.position.x, transform.position.y + HalfHeight, transform.position.z);
        Vector3 dest = gameObject.transform.position;
        Vector3 direction = dest - origin;
        Vector3 direction2 = dest - origin2;

        // Проверка горизонтального угла
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);
        float horizontalAngle = Vector3.Angle(horizontalDirection, transform.forward);
        if (horizontalAngle > _horizontalAngle)
            return false;

        // Проверка вертикального угла
        float verticalAngle = Mathf.Atan2(direction.y, horizontalDirection.magnitude) * Mathf.Rad2Deg;
        float verticalAngle2 = Mathf.Atan2(direction2.y, horizontalDirection.magnitude) * Mathf.Rad2Deg;

        if(!IsInHeight(direction) && Mathf.Abs(verticalAngle) > _verticalAngle && Mathf.Abs(verticalAngle2) > _verticalAngle)
            return false;

        // Проверка на наличие препятствий
        if (Physics.Linecast(transform.position, dest, _obstacleLayerMask))
            return false;

        Debug.Log("Object is in sight: " + gameObject.name);
        return true;
    }

    private bool IsInHeight(Vector3 direction)
    {
        if(direction.y > 0 && direction.y < _height)
            return true;
        
        return false;
    }

    private Mesh CreatePyramidMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 70) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 center = new Vector3(0, -HalfHeight, 0);
        Vector3 center2 = new Vector3(0, HalfHeight, 0);

        int vert = 0;

        float currentAngle = -_horizontalAngle;
        float deltaAngle = _horizontalAngle * 2 / segments;
        for(int i = 0; i < segments; ++i)
        {
            Vector3 bottomLeft = Quaternion.Euler(_verticalAngle, currentAngle, 0) * Vector3.forward * _distance;
            Vector3 bottomRight = Quaternion.Euler(_verticalAngle, currentAngle + deltaAngle, 0) * Vector3.forward * _distance;

            Vector3 topRight = Quaternion.Euler(-_verticalAngle, currentAngle + deltaAngle, 0) * Vector3.forward * _distance;
            Vector3 topLeft = Quaternion.Euler(-_verticalAngle, currentAngle, 0) * Vector3.forward * _distance;

            //top
            vertices[vert++] = center2;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            //bottom
            vertices[vert++] = center;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        currentAngle = -_verticalAngle;
        float deltaVertAngle = _verticalAngle * 2 / segments;
        float currentHeight = HalfHeight;
        float deltaHeight = _height / segments;
        for(int i = 0; i < segments; ++i)
        {
            Vector3 bottomLeft = Quaternion.Euler(currentAngle, -_horizontalAngle, 0) * Vector3.forward * _distance;
            Vector3 topLeft = Quaternion.Euler(currentAngle + deltaVertAngle, -_horizontalAngle, 0) * Vector3.forward * _distance;

            Vector3 bottomRight = Quaternion.Euler(currentAngle, _horizontalAngle, 0) * Vector3.forward * _distance;
            Vector3 topRight = Quaternion.Euler(currentAngle + deltaVertAngle, _horizontalAngle, 0) * Vector3.forward * _distance;

            Vector3 currentCenter = new Vector3(0, currentHeight, 0);
            Vector3 deltaCenter = new Vector3(0, currentHeight - deltaHeight, 0);

            //left side
            vertices[vert++] = currentCenter;
            vertices[vert++] = deltaCenter;
            vertices[vert++] = topLeft;

            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;
            vertices[vert++] = currentCenter;

            //right side
            vertices[vert++] = currentCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = deltaCenter;
            vertices[vert++] = currentCenter;

            currentAngle += deltaVertAngle;
            currentHeight -= deltaHeight;
        }

        //for far side
        float currentVerticalAngle = -_verticalAngle;
        for (int i = 0; i < segments; ++i)
        {
            float currentHorizontalAngle = -_horizontalAngle;
            for (int j = 0; j < segments; ++j)
            {
                Vector3 bottomLeft = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0) * Vector3.forward * _distance;
                Vector3 bottomRight = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle + deltaAngle, 0) * Vector3.forward * _distance;
                Vector3 topLeft = Quaternion.Euler(currentVerticalAngle + deltaVertAngle, currentHorizontalAngle, 0) * Vector3.forward * _distance;
                Vector3 topRight = Quaternion.Euler(currentVerticalAngle + deltaVertAngle, currentHorizontalAngle + deltaAngle, 0) * Vector3.forward * _distance;

                // Far side cells
                vertices[vert++] = bottomLeft;
                vertices[vert++] = topLeft;
                vertices[vert++] = bottomRight;

                vertices[vert++] = bottomRight;
                vertices[vert++] = topLeft;
                vertices[vert++] = topRight;

                currentHorizontalAngle += deltaAngle;
            }
            currentVerticalAngle += deltaVertAngle;
        }

        for(int i = 0; i < numVertices; ++i)
            triangles[i] = i;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public int Filter(GameObject[] buffer, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        int count = 0;

        foreach(GameObject obj in _objects)
        {
            if(obj == null)
                continue;

            if(obj.layer == layer)
                buffer[count++] = obj;

            if(buffer.Length == count)
                break;
        }

        return count;
    }

    public Vector3 GetLastKnownPosition()
    {
        if (_lastKnownObject != null && _lastKnownPositions.TryGetValue(_lastKnownObject, out Vector3 position))
            return position;

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if(_mesh != null)
        {
            Gizmos.color = _color;
            Gizmos.DrawMesh(_mesh, transform.position, transform.rotation);
        }
        //Gizmos.DrawWireSphere(transform.position, _distance);
        Gizmos.color = Color.green;
        foreach(GameObject obj in _objects)
        {
            if(obj == null)
                continue;

            Gizmos.DrawSphere(obj.transform.position, 0.2f);
        }
    }
}
