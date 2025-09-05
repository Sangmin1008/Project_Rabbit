using UnityEngine;
using System.Collections;
using BossStates;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(TargetingHandler))]
[RequireComponent(typeof(PhaseManager))]
[RequireComponent(typeof(BossPatternManager))]
[RequireComponent(typeof(AttackHandler))]

public class BossController : BaseController<BossController, BossState>, IUnitController, IHasGauge
{
    #region 인스펙터 세팅 필드
    [Header("테이블 ID")]
    [SerializeField] private int _id = 0;

    [Header("투사체 풀 & 발사지점")]
    [SerializeField] private ProjectilePool _projectilePool;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _tntFirePoint;

    [Header("핸들러 주입 (Inspector Drag & Drop)")]
    [SerializeField] private BossStatHandler _statHandler;
    [SerializeField] private BossVisualHandler _visualHandler;
    [SerializeField] private BossAudioHandler _audioHandler;      //  사운드 파일 확정나면 추가할 것!
    [SerializeField] private BossEffectHandler _effectHandler;
    [SerializeField] private TargetingHandler _targetHandler;
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private AttackHandler _attackHandler;
    [SerializeField] private BossPatternManager _patternManager;

    [Header("Room Bounds (for Evade)")]
    [Tooltip("보스가 퇴각할 때 맵 밖으로 안 나가게 할 X 최소값")]
    [SerializeField] private float roomMinX;
    [Tooltip("보스가 퇴각할 때 맵 밖으로 안 나가게 할 X 최대값")]
    [SerializeField] private float roomMaxX;

    #endregion

    #region 내부 컴포넌트 & 핸들러
    private Rigidbody2D _rigidbody;
    private StatBase _attackStat;
    private BossSO _data;

    private FacingHandler _facingHandler;
    private MovementHandler _movementHandler;
    private AnimationHandler _animationHandler;
    private PatternCooldownManager _cooldownManager;

    private bool _isDead = false;

    #endregion

    #region 이벤트 선언
    public event Action OnDamaged;  //  피격
    public event Action OnDeath;    //  사망
    public event Action OnPhase2;   //  2 페이즈 진입

    #endregion

    #region 프로퍼티
    public BossSO Data
    {
        get { return _data; }
        private set { _data = value; }
    }

    public BossStatHandler StatHandler
    {
        get { return _statHandler; }
    }

    //  IUnitController 프로퍼티
    public Collider2D Collider => GetComponent<Collider2D>();

    public Rigidbody2D Rigidbody
    {
        get { return _rigidbody; }
    }

    public StatBase AttackStat
    {
        get { return _attackStat; }
    }

    public float AttackRange
    {
        get { return StatManager.GetValueSafe(StatType.AttackRange, 1f); }
    }

    public float DetectingRange
    {
        get { return _data != null ? _data.detectingRange : 0f; }
    }

    public AttackType AttackType
    {
        get
        {
            if (_effectHandler != null && _effectHandler.ChargeHitboxActive)
            {
                return CurrentPattern.hitInfo.Type;
            }

            return _data.basicHitInfo.Type;
        }
    }

    public IDamageable Target
    {
        get { return _targetHandler.Current; }
    }

    public Transform FirePoint
    {
        get { return _firePoint; }
    }

    public float MinAttackDistance
    {
        get { return _data.basicHitInfo.Range; }
    }

    public bool IsDead
    {
        get { return _statHandler.IsDead; }
    }

    //  페이즈 관리

    public bool IsPhase2                        //  페이즈 진입 여부
    {
        get { return _phaseManager.IsPhase2; }
    }

    //  패턴 관리
    public int CurrentPatternIndex
    {
        get { return _patternManager.CurrentPatternIndex; }
    }

    public SkillSO CurrentPattern
    {
        get
        {
            if (_data == null || _data.patterns == null || _data.patterns.Length == 0)
            {
                Debug.LogError("[BossController] 패턴 데이터 없음!");
                return null;
            }

            if (CurrentPatternIndex < 0 || CurrentPatternIndex >= _data.patterns.Length)
            {
                Debug.LogError($"[BossController] 패턴 인덱스 범위 초과! (index={CurrentPatternIndex}, length={_data.patterns.Length})");
                return null;
            }

            return _data.patterns[CurrentPatternIndex];
        }
    }

