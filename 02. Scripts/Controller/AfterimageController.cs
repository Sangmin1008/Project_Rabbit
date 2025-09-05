using System;
using UnityEngine;

public class AfterimageController : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem particleSystem;
    
    private ParticleSystemRenderer _particleSystemRenderer;
    private ParticleSystem.ColorOverLifetimeModule _colModule;
    private Vector3 _flipLocalScale = new Vector3(-1f, 1f, 1f);
    private Vector3 _originalScale = new Vector3(1f, 1f, 1f);
    
    private bool _isFlipX = false;

    private void Awake()
    {
        _particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

        _colModule = particleSystem.colorOverLifetime;
        _colModule.enabled = true;
    }

    private void Start()
    {
        particleSystem.Stop(true,  ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void StartEffect(Material material, Gradient gradient, bool isFlipX)
    {
        _particleSystemRenderer.material = material;
        _isFlipX = isFlipX;
        _colModule.color = new ParticleSystem.MinMaxGradient(gradient);
        StartEffect();
    }

    private void StartEffect()
    {
        particleSystem.Play();
        particleSystem.transform.localScale = _isFlipX ? _flipLocalScale : _originalScale;
    }

    public void Stop()
    {
        particleSystem.Stop();
    }
}
