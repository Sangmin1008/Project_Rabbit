using UnityEngine;
using System.Collections;

public class TNTPattern
{
    private bool _hasFired;

    #region 스킬 패턴 로직

    // Execute
    public IEnumerator Execute(PatternContext context)
    {
        context.Boss.NotifyPatternStart();

        // Windup
        yield return new WaitForSeconds(context.Skill.hitInfo.WindupTime);

        // 애니메이션만 재생 (AnimationEvent는 더 이상 쓰지 않음)
        context.Unit.PlayPattern(context.Boss.CurrentPatternIndex);

        // 페이즈별 던질 개수
        var data = context.Skill.patternData as TNTPatternData;

        if (data == null)
        {
            Debug.LogWarning("[TNTPattern] 패턴 데이터 없음");
            yield break;
        }

        //  페이즈별 던질 개수 & 속도 배율
        bool isPhase2 = context.Boss.IsPhase2;
        int tossCount = isPhase2 ? data.tossCountPhase2 : data.tossCountPhase1;
        float speedMul = isPhase2 ? data.speedMultiplierPhase2 : data.speedMultiplierPhase1;

        // 즉시 for-루프 돌려서 토스
        for (int i = 0; i < tossCount; i++)
        {
            //  던지는 애니메이션 트리거
            context.Unit.PlayPattern(context.Boss.CurrentPatternIndex);

            var bomb = context.ProjectilePool.Get(ProjectileType.Firebomb);
            bomb.transform.position = context.Boss.TntFirePoint.position;
            bomb.transform.rotation = Quaternion.identity;
            bomb.SetActive(true);

            // 보스와 충돌 무시
            var bossCollider = context.Boss.Collider;
            var bombCollider = bomb.GetComponentInChildren<Collider2D>();

            if (bossCollider != null && bombCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, bombCollider, true);
            }

            // 초기화 & 발사
            if (bomb.TryGetComponent<FirebombProjectile>(out var proj))
            {
                proj.SetPool(context.ProjectilePool);
                proj.Launch(context.Boss.TntFirePoint.position, context.Target?.Collider.bounds.center ?? context.Boss.TntFirePoint.position, data.arcHeight, data.throwDuration, context.Skill.hitInfo, context.Boss);
                proj.Damage = (int)(context.Skill.hitInfo.Damage * speedMul);
            }

            // 토스 간격
            yield return new WaitForSeconds(data.tossInterval);
        }

        // ActiveTime + 패턴 후딜
        yield return new WaitForSeconds(context.Skill.hitInfo.ActiveTime + context.Skill.delay);

        context.Boss.NotifyPatternEnd();
    }

    #endregion

}