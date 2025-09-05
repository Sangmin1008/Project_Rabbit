using System;
using PlayerGroundStates;
using PlayerAirStates;
using PlayerAttackStates;
using PlayerActionStates;
using PlayerHitStates;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttackHandler))]
[RequireComponent(typeof(PlayerActionHandler))]
[RequireComponent(typeof(PlayerHitHandler))]
[RequireComponent(typeof(StaminaManager))]
[RequireComponent(typeof(GroundDetector))]
[RequireComponent(typeof(AfterimageController))]
public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
{
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private VoidEventChannelSO onStartLoadTitleScene;

    private Rigidbody2D _rigidbody2D;
    private PlayerInputController _playerInputController;
    
    private bool _isDead = false;
    private bool _isParryOrDodgeSuccessful = false;

    public Vector2 MoveInput => PlayerMovement.MoveInput;
    public bool IsGrounded => GroundDetector.IsGrounded;
    public bool IsFlipX => transform.localScale.x < 0f;
    public float VelocityY => _rigidbody2D.linearVelocityY;
    public bool JumpTriggered { get => PlayerMovement.JumpTriggered; set => PlayerMovement.JumpTriggered = value; }
    public bool CanDoubleJump { get => PlayerMovement.CanDoubleJump; set => PlayerMovement.CanDoubleJump = value; }
    public bool DashTriggered { get => PlayerActionHandler.DashTriggered; set => PlayerActionHandler.DashTriggered = value; }
    public bool IsDashing { get => PlayerActionHandler.IsDashing; set => PlayerActionHandler.IsDashing = value; }
    public bool IsDefensing { get => PlayerActionHandler.IsDefensing; set => PlayerActionHandler.IsDefensing = value; }
    public bool CanDash { get; set; } = true;
    public bool CanDefense { get; set; } = true;
    public bool CanAttack { get; set; } = true;
    public bool ComboAttackTriggered { get => PlayerAttackHandler.GroundAttackTriggered; set => PlayerAttackHandler.GroundAttackTriggered = value; }
    public bool IsComboAttacking { get => PlayerAttackHandler.IsGroundAttacking; set => PlayerAttackHandler.IsGroundAttacking = value; }
    public bool AirAttackTriggered { get => PlayerAttackHandler.AirAttackTriggered; set => PlayerAttackHandler.AirAttackTriggered = value; }
    public bool IsAirAttacking { get => PlayerAttackHandler.IsAirAttacking; set => PlayerAttackHandler.IsAirAttacking = value; }
    public bool AttackTriggered { get => PlayerAttackHandler.AttackTriggered; set => PlayerAttackHandler.AttackTriggered = value; }
    public bool IsAttacking { get => PlayerAttackHandler.IsAttacking; set => PlayerAttackHandler.IsAttacking = value; }
    public bool AttackPerformed { get => PlayerAttackHandler.AttackPerformed; set => PlayerAttackHandler.AttackPerformed = value; }
    public bool CanStrongAttack => PlayerAttackHandler.CanStrongAttack;
    public bool IsParryingOrDodging { get; set; } = false;
    public bool IsParryOrDodgeSuccessful { get => _isParryOrDodgeSuccessful; set => _isParryOrDodgeSuccessful = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public bool TookDamage { get; set; } = false;
    public bool IsInvincible { get; set; } = false;
    public bool IsMovementLocked { get; set; } = false;
    public bool GameIsPause => UIManager.Instance.GameIsPause;
    public AttackType ReceivedAttackType { get; set; }
    public StatBase AttackStat { get; set; }
    public IDamageable Target { get; private set; }
    public Collider2D Collider { get; private set; }
    public PlayerAnimation PlayerAnimation { get => playerAnimation; private set => playerAnimation = value; }
    public Rigidbody2D Rigidbody2D { get => _rigidbody2D; set => _rigidbody2D = value; }
    public SpriteRenderer SpriteRenderer { get; set; }
    public PlayerMovement PlayerMovement { get; private set; }
    public PlayerAttackHandler PlayerAttackHandler { get; private set; }
    public PlayerActionHandler PlayerActionHandler { get; private set; }
    public PlayerHitHandler PlayerHitHandler { get; private set; }
    public StaminaManager StaminaManager { get; private set; }
    public GroundDetector GroundDetector { get; private set; }
    public AfterimageController AfterimageController { get; private set; }
    public AttackType AttackType => PlayerAttackHandler.CurrentAttackInfoData.AttackType;
    public EffectController DashEffect;
    public EffectController LandingEffect;
    public EffectController JumpEffect;
    public EffectController ParryEffect;

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
        InitializePlayerData();
    }

    protected override void Start()
    {
        base.Start();
        BindInput();
        InitializeAttackStat();
        StatManager.SetMaxValue(StatType.CurHp);
        StatManager.SetMaxValue(StatType.CurStamina);
        StartCoroutine(StaminaManager.RecoverStaminaCoroutine());
        PlayerUIEvents.OnPlayerStatUIUpdate.Invoke();
    }

    protected override void Update()
    {
        base.Update();
        
        if (IsComboAttacking || IsAirAttacking || IsDashing || IsDefensing || IsMovementLocked || IsDead) return;
        Rotate();
    }
    
    private void FixedUpdate()
    {
        if (IsDashing) return;
        if (!CanDash) DashTriggered = false;
        if (!IsComboAttacking && !IsDefensing && !IsMovementLocked && !IsDead) Movement();
        if (!IsGrounded) Fall();
    }

    protected override IState<PlayerController, PlayerState> GetState(PlayerState state)
    {
        return state switch
        {
            PlayerState.Idle => new IdleState(),
            PlayerState.Move => new MoveState(),

            PlayerState.Jump => new JumpState(),
            PlayerState.Fall => new FallState(),
            PlayerState.DoubleJump => new DoubleJumpState(),
            
            PlayerState.ComboAttack => new ComboAttackState(),
            PlayerState.AirAttack => new AirAttackState(),
            PlayerState.StrongAttack => new StrongAttackState(),
            
            PlayerState.Defense => new DefenseState(),
            PlayerState.Dash => new DashState(),
            
            PlayerState.NormalHit => new NormalHitState(),
            PlayerState.StrongHit  => new StrongHitState(),
            PlayerState.NormalKnockback => new NormalKnockbackState(),
            PlayerState.StrongKnockback  => new StrongKnockbackState(),
            
            PlayerState.Dead => new PlayerDeadState(),
            _ => null
        };
    }
    
    private void InitializeComponents()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerInputController = GetComponent<PlayerInputController>();
        
        playerAnimation = GetComponent<PlayerAnimation>();
        
        Collider = GetComponent<Collider2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerAttackHandler = GetComponent<PlayerAttackHandler>();
        PlayerActionHandler = GetComponent<PlayerActionHandler>();
        PlayerHitHandler = GetComponent<PlayerHitHandler>();
        StaminaManager = GetComponent<StaminaManager>();
        GroundDetector =  GetComponent<GroundDetector>();
        AfterimageController = GetComponent<AfterimageController>();
        
        GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
    }

    private void InitializePlayerData()
    {
        PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
        PlayerSO playerData = playerTable.GetDataByID(0);
        StatManager.Initialize(playerData, this);
    }

    private void BindInput()
    {
        var action = _playerInputController.PlayerActions;
        action.Move.performed += context =>
        {
            if (GameIsPause) return;
            PlayerMovement.MoveInput = context.ReadValue<Vector2>();
        };
        action.Move.canceled += _ => PlayerMovement.MoveInput = Vector2.zero;
        
        action.Jump.started += _ =>
        {
            if (GameIsPause) return;
            JumpTriggered = true;
        };

        action.Attack.started += _ =>
        {
            if (GameIsPause) return;
            if (CanAttack)
            {
                AttackTriggered = true;
            }
        };
        action.Attack.performed += _ =>
        {
            if (GameIsPause) return;
            AttackPerformed = true;
        };
        action.Attack.canceled += _ =>
        {
            if (GameIsPause) return;
            AttackPerformed = false;
        };
        
        action.Defense.performed += _ =>
        {
            if (GameIsPause) return;
            if (StatManager.GetValue(StatType.CurStamina) >= StaminaManager.DefenseStaminaCost)
                IsDefensing = true;
        };
        action.Defense.canceled += _ => IsDefensing = false;
        
        action.Dash.started += _ =>
        {
            if (GameIsPause) return;
            if (StatManager.GetValue(StatType.CurStamina) >= StaminaManager.DashStaminaCost)
                DashTriggered = true;
        }; 
    }

    private void InitializeAttackStat()
    {
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    public override void Movement() => PlayerMovement.Movement();
    public void Rotate() => PlayerMovement.Rotate();
    private void Fall() => PlayerMovement.Fall();
    public void Jump() => PlayerMovement.Jump();
    public void StopMoving() => PlayerMovement.StopMove();
    public void Attack() => PlayerAttackHandler.AttackAllTargets();
    public override void FindTarget() => PlayerAttackHandler.FindTarget();
    public void Parry() => StartCoroutine(PlayerActionHandler.Parry());
    public bool TryParrying() => PlayerActionHandler.TryParrying();
    public void Dodge() =>  PlayerActionHandler.Dodge();
    public bool TryDodging() => PlayerActionHandler.TryDodging();
    public void TakeDamage(IAttackable attacker) => PlayerHitHandler.TakeDamage(attacker);

    public void Dead()
    {
        //Debug.Log("플레이어 죽음!!");
        _isDead = true;
        onStartLoadTitleScene.Raise();
    }
}