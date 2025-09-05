using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParallaxLayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private int layerIndex;
    [SerializeField] private int initialPoolSize = 3;
    [SerializeField] private float recycleDistanceMultiplier = 1.0f;
    
    private Dictionary<Transform, float> _segmentYPositions = new Dictionary<Transform, float>();
    private List<GameObject> _layers;
    private float _parallaxWeight;
    private float _segmentWidth;
    private float _lastCameraX;
    private float _groundY;

    private LinkedList<Transform> _activeSegments = new LinkedList<Transform>();
    private Queue<Transform> _objectPool = new Queue<Transform>();

    private void OnEnable()
    {
        if (BackgroundManager.Instance != null)
            BackgroundManager.Instance.OnStartCompleted += Initialize;
    }

    private void LateUpdate()
    {
        UpdateLayer();
    }
    
    private void Initialize()
    {
        _layers = BackgroundManager.Instance.Layers[layerIndex];
        _parallaxWeight = BackgroundManager.Instance.Weights[layerIndex];
        _groundY = BackgroundManager.Instance.BottomTarget.position.y + BackgroundManager.Instance.GroundOffset;
        
        InitializePool();
        
        _segmentWidth = _layers[0].GetComponent<SpriteRenderer>().bounds.size.x;
        _lastCameraX = cameraTransform.position.x;
        
        SpawnInitialSegments();
        BackgroundManager.Instance.OnStartCompleted -= Initialize;
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject background = Instantiate(_layers[i % _layers.Count], transform);
            background.SetActive(false);
            _objectPool.Enqueue(background.transform);
        }
    }

    private void SpawnInitialSegments()
    {
        SpawnSegment(0); 
        SpawnSegment(_segmentWidth);
        SpawnSegment(-_segmentWidth);
    }
    
    private void SpawnSegment(float offset)
    {
        if (_objectPool.Count == 0)
            return;

        Transform segment = _objectPool.Dequeue();
        segment.gameObject.SetActive(true);
        
        float parallaxPosX = (cameraTransform.position.x * _parallaxWeight) + offset;
        
        var spriteRenderer = segment.GetComponent<SpriteRenderer>();
        float halfHeight = spriteRenderer.bounds.size.y / 2;
        float height = _groundY + halfHeight;
        
        segment.position = new Vector3(parallaxPosX, height, 0);
        _segmentYPositions[segment] = height;

        if (offset > 0 && _activeSegments.Count > 0)
            _activeSegments.AddLast(segment);
        else
            _activeSegments.AddFirst(segment);
    }

    private void UpdateLayer()
    {
        float deltaX = (cameraTransform.position.x - _lastCameraX) * _parallaxWeight;
        transform.position += new Vector3(deltaX, 0, 0);
        _lastCameraX = cameraTransform.position.x;

        float threshold = _segmentWidth * recycleDistanceMultiplier;
        Transform firstSegment = _activeSegments.First.Value;
        Transform lastSegment  = _activeSegments.Last.Value;

        if (cameraTransform.position.x > firstSegment.position.x + threshold)
        {
            MoveSegment(true);
        }
        if (cameraTransform.position.x < lastSegment.position.x - threshold)
        {
            MoveSegment(false);
        }
    }

    private void MoveSegment(bool isMovingRight)
    {
        if (isMovingRight)
        {
            Transform segmentToMove = _activeSegments.First.Value;
            _activeSegments.RemoveFirst();
            
            float newX = _activeSegments.Last.Value.position.x + _segmentWidth;
            segmentToMove.position = new Vector3(newX, _segmentYPositions[segmentToMove], segmentToMove.position.z);
            _activeSegments.AddLast(segmentToMove);
        }
        else
        {
            Transform segmentToMove = _activeSegments.Last.Value;
            _activeSegments.RemoveLast();

            float newX = _activeSegments.First.Value.position.x - _segmentWidth;
            segmentToMove.position = new Vector3(newX, _segmentYPositions[segmentToMove], segmentToMove.position.z);
            _activeSegments.AddFirst(segmentToMove);
        }
    }
}