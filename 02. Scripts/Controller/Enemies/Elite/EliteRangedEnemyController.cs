using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EliteRangedEnemyController : BaseEnemyController
{
    #region Runtime 레퍼런스들
    private ProjectilePool _projectilePool;
    #endregion

    #region IUnitController Overrides
    public override Transform HitBoxSpawnPoint => null;

    public override Transform FirePoint => _firePoint;

    public override float MinAttackDistance => HitData.Range;
    #endregion

    #region 유니티 콜백
    protected override void Awake()
    {
        base.Awake();

        if (_firePoint == null)
        {
            var t = transform.Find("FirePoint");

            if (t != null)
            {
                _firePoint = t;
            }

            else
            {
                Debug.LogError($"{gameObject.name}: _firePoint가 할당되어 있지 않고, 'FirePoint' 자식도 없습니다!");
            }
        }

    }
    #endregion

    #region Attack 정의들
    public override AttackType AttackType => HitData.Type;

    public override float AttackRange => HitData.Range;

    protected override HitData HitData => Data.RangedHitData;
    #endregion

    #region Combat Initialization
    protected override void InitializeCombatHandlers()
    {
        //  Pool과 데미지 리시버 셋업
        _projectilePool = Object.FindFirstObjectByType<ProjectilePool>();

        _damageReceiver = new DamageReceiver(StatManager, this, OnDeathRoutine(), this);
    }
    #endregion

    #region Attack 로직
    public override void Attack()
    {
        ShowOutline();

        StartCoroutine(DoRangedAttackRoutine());
    }

    private IEnumerator DoRangedAttackRoutine()
    {
        IsAttacking = true;

        //  준비 시간
        yield return new WaitForSeconds(HitData.WindupTime);

        //  연속 사격
        for (int i = 0; i < HitData.shotCount; i++)
        {
            PlayAttackSound();

            _animator.SetTrigger("BasicAttack");

            var projectileObj = _projectilePool.Get(ProjectileType.Missile);

            //  위치 리셋
            projectileObj.transform.SetParent(null, worldPositionStays: true);
            projectileObj.transform.position = _firePoint.position;

            var projectile = projectileObj.GetComponent<EliteProjectile>();
            projectile.SetPool(_projectilePool, ProjectileType.Missile);

            Vector2 spawnPos = _firePoint.position;
            Vector2 targetPos = Target.Collider.bounds.center;
            Vector2 direction = (targetPos - spawnPos).normalized;

            projectile.Initialize(direction, HitData.ProjectileSpeed, HitData.Damage, this);

            yield return new WaitForSeconds(HitData.ShotInterval);
        }

        //  Active duration 대기
        yield return new WaitForSeconds(HitData.ActiveTime);

        HideOutline();

        //  FSM용 플래그 내리기
        IsAttacking = false;
    }


    #endregion

    #region Death Callback
    private IEnumerator OnDeathRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        ChangeState(BaseEnemyState.Die);
    }
    #endregion
}
