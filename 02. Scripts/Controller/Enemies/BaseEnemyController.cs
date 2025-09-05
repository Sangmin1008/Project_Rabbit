using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(Collider2D))]

public abstract class BaseEnemyController : BaseController<BaseEnemyController, BaseEnemyState>, IUnitController
{
    #region SerializeField 필드값들
    [Header("식별")]
    [SerializeField] private int _id;
    public int EnemyTypeID => _id;

    [Header("추격 최소 거리 (원거리용)")]
    [SerializeField] private float _minAttackDistance;
    [Header("근접 히트박스 위치")]
    [SerializeField] protected Transform _hitBoxSpawnPoint;
    [Header("원거리 발사 위치")]
    [SerializeField] protected Transform _firePoint;

    [SerializeField] private StatManager _statManager;

    protected SpriteOutline Outline;

    [Header("공통 공격 사운드")]
    [SerializeField] protected AudioClip _attackSfx;

    private float initialGravityScale;

    #endregion

    #region 낙사 처리 설정
    [Header("낙사 처리 설정")]
    [Tooltip("이 값보다 y값이 낮아지면 자동으로 사망 처리 후 풀로 반환")]
    [SerializeField] private float _deadYThreshold = -20f;
    #endregion

    #region IUnitController 프로퍼티들
    public StatManager StatManager
    {
        get => _statManager;
        private set => _statManager = value;
    }

    public Collider2D Collider { get; private set; }
    public IDamageable Target { get; private set; }
    public bool IsDead { get; private set; }
    public abstract Transform HitBoxSpawnPoint { get; }
    public virtual Transform FirePoint
    {
        get { return _firePoint; }
    }
    public abstract float MinAttackDistance { get; }

    private bool _isChasing;
    #endregion

    #region Data & Stats
    public BaseEnemySO Data { get; private set; }
    public float MoveSpeed => StatManager.GetValue(StatType.MoveSpeed);
    public float DetectingRange => Data.DetectingRange;
    public abstract AttackType AttackType { get; }
    public StatBase AttackStat { get; protected set; }
    public abstract float AttackRange { get; }
    protected abstract HitData HitData { get; }
    #endregion

    #region 컴포넌트 & 핸들러들
    private Rigidbody2D _rigidbody;
    protected Animator _animator;
    private AnimationHandler _animationHandler;
    public MovementHandler MovementHandler { get; private set; }
    protected DamageReceiver _damageReceiver;
    private EliteEnemyPool _pool;
    #endregion

    #region 상태 플래그
    public bool IsAttacking { get;  set; }
    #endregion

    #region Attack CoolDown
    protected float _lastAttackTime = -Mathf.Infinity;
    public float LastAttackTime { get; set; }

    public float AttackCooldown
    {
        get { return HitData.Cooldown; }
    }

    #endregion

    #region 피격 플래시 설정
    [Header("피격 플래시 설정 (모든 적 공통)")]
    [SerializeField] protected SpriteRenderer _flashRenderer;
    [SerializeField, ColorUsage(false, true)]
    protected Color _flashColor = Color.red;
    [SerializeField, Range(0.01f, 0.5f)]
    protected float _flashDuration = 0.1f;

    protected Material _material;
    protected Coroutine _flashRoutine;
    #endregion

    #region 사망 넉백 & 스턴 설정
    [Header("사망 넉백 & 스턴 설정")]
    [SerializeField, Range(0f, 0.5f)] private float deathStunDuration = 0.2f; 
    [SerializeField] private float deathArcVerticalForce = 5f;   
    [SerializeField] private float deathArcDuration = 0.5f;
    [SerializeField] private float deathKnockbackForce = 5f;
    [SerializeField, Range(1f, 3f)] private float deathSpeedMultiplier = 1.2f;

    private int _originalLayer;

    [Header("Ground Detection")]
    [SerializeField] private GroundDetector _groundDetector;

