using System;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private int cameraIndex;
    private Collider2D _zoneCollider;
    
    public int CameraIndex => cameraIndex;

    public static event Action<CinemachineCameraController> OnCameraZoneEnter;
    public static event Action<CinemachineCameraController> OnCameraZoneExit;

    
    private void Awake()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();
        
        CameraSwitchingManager.Instance?.RegisterCamera(this);
        if (cinemachineCamera.TryGetComponent(out CinemachineConfiner2D confiner))
        {
            _zoneCollider = confiner.BoundingShape2D;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCameraZoneEnter?.Invoke(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCameraZoneExit?.Invoke(this);
        }
    }
    
    public void SetPriority(int priority)
    {
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Priority = priority;
        }
    }
    
    public bool IsPlayerInside(PlayerController player)
    {
        if (_zoneCollider == null || player == null) return false;
        return _zoneCollider.bounds.Intersects(player.Collider.bounds);
    }

    public CinemachineCamera GetCinemachineCamera()
    {
        return cinemachineCamera;
    }
}
