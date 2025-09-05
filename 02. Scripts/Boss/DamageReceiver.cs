using UnityEngine;
using System.Collections;

public class DamageReceiver
{
    private readonly StatManager _statManager;
    private readonly IEnumerator _onDeathCoroutine;
    private readonly IUnitController _unit;
    private readonly MonoBehaviour _coroutineHost;

    private bool _isDead = false;
    private bool _isInvulnerable;

    public DamageReceiver(StatManager statManager, IUnitController unit, IEnumerator onDeathCoroutine, MonoBehaviour coroutineHost)
    {
        _statManager = statManager;
        _unit = unit;
        _onDeathCoroutine = onDeathCoroutine;
        _coroutineHost = coroutineHost;
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead || _unit.IsDead)
        {
            return;
        }

        float damage = attacker.AttackStat?.Value ?? 0;

        _statManager.Consume(StatType.CurHp, StatModifierType.Base, damage);

        if (_statManager.GetValue(StatType.CurHp) <= 0)
        {
            _isDead = true;
            _coroutineHost.StartCoroutine(_onDeathCoroutine);
        }
    }

    public void ResetReceiver()
    {
        _isDead = false;
        _isInvulnerable = false;
    }
}