    //  착지 처리 여부 플래그
    private bool _landingHandled = false;

    #endregion

    #region 유니티 콜백
    protected override void Awake()
    {
        base.Awake();

        StatManager = GetComponent<StatManager>();
        Collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        MovementHandler = new MovementHandler(_rigidbody, this);
        _animationHandler = new AnimationHandler(_animator, this);

        if (_flashRenderer == null)
        {
            var spriteRenderer = transform.Find("MainSprite");

            if (spriteRenderer != null)
            {
                _flashRenderer = spriteRenderer.GetComponent<SpriteRenderer>();
            }
        }

        //  Shader 기반 Flash / Outline 제어를 위한 머테리얼 인스턴스 생성
        if (_flashRenderer != null)
        {
            _material = Instantiate(_flashRenderer.material);
            _flashRenderer.material = _material;
        }

        //  루트에 붙어있으면 그걸 쓰고, 없으면 자식에서 찾아서 캐싱하기
        Outline = GetComponent<SpriteOutline>() ?? GetComponentInChildren<SpriteOutline>();

        _originalLayer = gameObject.layer;

        if (_groundDetector == null)
        {
            _groundDetector = GetComponentInChildren<GroundDetector>();
        }

        initialGravityScale = _rigidbody.gravityScale;
    }

    protected override void Start()
    {
        var table = TableManager.Instance.GetTable<BaseEnemyTable>();

        if (table == null)
        {
            Debug.LogError("TableManager에 BaseEnemyTable이 없음!!");
        }


        Data = table?.GetDataByID(_id);

        if (Data == null)
        {
            Debug.LogError($" BaseEnemySO(ID={_id})가 BaseEnemyTable에 등록되지 않았습니다!");
        }

        //_rigidbody.gravityScale = Data.gravityScale;

        StatManager.Initialize(Data, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);

        InitializeCombatHandlers();

        base.Start();

        ChangeState(BaseEnemyState.Idle);
    }

    protected override void Update()
    {
        FindTarget();
        base.Update();

        //  낙사 감지
        CheckFallDeath();
    }

    #endregion

    #region Initialization Helpers
    protected abstract void InitializeCombatHandlers();

    public void SetPool(EliteEnemyPool pool) => _pool = pool;
    #endregion

    #region 타겟팅 & Movement
    public override void FindTarget()
    {
        if (Target != null && !Target.IsDead)
        {
            return;
        }

        var player = FindFirstObjectByType<PlayerController>();
        Target = player?.GetComponent<IDamageable>();
    }

    public override void Movement()
    {
        if (Target == null || Target.IsDead)
        {
            return;
        }

        float directX = Target.Collider.bounds.center.x - transform.position.x;
        float horizontalDist = Mathf.Abs(directX);

        if (horizontalDist > HitData.Range)
        {
            MovementHandler.Chase();
            _animationHandler.SetSpeed(Mathf.Abs(_rigidbody.linearVelocity.x));
        }

        else
        {
            MovementHandler.StopXMovement();
            _animationHandler.SetSpeed(0);

            ChangeState(BaseEnemyState.Attack);
        }
    }

    public void MovementWithDistance(float minDistance) => MovementHandler.ChaseWithMinDistance(minDistance);

    public void FaceToTarget()
    {
        if (Target == null)
        {
            return;
        }

        Vector3 dir = Target.Collider.bounds.center - Collider.bounds.center;
        float flipX = dir.x >= 0 ? 1 : -1;

        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * flipX;
        transform.localScale = scale;
    }

