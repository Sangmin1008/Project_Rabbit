using UnityEngine;
using System;
using System.Collections.Generic;

//[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]

public class EnemyController : BaseController<EnemyController, EnemyState>, IAttackable, IDamageable
{
    // 정적 이벤트 - 어떤 적이든 죽었을 때 발생
    public static event System.Action<EnemyController> OnAnyEnemyDie;
    private CharacterController _characterController;
    private IDamageable _target;
    private bool _isDead;

    public bool IsDead => _isDead;
    public Collider2D Collider { get; private set; }
    public StatBase AttackStat { get; private set; }
    public IDamageable Target => _target;

    [SerializeField] private EnemySO _data;
    public EnemySO Data => _data;

    [Header("오브젝트 풀 설정")]
    public string poolName = ""; // 어떤 풀에서 나왔는지 저장

    [Header("공격 설정")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;
    public float minAttackDistance = 3f;
    
    [Header("죽음 이펙트")]
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;
    public GameObject hitEffectPrefab;
    
    // 컴포넌트
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    // 상태 변수
    public bool IsAttacking { get; set; }
    public bool CanAttack { get; set; } = true;
    public float LastAttackTime { get; set; }

    //  이 부분 수정 필요합니다!
    public AttackType AttackType => AttackType.None;

    // 원본 크기 저장
    private Vector3 originalScale;

    protected override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<Collider2D>();
        
        // 원본 크기 저장
        originalScale = transform.localScale;
        
        StatManager.Initialize(Data, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        FindTarget();
        UpdateSpriteDirection();
    }

    protected override IState<EnemyController, EnemyState> GetState(EnemyState state) => state switch
    {
        EnemyState.Idle => new EnemyStates.IdleState(),
        EnemyState.Chasing => new EnemyStates.ChasingState(),
        EnemyState.Attack => new EnemyStates.AttackState(
            StatManager.GetValueSafe(StatType.AttackSpd, 1.0f), // 기본 공격 속도 1.0
            StatManager.GetValueSafe(StatType.AttackRange, 3.0f)), // 기본 공격 범위 3.0
        EnemyState.Die => new EnemyStates.DieState(),
        _ => null
    };

    public override void Movement()
    {
        base.Movement();    

        if (_target == null)
        {
            return;
        }

        float speed = StatManager.GetValueSafe(StatType.MoveSpeed, 5f);
        Vector3 dir = (_target.Collider.transform.position - transform.position).normalized;

        if (_characterController != null && _characterController.enabled)
        {
            _characterController.Move(dir * speed * Time.deltaTime);
        }
        else if (_rigidbody2D != null)
        {
            // X축만 이동, Y축은 중력에 맡김
            _rigidbody2D.linearVelocity = new Vector2(dir.x * speed, _rigidbody2D.linearVelocity.y);
        }
    }

    public void MovementWithDistance(float minDistance)
    {
        if (_target == null) return;

        float speed = StatManager.GetValueSafe(StatType.MoveSpeed, 5f);
        float distanceToTarget = Vector3.Distance(transform.position, _target.Collider.transform.position);
        
        Vector3 dir;
        
        // 너무 가까우면 뒤로 이동
        if (distanceToTarget < minDistance)
        {
            dir = (transform.position - _target.Collider.transform.position).normalized;
        }
        // 적정 거리보다 멀면 접근
        else if (distanceToTarget > StatManager.GetValueSafe(StatType.AttackRange, 3.0f) * 0.8f)
        {
            dir = (_target.Collider.transform.position - transform.position).normalized;
        }
        // 적정 거리면 정지
        else
        {
            if (_rigidbody2D != null)
            {
                // X축만 정지, Y축은 중력에 맡김
                _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocity.y);
            }
            return;
        }

        if (_characterController != null && _characterController.enabled)
        {
            _characterController.Move(dir * speed * Time.deltaTime);
        }
        else if (_rigidbody2D != null)
        {
            // X축만 이동, Y축은 중력에 맡김
            _rigidbody2D.linearVelocity = new Vector2(dir.x * speed, _rigidbody2D.linearVelocity.y);
        }
    }

    public void Attack()
    {
        _target?.TakeDamage(this);
    }

    // Melee 공격을 위한 타겟 설정 메서드
    public void SetAttackTarget(IDamageable target)
    {
        _target = target;
    }

