using System;
using System.Collections;
using UnityEngine;

public class AreaProjectile : MonoBehaviour, IAttackable
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float directionChangeInterval = 5f;
    public StatBase AttackStat { get; private set; }
    public IDamageable Target { get; private set; }
    [field:SerializeField] public AttackType AttackType { get; set; }

    private void Start()
    {
        AttackStat = new CalculatedStat(StatType.AttackPow, 20);
        StartCoroutine(MoveLeftRightCoroutine());
    }

    public void Attack()
    {
        Target.TakeDamage(this);
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Target = damageable;
            Attack();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            if (damageable == Target) Target = null;
        }
    }
    
    private IEnumerator MoveLeftRightCoroutine()
    {
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        while (true)
        {
            yield return MoveInDirection(left, directionChangeInterval);

            yield return MoveInDirection(right, directionChangeInterval);
        }
    }

    private IEnumerator MoveInDirection(Vector3 direction, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