    public void LockXPosition()
    {
        //  Y축 회전 허용할까..?
        _rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    public void UnLockXPosition()
    {
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    #endregion

    #region FSM 콜백 - Chasing 진입 / 이탈
    public void OnChaseEnter()
    {
        _isChasing = true;
    }

    public void OnChaseExit()
    {
        _isChasing = false;
    }
    #endregion

    #region 공격 시 나올 사운드 메서드
    public virtual void PlayAttackSound()
    {
        if (_attackSfx != null && SceneAudioManager.Instance != null)
        {
            SceneAudioManager.Instance.PlaySfx(_attackSfx);
        }
    }
    #endregion

    #region IAttackable & IDamageable
    public abstract void Attack();
    
    public virtual void TakeDamage(IAttackable attacker)
    {
        if (IsDead)
        {
            return;
        }

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }

        _flashRoutine = StartCoroutine(FlashCoroutine(_flashColor));

        //  체력 감소 처리
        _damageReceiver.TakeDamage(attacker);

        if (IsDead || StatManager.GetValue(StatType.CurHp) <= 0f)
        {
            ChangeState(BaseEnemyState.Die);
        }
    }

    protected IEnumerator FlashCoroutine(Color color)
    {
        //  FlashColor에 알파를 절반 정도로 낮춰서 반투명으로 덧칠하기
        color.a = 0.8f;
        _material.SetColor("_FlashColor", color);

        float elapsed = 0f;
        float duration = _flashDuration;

        //  시간이 흐르면서 _Flash 값을 1 -> 0으로 줄여서 자연스럽게 페이드 아웃시키기
        while (elapsed < duration)
        {
            float time = elapsed / duration;
            float strength = 1f - time;
            _material.SetFloat("_Flash", strength);

            elapsed += Time.deltaTime;
            yield return null;
        }

        //  완전히 끄기
        _material.SetFloat("_Flash", 0f);
        _flashRoutine = null;
    }

    #endregion

    #region Animations
    public void SetSpeed(float speed) => _animationHandler.SetSpeed(speed);

    public void PlayBasicAttack() => _animationHandler.PlayBasicAttack();

    public void PlayDeath() => _animationHandler.PlayDeath();

    public void PlayPattern(int patternIndex)
    {
    }

    #endregion

    #region 사망 & 풀 반환 처리
    public void Dead()
    {
        //  현재 재생 중인 애니메이션을 즉시 클리어하기
        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
            _animator.speed = 0f;
        }

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        StartDeathSequence();
    }

    public void StartDeathSequence()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        //  모든 이전 코루틴 중지
        StopAllCoroutines();

        //  물리 활성화 & 중력 복원
        _rigidbody.simulated = true;
        _rigidbody.gravityScale = Data != null ? Data.gravityScale : 3f;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        //  GroundDetector 차단 시간 설정
        if (_groundDetector != null)
        {
            _groundDetector.GroundCheckBlockTime = _groundDetector.GroundCheckDisableDuration;
        }

        //  포물선 넉백 
        float sign = Target != null ? Mathf.Sign(transform.position.x - Target.Collider.bounds.center.x) : -1f;

        float boost = 0.3f;

        float vx = sign * deathKnockbackForce * deathSpeedMultiplier * (1f + boost);
        float vy = deathArcVerticalForce * deathSpeedMultiplier;

        _rigidbody.linearVelocity = new Vector2(vx, vy);

