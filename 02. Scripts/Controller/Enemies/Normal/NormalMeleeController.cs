using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NormalMeleeController : BaseEnemyController
{
    #region 필드 값들
    [Header("근접 전용 : 히트 박스 스폰 지점")]
    [SerializeField] private Transform _meleeSpawnPoint;

    [Header("근접 전용 : 타겟 레이어 마스크")]
    [SerializeField] private LayerMask _targetMask;

    [Header("근접 전용 : 애니메이션 이벤트 핸들러")]
    [SerializeField] private OnAttackEventHandler _eventHandler;

    #endregion

    #region Sound
    private AudioSource _localAudioSource;

    #endregion

    #region IUnitController 구현
    public override Transform HitBoxSpawnPoint
    {
        get { return _meleeSpawnPoint;}
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
        get { return HitData.Range; }
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

    #region Combat 뭐시기
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

        // 히트 플래그 초기화
        _eventHandler?.ResetHitFlag();

        // 기본 공격 애니메이션 재생
        PlayBasicAttack();

        // 공격 시간 관리
        StartCoroutine(DoMeleeAttackRoutine());
    }

    private IEnumerator DoMeleeAttackRoutine()
    {
        yield return new WaitForSeconds(HitData.WindupTime);

        //PlayAttackSound();

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

    public override void ExecuteMeleeHit()
    {
        Vector2 center = _meleeSpawnPoint.position;
        float radius = HitData.Range;

        var hits = Physics2D.OverlapCircleAll(center, radius, _targetMask);

        foreach (var targetCollider in hits)
        {
            if (targetCollider.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
            {
                damageable.TakeDamage(this);
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
