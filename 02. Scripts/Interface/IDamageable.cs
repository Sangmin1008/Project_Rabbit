using UnityEngine;

public interface IDamageable
{
    public bool IsDead { get; }
    Collider2D Collider { get; }

    public void TakeDamage(IAttackable attacker);
    public void Dead();
}