    public override void FindTarget()
    {
        if (_target != null && !_target.IsDead)
        {
            return;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            _target = playerObj.GetComponent<IDamageable>();
            if (_target == null)
            {
                // PlayerController가 IDamageable을 구현하지 않은 경우 처리
                Debug.LogWarning("Player doesn't implement IDamageable interface");
            }
        }
    }

    private void UpdateSpriteDirection()
    {
        if (_target == null || _spriteRenderer == null) return;
        
        // 타겟이 왼쪽에 있으면 왼쪽을 봄
        if (_target.Collider.transform.position.x < transform.position.x)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }
    }

    public void Dead()
    {
        if (_isDead) return;
        
        _isDead = true;
        
        // 정적 이벤트 발생 - 다른 시스템들에게 이 적이 죽었음을 알림
        OnAnyEnemyDie?.Invoke(this);
        
        // 상태를 Die로 변경
        ChangeState(EnemyState.Die);
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead) return;
        
        float damage = attacker.AttackStat?.Value ?? 0;
        
        // HP 감소 (데미지 처리)
        StatManager.Consume(StatType.CurHp, StatModifierType.Base, damage);
        
        // 피격 효과
        StartCoroutine(HitEffect());
        
        // HP 체크
        float currentHealth = StatManager.GetValueSafe(StatType.CurHp, 0f);
        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    private System.Collections.IEnumerator HitEffect()
    {
        if (_spriteRenderer != null)
        {
            // 피격 이펙트 생성
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // 색상 변경 효과
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (_spriteRenderer != null) // null 체크 (죽음 중일 수 있음)
            {
                _spriteRenderer.color = Color.white;
            }
        }
    }

    // 오브젝트 풀에서 사용될 때 초기화
    private void OnEnable()
    {
        // 첫 번째 활성화인지 확인 (Start가 아직 호출되지 않았으면 스킵)
        if (StatManager == null) return;
        
        // 원본 크기로 복원
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
        
        // 타겟 재탐색
        _target = null;
        FindTarget();
        
        // 상태 변수 초기화
        IsAttacking = false;
        CanAttack = true;
        LastAttackTime = 0f;
        
        // 죽음 상태 초기화 
        _isDead = false;
    }

    // 공격 범위 시각화 (에디터용)
    private void OnDrawGizmosSelected()
    {
        // 근거리 공격 범위 (MeleeMonster가 있을 때만)
        if (GetComponent<MeleeMonster>() != null && attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        
        // 감지 범위
        if (Data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Data.DetectionRange);
        }
    }

    // 풀 이름 찾기 헬퍼 메서드 (public으로 변경)
    public string GetPoolName()
    {
        // 1. EnemyController에 저장된 poolName 사용 (가장 정확함)
        if (!string.IsNullOrEmpty(poolName))
        {
            //Debug.Log($"Using stored poolName: {poolName}");
            return poolName;
        }
        
        // 2. 게임오브젝트 이름에서 풀 이름 추출 (폴백)
        // 예: "Goblin_Melee_MeleeMonsterPool" -> "MeleeMonsterPool"
        string objectName = gameObject.name;
        
        // 마지막 언더스코어 이후의 문자열 찾기
        int lastUnderscore = objectName.LastIndexOf('_');
        if (lastUnderscore >= 0 && lastUnderscore < objectName.Length - 1)
        {
            string possiblePoolName = objectName.Substring(lastUnderscore + 1);
            
            // 풀에 해당 이름이 있는지 확인
            if (MonsterObjectPool.Instance != null)
            {
                var poolInfo = MonsterObjectPool.Instance.poolInfos.Find(p => p.poolName == possiblePoolName);
                if (poolInfo != null)
                {
                    return possiblePoolName;
                }
            }
        }
        
        // 3. 몬스터 타입으로 풀 이름 추정
        if (Data != null)
        {
            string poolNameByType = Data.Type.ToString() + "MonsterPool";
            var poolInfo = MonsterObjectPool.Instance.poolInfos.Find(p => p.poolName == poolNameByType);
            if (poolInfo != null)
            {
                return poolNameByType;
            }
        }
        
        // 4. 컴포넌트 기반으로 풀 이름 찾기
        if (GetComponent<MeleeMonster>() != null)
            return "MeleeMonsterPool";
        else if (GetComponent<RangedMonster>() != null)
            return "RangedMonsterPool";
        else if (GetComponent<HomingMonster>() != null)
            return "HomingMonsterPool";
        else if (GetComponent<MultiShotMonster>() != null)
            return "MultiShotMonsterPool";
        
        return null;
    }
    
    // 원본 크기 가져오기
    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }
}
