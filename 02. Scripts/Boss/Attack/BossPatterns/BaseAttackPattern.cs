using UnityEngine;
using System.Collections;

public class BaseAttackPattern
{
    #region 패턴 실행

    //  전체 공격 시퀀스
    public IEnumerator Execute(PatternContext context)
    {
        context.Boss.NotifyPatternStart();

        if (context.Unit.IsDead)
        {
            yield break;
        }

        HitData info = context.Boss.Data.basicHitInfo;
        float phaseMul = context.Boss.IsPhase2 ? 1.5f : 1.0f;

        // Windup (공격 준비)
        yield return new WaitForSeconds(info.WindupTime);

        // 애니메이션 실행 (AttackHandler에서 PlayBasicAttack 호출)
        context.Unit.PlayBasicAttack();


        // 투사체 비행시간 등 연출
        yield return new WaitForSeconds(info.ActiveTime);

        // 게이지 증가
        if (context.Unit is IHasGauge gaugeUnit)
        {
            gaugeUnit.AddBasicGauge();
        }

        // 후딜
        yield return new WaitForSeconds(context.Boss.Data.basicAttackPostDelay);

        context.Boss.NotifyPatternEnd();
    }

    #endregion

    #region Animation Event에서 사용할 메서드

    //  Animation Event에서 실제 공격 타이밍에 호출
    public void Fire(PatternContext context)
    {
        HitData info = context.Boss.Data.basicHitInfo;
        float phaseMul = context.Boss.IsPhase2 ? 1.5f : 1.0f;

        Vector2 startPos = context.FirePoint.position;
        Vector2 targetPos = (context.Target != null) ? context.Target.Collider.bounds.center : startPos;
        Vector2 direction = (targetPos - startPos).normalized;

        GameObject bullet = context.ProjectilePool.Get(ProjectileType.Bullet);
        bullet.transform.position = context.FirePoint.position;
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(true);

        if (bullet.TryGetComponent<BossProjectile>(out var proj))
        {
            proj.SetupPool(context.ProjectilePool);
            proj.Initialize(direction, context.Boss, info.ProjectileSpeed * phaseMul);
            proj.Damage = (int)(info.Damage * phaseMul);
            proj.AttackType = info.Type;
        }
    }
    #endregion

}
