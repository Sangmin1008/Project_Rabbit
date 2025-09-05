using System.Collections;
using UnityEngine;

public class EliteShieldEnemyController : BaseEnemyController
{
    #region Serialized 필드값들
    [Header("근접 전용 : 히트 박스 스폰 지점")]
    [SerializeField] private Transform _meleeSpawnPoint;

    [Header("근접 전용 : 타겟 레이어 마스크")]
    [SerializeField] private LayerMask _targetMask;

    [Header("근접 전용 : 애니메이션 이벤트 핸들러")]
    [SerializeField] private OnAttackEventHandler _eventHandler;

    [Header("실드 피격 플래시 색상")]
    [SerializeField, ColorUsage(false, true)]
    private Color _blockFlashColor = Color.white;

    #endregion

    #region Sound
    private AudioSource _localAudioSource;

    #endregion

    #region IUnitController overrides
    public override Transform HitBoxSpawnPoint
    {
        get { return _meleeSpawnPoint; }
    }

    public override Transform FirePoint
    {
        get { return null; }
    }

    public override float MinAttackDistance
    {
        get { return 0f; }
    }

    #endregion

    #region Attack 관련 정의
    public override AttackType AttackType
    {
        get { return HitData.Type; }
    }

    public override float AttackRange
    {
        get { return HitData.Range;  }
    }

    protected override HitData HitData
    {
        get { return Data.MeleeHitData; }
    }

    #endregion

    #region 유니티 콜백

    protected override void Awake()
    {
        base.Awake();

        _localAudioSource = gameObject.AddComponent<AudioSource>();
        _localAudioSource.spatialBlend = 0f;
        _localAudioSource.playOnAwake = false;

    }
    protected override void Start()
    {
        base.Start();

        if (_eventHandler != null && _eventHandler.Controller == null)
        {
            _eventHandler.Controller = this;
        }
    }
    #endregion

    #region Combat Initialization
    protected override void InitializeCombatHandlers()
    {
        _damageReceiver = new DamageReceiver(StatManager, this, OnDeathRoutine(), this);
    }
    #endregion

    #region Attack 로직
    public override void Attack()
    {
        if (IsAttacking)
        {
            return;
        }

        IsAttacking = true;

        ShowOutline();

        _eventHandler?.ResetHitFlag();

        PlayBasicAttack();

        StartCoroutine(DoMeleeAttackRoutine());
    }

    private IEnumerator DoMeleeAttackRoutine()
    {
        yield return new WaitForSeconds(HitData.WindupTime);

        yield return new WaitForSeconds(HitData.ActiveTime);

        HideOutline();

        IsAttacking = false;
    }

    public override void PlayAttackSound()
    {
        if (_attackSfx == null)
        {
            return;
        }

        float volume = SceneAudioManager.Instance != null ? SceneAudioManager.Instance.GetSfxVolume() : 0f;

        _localAudioSource.volume = volume;
        _localAudioSource.PlayOneShot(_attackSfx);
    }


    #endregion

    #region Melee Hit Execution
    public override void ExecuteMeleeHit()
    {
        Vector2 center = _meleeSpawnPoint.position;

        float radius = HitData.Range;

        var hits = Physics2D.OverlapCircleAll(center, radius, _targetMask);

        foreach (var c in hits)
        {
            if (c.TryGetComponent<IDamageable>(out var dmg) && !dmg.IsDead)
            {
                dmg.TakeDamage(this);
            }
        }
    }

    #endregion

    #region 실드 특수 피격 처리
    public override void TakeDamage(IAttackable attacker)
    {
        if (IsDead)
        {
            return;
        }

        //  플레이어 공격인지 체크
        var player = attacker as PlayerController;

        //  강공격만 데스, 그 외에는 피격 플래시만
        if (player != null && player.PlayerAttackHandler.CurrentAttackInfoData.IsStrongAttack)
        {
            // 부모 피격 처리 정상 실행
            base.TakeDamage(attacker); 
        }

        else
        {
            // 무적: 피격 연출만 발생, 체력 감소/사망 없음
            if (_flashRenderer != null)
            {
                if (_flashRoutine != null)
                {
                    StopCoroutine(_flashRoutine);
                }

                _flashRoutine = StartCoroutine(FlashCoroutine(_blockFlashColor));
            }
        }
    }
    #endregion

    #region Death 콜백
    private IEnumerator OnDeathRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        ChangeState(BaseEnemyState.Die);
    }

    #endregion
}
