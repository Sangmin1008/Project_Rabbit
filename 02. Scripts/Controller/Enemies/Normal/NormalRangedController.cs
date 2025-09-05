using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider2D))]
public class NormalRangedController : BaseEnemyController
{
    #region Runtime 레퍼런스
    private ProjectilePool _projectilePool;
    #endregion

    #region IUnitController 구현
    public override Transform HitBoxSpawnPoint
    { 
        get { return null; } 
    }

    public override Transform FirePoint
    { 
        get { return _firePoint; } 
    }

    public override float MinAttackDistance
    {
        get { return HitData.Range; }
    }

    #endregion

    #region Attack 정의들
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
        get { return Data.RangedHitData; }
    }

    #endregion

    #region 테두리 추가 유지 시간
    [Header("테두리 추가 유지 시간")]
    [Tooltip("발사 후에도 추가로 얼마나(초) 테두리를 유지할지")]
    [SerializeField] private float _outlineExtraKeep;
    #endregion

    #region 유니티 콜백
    protected override void Awake()
    {
        base.Awake();

        if (_firePoint == null)
        {
            var firePointTransform = transform.Find("FirePoint");

            if (firePointTransform != null)
            {
                _firePoint = firePointTransform;
            }

            else
            {
                Debug.LogError($"{gameObject.name}: _firePoint가 할당되어 있지 않고, 'FirePoint' 자식도 없음!");
            }
        }
    }

    protected override void Start()
    {
        base.Start();

        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);

        //  풀 캐싱
        _projectilePool = Object.FindFirstObjectByType<ProjectilePool>();

        if (_projectilePool == null)
        {
            Debug.LogError("ProjectilePool이 씬에 존재하지 않음!");
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

        PlayBasicAttack(); // 애니메이션 재생 → 해당 애니메이션에서 FireProjectile 이벤트 호출

        // 공격 끝나는 시점에 플래그를 내려주는 코루틴 (애니메이션 길이 활용)
        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(HitData.ActiveTime);

        HideOutline();

        IsAttacking = false;
    }

    // FireProjectile 이벤트 함수: 이 함수에서 총알을 발사!
    public void FireProjectile()
    {
        ShowOutline();

        PlayAttackSound();

        if (_projectilePool == null)
        {
            _projectilePool = Object.FindFirstObjectByType<ProjectilePool>();

            if (_projectilePool == null)
            {
                Debug.LogError("ProjectilePool이 씬에 존재하지 않습니다!");
                return;
            }
        }

        if (_firePoint == null || Target == null)
        {
            return;
        }

        var projectileObj = _projectilePool.Get(ProjectileType.Bullet);
        projectileObj.transform.SetParent(null, true);
        projectileObj.transform.position = _firePoint.position;

        var projectile = projectileObj.GetComponent<BossProjectile>();
        projectile.SetupPool(_projectilePool);

        Vector2 spawnPos = _firePoint.position;
        Vector2 targetPos = Target.Collider.bounds.center;
        Vector2 direction = (targetPos - spawnPos).normalized;

        projectile.Initialize(direction, this, HitData.ProjectileSpeed);
        projectile.Damage = (int)HitData.Damage;
        projectile.AttackType = HitData.Type;

        StartCoroutine(DisableOutlineAfterDelay());
    }

    private IEnumerator DisableOutlineAfterDelay()
    {
        //  HitData.ActiveTime만큼 기다리기

        float waitTime = HitData.ActiveTime + _outlineExtraKeep;

        yield return new WaitForSeconds(waitTime);

        HideOutline();
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
