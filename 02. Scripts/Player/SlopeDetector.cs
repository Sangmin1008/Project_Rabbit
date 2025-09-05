using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SlopeDetector : MonoBehaviour
{
    [SerializeField] private Transform bottomCheck;
    [SerializeField] private Transform frontCheck;
    [SerializeField] private Transform backCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float distance;
    
    public float MaxAngle;
    public float Angle;
    public Vector2 Perp;
    public bool IsOnSlope { get; private set; }

    private void FixedUpdate()
    {
        CheckIfOnSlope();
        if (IsOnSlope)
        {
            //Debug.Log("나 경사에 있음");
        }
        else
        {
            //Debug.Log("아닌데???");
        }
    }

    void CheckIfOnSlope()
    {
        RaycastHit2D bottomHit = Physics2D.Raycast(bottomCheck.position, Vector2.down, distance, groundLayer);
        RaycastHit2D frontHit = Physics2D.Raycast(frontCheck.position, transform.right, distance, groundLayer);
        RaycastHit2D backHit = Physics2D.Raycast(backCheck.position, -transform.right, distance, groundLayer);

        if (bottomHit || frontHit || backHit)
        {
            if (frontHit)
                CheckSlope(frontHit);
            else if (backHit)
                CheckSlope(backHit);
            else
                CheckSlope(bottomHit);
        }
        
        Debug.DrawLine(bottomHit.point, bottomHit.point + bottomHit.normal, Color.red);
        Debug.DrawLine(bottomHit.point, bottomHit.point + Perp, Color.blue);
        Debug.DrawLine(frontHit.point, frontHit.point + frontHit.normal, Color.red);
        Debug.DrawLine(frontHit.point, frontHit.point + Perp, Color.blue);
        Debug.DrawLine(backHit.point, backHit.point + backHit.normal, Color.red);
        Debug.DrawLine(backHit.point, backHit.point + Perp, Color.blue);
    }

    void CheckSlope(RaycastHit2D hit)
    {
        Angle = Vector2.Angle(hit.normal, Vector2.up);
        Perp = Vector2.Perpendicular(hit.normal).normalized;
        
        IsOnSlope = hit && 0.01f < Angle && Angle < MaxAngle;
    }
}