    //  보조 오브젝트 참조
    public Transform TntFirePoint
    {
        get { return _tntFirePoint; }
    }

    public BossState CurrentStatePublic
    {
        get { return base.CurrentState; }
    }

    //  Phase2 공중 포격이 이미 실행됐는지 체크
    public bool ShellingDone { get; private set; }

    //  패턴 수행 중인지를 나타내는 플래그
    public bool IsPerformingPattern { get; private set; }

    public Coroutine CurrentPatternCoroutine { get; set; }

    public float RoomMinX
    {
        get { return roomMinX; }
    }

    public float RoomMaxX
    {
        get { return roomMaxX; }
    }
 
    #endregion

    #region IUnitController 구현
    public void SetSpeed(float speed)
    {
        _animationHandler.SetSpeed(speed);
    }

    public void PlayBasicAttack()
    {
        _animationHandler.PlayBasicAttack();
    }

    public void PlayPattern(int idx)
    {
        _animationHandler.PlayPattern(idx);
    }

    public void PlayDeath()
    {
        _animationHandler.PlayDeath();
    }

    public bool TNTHasFired { get; set; }
    #endregion

    #region IHasGauge 구현 & 게이지 관련
    public void AddBasicGauge()
    {
        _statHandler.AddGauge();
    }

    public bool TryConsumeGaugeForPattern()
    {
        return _statHandler.TryConsumeGauge();
    }

    public void ResetBasicGauge()
    {
        _statHandler.ResetGauge();
    }

    #endregion

    #region 유니티 콜백
    protected override void Awake()
    {
        base.Awake();

        //  컴포넌트 캐싱
        _rigidbody = GetComponent<Rigidbody2D>();
        _animationHandler = new AnimationHandler(GetComponentInChildren<Animator>(), this);
        _movementHandler = new MovementHandler(_rigidbody, this);

        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        var flipCollider = GetComponent<ColliderFlipHandler>();

        _facingHandler = new FacingHandler(spriteRenderer, flipCollider, _firePoint, _tntFirePoint);

        var animator = GetComponentInChildren<Animator>();

        //  AttackHandler는 RequireComponent로 붙어있으니 GetComponent로
        _attackHandler = GetComponent<AttackHandler>();
        _attackHandler.Initialize(this, animator, this, _projectilePool, _firePoint);

        _cooldownManager = new PatternCooldownManager();
        _cooldownManager.OnCooldownEnded += HandleCooldownEnded;
    }

    protected override void Start()
    {
        // 테이블 로드
        _data = TableManager.Instance.GetTable<BossTable>().GetDataByID(_id);

        // 매니저 초기화
        _patternManager.Initialize(_data);

        _statHandler.Initialize(GetComponent<StatManager>(), _data, this, OnDeathSequence(), this);

        _attackStat = _statHandler.StatManager.GetStat<CalculatedStat>(StatType.AttackPow);

        base.Start(); // 상태 머신 진입
    }

    protected override void Update()
    {
        if (_isDead)
        {
            return;
        }

        //  타겟 갱신
        _targetHandler.Refresh();

        //  페이즈 체크, 패턴 후딜 등
        _phaseManager.CheckPhase();

        _cooldownManager.Update(Time.deltaTime);

        //  패턴 수행 중이거나 쿨다운 중이면 FSM 갱신 건너뛰기
        if (IsPerformingPattern || _cooldownManager.IsCoolingDown)
        {
            return;
        }

        base.Update(); // FSM 갱신
    }

    private void OnDestroy()
    {
        //  이벤트 구독 해제
        if (_cooldownManager != null)
        {
            _cooldownManager.OnCooldownEnded -= HandleCooldownEnded;
        }
    }

    #endregion

    #region 이벤트 핸들러
    private void HandleCooldownEnded()
    {
        //  쿨다운 끝나면 추격 상태로 복귀
        RequestStateChange(BossState.Chasing);
    }

