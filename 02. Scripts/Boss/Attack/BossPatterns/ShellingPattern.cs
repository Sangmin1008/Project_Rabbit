using UnityEngine;
using System.Collections;

public class ShellingPattern
{
    public IEnumerator Execute(PatternContext context)
    {
        context.Boss.NotifyPatternStart();

        HitData info = context.Skill.hitInfo;
        var data = context.Skill.patternData as ShellingPatternData;

        if (data == null)
        {
            Debug.LogWarning("ShellingPattern: 패턴 데이터가 잘못 할당됨.");
            yield break;
        }

        //  Windup
        yield return new WaitForSeconds(info.WindupTime);

        //  보스나 플레이어 사망 전까지 미사일 반복 소환
        while (!context.Boss.IsDead && context.Target != null && !context.Target.IsDead)
        {
            Vector3 targetCenter = context.Target.Collider.bounds.center;
            Vector3 spawnPos = new Vector3(targetCenter.x, targetCenter.y + data.spawnHeight, targetCenter.z);

            //  풀에서 꺼내서 배치
            var shellingObj = context.ProjectilePool.Get(ProjectileType.Shelling);
            shellingObj.transform.position = spawnPos;
            shellingObj.transform.parent = null;
            shellingObj.SetActive(false);

            //  방향 세팅 → 수직 낙하
            var shelling = shellingObj.GetComponent<ShellingProjectile>();
            shelling.Damage = info.Damage;
            shelling.AttackType = info.Type;
            shelling.Initialize(context.ProjectilePool, context.Boss, Vector2.down, data.projectileSpeed);

            //  다음 소환까지 대기할 것
            yield return new WaitForSeconds(data.spawnInterval);
        }

        //  후딜
        yield return new WaitForSeconds(info.ActiveTime + context.Skill.delay);

        context.Boss.NotifyPatternEnd();
    }
}
