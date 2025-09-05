using System;
using System.Collections;
using UnityEngine;

public class FallDetector : MonoBehaviour, IAttackable
{
    [SerializeField] private Transform transform;
    [SerializeField] private float damage;
    private WaitForSecondsRealtime _waitForSecondsRealtime = new WaitForSecondsRealtime(0.5f);
    public StatBase AttackStat { get; private set; }
    public IDamageable Target { get; private set; }

    public AttackType AttackType { get; private set; }
    

    private void Start()
    {
        AttackStat = new CalculatedStat(StatType.AttackPow, damage);
        AttackType = AttackType.Light;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            Target = player;
            Attack();
            if (player.IsDead)
                return;
            StartCoroutine(SetPositionCoroutine(player.transform));
        }
    }

    private IEnumerator SetPositionCoroutine(Transform playerTransform)
    {
        yield return _waitForSecondsRealtime;
        playerTransform.position = transform.position;
    }

    public void Attack()
    {
        Target.TakeDamage(this);
    }
}
