using System;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground Detector")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    
    public bool IsGrounded => GroundCheckBlockTime <= 0f && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    public float GroundCheckBlockTime { get; set; } = 0f;
    public float GroundCheckDisableDuration => 0.1f;
    public Transform GroundCheckBlock => groundCheck;

    private void Update()
    {
        CheckGroundState();
    }

    private void CheckGroundState()
    {
        if (GroundCheckBlockTime > 0f)
            GroundCheckBlockTime -= Time.deltaTime;
    }
}