    #endregion

    #region FSM 상태 매핑
    protected override IState<BossController, BossState> GetState(BossState state)
    {
        // 항상 state로부터 인덱스 역산 (절대 CurrentPatternIndex 사용 X)
        int index = CurrentPatternIndex;
        SkillSO so = Data.patterns.Length > index && index >= 0 ? Data.patterns[index] : null;

        return state switch
        {
            BossState.Idle => new IdleState(),
            BossState.Chasing => new ChasingState(),
            BossState.Attack => new AttackState(),

            BossState.Pattern1 => new PatternFirebombState(),
            BossState.Pattern2 => new PatternShootState(),
            BossState.Pattern3 => new PatternChargeState(),
            BossState.Pattern4 => new PatternSummonState(),
            BossState.Pattern5 => new PatternShellingState(),

            BossState.Evade => new EvadeState(_data.evadeDistance, _data.evadeDuration, _data.evadeChance),
            BossState.Die => new DieState(),
            _ => new IdleState(),
        };
    }

    public void RequestStateChange(BossState next)
    {
        //  Phase2에 진입했고, 아직 Shelling이 실행되지 않았다면
        if (_phaseManager.IsPhase2 && !ShellingDone)
        {
            next = BossState.Pattern5;
            ShellingDone = true;
        }

        ChangeState(next);
    }
    #endregion

    #region IAttackable 구현
    public void Attack()
    {
        StartCoroutine(_attackHandler.BasicAttack());
    }
    #endregion

    #region IDamageable 구현
    public void TakeDamage(IAttackable attacker)
    {
        if (_statHandler.IsDead)
        {
            return;
        }

        _statHandler.TakeDamage(attacker);

        _audioHandler?.PlayHit();

        //  피격 이벤트 발행
        OnDamaged?.Invoke();

        if (_statHandler.IsDead)
        {
            //  사망 이벤트
            OnDeath?.Invoke();

            RequestStateChange(BossState.Die);

            return;
        }

        //  피격 후 뒤로 물러나는 확률 검사
        if (UnityEngine.Random.value < Data.evadeChance)
        {
            RequestStateChange(BossState.Evade);
        }
    }
    #endregion

    #region 사망 시퀀스
    private IEnumerator OnDeathSequence()
    {
        yield return new WaitForSeconds(0.5f);

        RequestStateChange(BossState.Die);
    }

    public void Dead()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        _audioHandler?.PlayDeath();

        PlayDeath();
        Destroy(gameObject, 1f);
    }
    #endregion

    #region 타겟 탐색 및 이동
    public override void Movement()
    {
        //  실제 추격 로직
        _movementHandler.Chase();

        _animationHandler.SetSpeed(Mathf.Abs(_rigidbody.linearVelocity.x));
    }
    #endregion

    #region 위치 잠금 & 페이싱 메서드
    public void LockPosition(bool freezeX, bool freezeY)
    {
        var cons = RigidbodyConstraints2D.FreezeRotation;

        if (freezeX)
        {
            cons |= RigidbodyConstraints2D.FreezePositionX;
        }

        if (freezeY)
        {
            cons |= RigidbodyConstraints2D.FreezePositionY;
        }

        _rigidbody.constraints = cons;
    }

    public void FaceToTarget()
    {
        _facingHandler.FaceToTarget(Target);
    }

    #endregion

    #region 패턴 후 쿨다운 관리
    //  패턴 시작 시
    public void NotifyPatternStart()
    {
        IsPerformingPattern = true;
    }

    //  패턴 종료 시
    public void NotifyPatternEnd()
    {
        IsPerformingPattern = false;

        float postCooldown = CurrentPattern.delay;

        _cooldownManager.StartCooldown(postCooldown);
    }

    #endregion

    #region 페이즈 관련
    public void OnEnterPhase2()
    {
        //  BossPatternManager에게 페이즈 2 진입 알리기
        _patternManager.NotifyPhase2Pending();

        RequestStateChange(BossState.Pattern5);
    }

    #endregion
}