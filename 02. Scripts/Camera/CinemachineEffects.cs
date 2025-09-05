using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineEffects : SceneOnlySingleton<CinemachineEffects>
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin _noise;
    private Coroutine _zoomRoutine;

    public int CurrentCameraIndex = 0;
    

    protected override void Awake()
    {
        base.Awake();
        if (cinemachineCamera != null)
        {
            _noise = cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        }
    }

    public void ChangeCamera(CinemachineCamera camera)
    {
        if (camera == null) return;
        if (camera.gameObject == null) return;
        
        cinemachineCamera = camera;
        _noise = cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
    }

    public CinemachineCamera GetCamera() => cinemachineCamera;

    public void ShakeCamera(float intensity, float time)
    {
        if (_noise == null) return;

        _noise.AmplitudeGain = intensity;
        Invoke(nameof(ResetShake), time);
    }

    private void ResetShake()
    {
        if (_noise == null) return;

        _noise.AmplitudeGain = 0f;
    }

    public void Zoom(float targetZoomValue, float zoomRatio, Transform targetTransform, float startDuration, float endDuration, bool keepZoom, AnimationCurve curve)
    {
        if (_zoomRoutine != null)
            StopCoroutine(_zoomRoutine);
        
        _zoomRoutine = StartCoroutine(ZoomRoutine(targetZoomValue, zoomRatio, targetTransform, startDuration, endDuration, keepZoom, curve));
    }
    
    private IEnumerator ZoomRoutine(float targetZoomValue, float zoomRatio, Transform targetTransform, float startDuration, float endDuration, bool keepZoom, 
        AnimationCurve curve)
    {
        if (cinemachineCamera == null || cinemachineCamera.gameObject == null)
            yield break;
        
        targetZoomValue *= zoomRatio;
        
        float timer = 0f;
        float defaultValue = cinemachineCamera.Lens.OrthographicSize;

        Transform cameraTransform = cinemachineCamera.gameObject.transform;
        Vector3 defaultPos = cameraTransform.position;
        Vector3 targetPos  = targetTransform.position;

        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime / startDuration;
            float t = Mathf.Clamp01(timer);
            float lerp = (curve != null) ? curve.Evaluate(t) : t;

            float value = Mathf.Lerp(defaultValue, targetZoomValue, lerp);
            cinemachineCamera.Lens.OrthographicSize = value;

            float moveRatio = 1f - (value / defaultValue);
            
            cameraTransform.position = Vector3.Lerp(defaultPos, targetPos, moveRatio);

            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.1f);

        if (!keepZoom)
        {
            timer = 0f;
            while (timer < 1f)
            {
                timer += Time.unscaledDeltaTime / endDuration;
                float t = Mathf.Clamp01(timer);
                float lerp = (curve != null) ? curve.Evaluate(t) : t;

                float value = Mathf.Lerp(targetZoomValue, defaultValue, lerp);
                cinemachineCamera.Lens.OrthographicSize = value;
                
                float moveRatio = 1f - (value / defaultValue);
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, defaultPos, lerp);

                yield return null;
            }
        }

        _zoomRoutine = null;
    }
}
