using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider2D))]
public class EliteMeleeHitbox : MonoBehaviour
{
    public IDamageable Target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  플레이어가 아니라면 무시하기
        if (!collision.TryGetComponent<PlayerController>(out var player))
        {
            return;
        }

        Target =  player;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out _))
        {
            Target = null;
        }
    }
}
