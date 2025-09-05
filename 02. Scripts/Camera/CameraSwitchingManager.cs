using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchingManager : SceneOnlySingleton<CameraSwitchingManager>
{
    [SerializeField] private int prevCameraPriority = 50;
    [SerializeField] private int activePriority = 101;
    [SerializeField] private int defaultPriority = 1;
    
    private CinemachineCameraController _prevCamera;
    private CinemachineCameraController _currCamera;
    private List<CinemachineCameraController> _cameraControllers = new();

    private float _timer = 0f;
    
    private void OnEnable()
    {
        CinemachineCameraController.OnCameraZoneEnter += HandleCameraEnter;
        CinemachineCameraController.OnCameraZoneExit += HandleCameraExit;
    }

    private void OnDisable()
    {
        CinemachineCameraController.OnCameraZoneEnter -= HandleCameraEnter;
        CinemachineCameraController.OnCameraZoneExit -= HandleCameraExit;
    }
    
    
    public void RegisterCamera(CinemachineCameraController controller)
    {
        if (!_cameraControllers.Contains(controller))
        {
            _cameraControllers.Add(controller);
        }
    }

    private void HandleCameraEnter(CinemachineCameraController enteredCamera)
    {
        if (_prevCamera != null && _prevCamera != enteredCamera)
        {
            _prevCamera.SetPriority(defaultPriority);
        }

        if (_currCamera != null && _currCamera != enteredCamera)
        {
            _currCamera.SetPriority(prevCameraPriority);
        }

        enteredCamera.SetPriority(activePriority);
        CinemachineEffects.Instance.ChangeCamera(enteredCamera.GetCinemachineCamera());

        _prevCamera = _currCamera;
        _currCamera = enteredCamera;
    }

    private void HandleCameraExit(CinemachineCameraController exitedCamera)
    {
        if (exitedCamera == _currCamera)
        {
            //Debug.Log("카메라 정상 교체");
            _prevCamera?.SetPriority(defaultPriority);
            exitedCamera?.SetPriority(prevCameraPriority);
        }
        else
        {
            exitedCamera?.SetPriority(defaultPriority);
        }
    }
}
