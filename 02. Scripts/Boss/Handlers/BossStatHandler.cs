using System;
using System.Collections;
using UnityEngine;

public class BossStatHandler : MonoBehaviour
{
    #region 필드 값 세팅
    private StatManager _statManager;
    private GaugeManager _gaugeManager;
    private DamageReceiver _damageReceiver;
    private BossSO _data;
    private bool _isDead;

    public event Action OnDeath;

    private IEnumerator _onDeathRoutine;
    private MonoBehaviour _host;

    #endregion

    #region 프로퍼티
    public StatManager StatManager
    {
        get { return _statManager; } 
    }

    public GaugeManager GaugeManager
    {
        get { return _gaugeManager; }
    }

    public bool IsDead
    {
        get { return _isDead || _statManager.GetValueSafe(StatType.CurHp, 0f) <= 0f; }
    }

    public float MoveSpeed
    {
        get { return _statManager.GetValueSafe(StatType.MoveSpeed, 0f); }
    }

    #endregion

    #region 초기화
    public void Initialize(StatManager statManager, BossSO data, IUnitController unit, IEnumerator onDeath, MonoBehaviour host)
    {
        _statManager = statManager;
        _data = data;
        _statManager.Initialize(data, unit);

        _damageReceiver = new DamageReceiver(_statManager, unit, onDeath, host);
        _gaugeManager = new GaugeManager(data.maxBasicGauge, data.gaugePerBasicAttack);
        _isDead = false;

        _onDeathRoutine = onDeath;
        _host = host;
    }

    #endregion

    #region 피격, 사망 처리
    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead)
        {
            return;
        }

        _damageReceiver.TakeDamage(attacker);

        //  피격 후 사망 상태 체크
        if (_statManager.GetValueSafe(StatType.CurHp,0f) <= 0f)
        {
            MarkDead();
        }
    }

    public void MarkDead()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        //  외부 구독자에게 알림
        OnDeath?.Invoke();

        if (_host != null && _onDeathRoutine != null)
        {
            _host.StartCoroutine(_onDeathRoutine);
        }
    }

    #endregion

    #region 게이지 관련
    public void AddGauge()
    {
        _gaugeManager.Add();
    }

    public bool TryConsumeGauge()
    {
        return _gaugeManager.TryConsumeIfFull();
    }

    public void ResetGauge()
    {
        _gaugeManager.Reset();
    }

    #endregion
}
