using UnityEngine;
using System.Collections;
using NUnit.Framework.Constraints;

public class ShootPattern 
{
    #region  패턴 실행
    public IEnumerator Execute(PatternContext context)
    {
        context.Boss.NotifyPatternStart();

        var audioHandler = context.Boss.GetComponent<BossAudioHandler>();

        HitData info = context.Skill.hitInfo;
        ShootPatternData data = context.Skill.patternData as ShootPatternData;

        if (data == null)
        {
            Debug.LogWarning("ShootPattern: 패턴 데이터가 잘못 할당됨.");
            yield break;
        }

        //   페이즈 별 값 읽기
        bool isPhase2 = context.Boss.IsPhase2;
        int shootCount = isPhase2 ? data.shotCountPhase2 : data.shotCountPhase1;
        float projectileSpeed = isPhase2 ? data.projectileSpeedPhase2 : data.projectileSpeedPhase1;
        float damageMultiplier = isPhase2 ? data.damageMultiplierPhase2 : data.damageMultiplierPhase1;
        float damage = info.Damage * damageMultiplier;

        //  Windup
        yield return new WaitForSeconds(info.WindupTime);

        //  발 사!
        for (int i = 0; i < shootCount; i++)
        {
            GameObject bullet = context.ProjectilePool.Get(ProjectileType.Bullet);
            bullet.transform.position = context.FirePoint.position;

            Vector2 startPos = context.FirePoint.position;
            Vector2 targetPos = (context.Target != null) ? context.Target.Collider.bounds.center : startPos;
            Vector2 direction = (targetPos - startPos).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            bullet.SetActive(true);

            BossProjectile projectileComponent = bullet.GetComponent<BossProjectile>();

            if (projectileComponent != null )
            {
                projectileComponent.SetupPool(context.ProjectilePool);
                projectileComponent.Initialize(direction, context.Boss, projectileSpeed);
                projectileComponent.Damage = (int)damage;
                projectileComponent.AttackType = info.Type;
            }

            audioHandler?.PlayAttack();

            yield return new WaitForSeconds(data.shotInterval);
        }

        //  후딜
        yield return new WaitForSeconds(info.ActiveTime + context.Skill.delay);

        context.Boss.NotifyPatternEnd();
    }

    #endregion
}