        //  넉백→아크 대기 후 애니메이션 재생
        StartCoroutine(DeathAfterArc());
    }

    private IEnumerator DeathAfterArc()
    {
        //  넉백 아크 지속 시간 대기
        yield return new WaitForSeconds(deathArcDuration);

        yield return new WaitUntil(() => _groundDetector != null && _groundDetector.IsGrounded);

        if (!_landingHandled)
        {
            _landingHandled = true;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        //  사망 애니메이션 재생
        if (_animator != null)
        {
            _animator.speed = 1f;
        }

        _animationHandler.ResetTriggers();
        _animationHandler.PlayDeath();

        //  애니메이션 끝나면 Disable, 풀 반환
        float deathLength = GetDeathAnimLength();
        StartCoroutine(DisableAfterDeath(deathLength));
    }

    // 이 코루틴만 살아남고, MonoBehaviour를 꺼도 계속 실행됩니다.
    private IEnumerator DisableAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_rigidbody != null)
        {
            _rigidbody.simulated = false;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        //  Animator 컴포넌트 비활성화 → 더 이상 Move/Idle Blend가 돌아가지 않음
        if (_animator != null)
        {
            _animator.enabled = false;
        }

        //  스크립트 비활성 & 풀 반환
        enabled = false;

        if (_pool != null)
        {
            _pool.Release(this);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private float GetDeathAnimLength()
    {
        foreach (var clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Contains("Die"))
            {
                return clip.length;
            }
        }
            
        return 1f;
    }
    #endregion

    #region FSM 매핑
    protected override IState<BaseEnemyController, BaseEnemyState> GetState(BaseEnemyState state) => state switch
    {
        BaseEnemyState.Idle => new BaseEnemyStates.IdleState(),
        BaseEnemyState.Chasing => new BaseEnemyStates.ChasingState(),
        BaseEnemyState.Attack => new BaseEnemyStates.AttackState(),
        BaseEnemyState.Cooldown => new BaseEnemyStates.CooldownState(),
        BaseEnemyState.Die => new BaseEnemyStates.DieState(),
        _ => new BaseEnemyStates.IdleState()
    };

    #endregion

    #region 리스폰 리셋
    public void ResetForSpawn()
    {
        StopAllCoroutines();

        gameObject.layer = _originalLayer;

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }

        if (_flashRenderer != null && _material != null)
        {
            //  인스턴스화된 머테리얼 할당하기
            _flashRenderer.material = _material;

            //  Flash / Outline 모두 끄기
            _material.SetFloat("_Flash", 0f);
            _material.SetFloat("_Outline", 0f);
        }

        //  상태 플래그 초기화
        IsDead = false;
        IsAttacking = false;
        Target = null;
        _isChasing = false;

        //  GroundDetector 블록 해제 & 착지 플래그 초기화
        _landingHandled = false;

        if (_groundDetector != null)
        {
            _groundDetector.GroundCheckBlockTime = 0f;
        }

        //  Rigidbody 리셋
        if (_rigidbody != null)
        {
            _rigidbody.simulated = true;
            //_rigidbody.gravityScale = Data != null ? Data.gravityScale : 1f;
            _rigidbody.gravityScale = initialGravityScale;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }

        //  Collider 활성화
        if (Collider != null)
        {
            Collider.enabled = true;
        }

        enabled = true;

        //  애니메이터 활성화
        if (_animator != null)
        {
            _animator.enabled = true;
        }

        //  StatManager 초기화
        if (Data == null)
        {
            // 데이터가 없다면 테이블에서 로드
            var table = TableManager.Instance.GetTable<BaseEnemyTable>();

            if (table != null)
            {
                Data = table.GetDataByID(_id);
            }
        }

        if (StatManager != null && Data != null)
        {
            StatManager.Initialize(Data, this);
            AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
        }

        //  Combat 뭐시기 초기화
        InitializeCombatHandlers();

        //  애니메이션 트리거 초기화
        if (_animationHandler != null)
        {
            _animationHandler.ResetTriggers();
        }

        //  상태 머신 재진입
        StartCoroutine(DeferEnter());
    }

    private IEnumerator DeferEnter()
    {
        yield return null;

        ChangeState(BaseEnemyState.Idle);

        yield return null;

        ChangeState(BaseEnemyState.Chasing);
    }
    #endregion

    #region 공격 관련
    public virtual void ExecuteMeleeHit() { }

    #endregion

    #region 테두리 관련 메서드
    public void ShowOutline()
    {
        Outline?.UpdateOutline(true);
    }

    public void HideOutline()
    {
        Outline?.UpdateOutline(false);
    }
    #endregion

    #region 낙사 감지 메서드
    private void CheckFallDeath()
    {
        if (!IsDead && transform.position.y < _deadYThreshold)
        {
            ChangeState(BaseEnemyState.Die);
        }
    }
    #endregion
}
