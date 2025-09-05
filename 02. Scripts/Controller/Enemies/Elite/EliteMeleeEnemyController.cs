using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class EliteMeleeEnemyController : BaseEnemyController
{
    #region Serialized 필드값들
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

    #region IUnitController overrides
    public override Transform HitBoxSpawnPoint => _meleeSpawnPoint;

    public override Transform FirePoint => null;

    public override float MinAttackDistance => 0f;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        _localAudioSource = gameObject.AddComponent<AudioSource>();
        _localAudioSource.spatialBlend = 0f;
        _localAudioSource.playOnAwake = false;
    }

    #region 유니티 콜백
    protected override void Start()
    {
        base.Start();

        if (_eventHandler != null && _eventHandler.Controller == null)
        {
            _eventHandler.Controller = this;
        }
    }

    #endregion

    #region Attack 관련 정의
    public override AttackType AttackType => HitData.Type;

    public override float AttackRange => HitData.Range;

    protected override HitData HitData => Data.MeleeHitData;
    #endregion

    #region 테두리 추가 유지 시간
    [Header("테두리 추가 유지 시간")]
    [Tooltip("발사 후에도 추가로 얼마나(초) 테두리를 유지할지")]
    [SerializeField] private float _outlineExtraKeep;

    #endregion

    #region Combat Initialization
    protected override void InitializeCombatHandlers()
    {
        //  데미지 수신기는 BaseEnemyController에서 StatManager 사용
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

        _eventHandler?.ResetHitFlag();

        //  공격 애니메이션 실행
        PlayBasicAttack();

        //  attack 애니메이션 길이 + ActiveTime 이후 자동으로 공격 종료하기
        StartCoroutine(DoMeleeAttackRoutine());
    }

    private IEnumerator DoMeleeAttackRoutine()
    {
        yield return new WaitForSeconds(HitData.WindupTime);

        float waitTime = HitData.ActiveTime + _outlineExtraKeep;

        yield return new WaitForSeconds(waitTime);

        IsAttacking = false;
    }

    public override void PlayAttackSound()
    {
        if (_attackSfx == null)
        {
            return;
        }

        float volume = SceneAudioManager.Instance != null ? SceneAudioManager.Instance.GetSfxVolume() : 1f;

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

    #region Death 콜백
    private IEnumerator OnDeathRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        ChangeState(BaseEnemyState.Die);
    }
    #endregion
}
