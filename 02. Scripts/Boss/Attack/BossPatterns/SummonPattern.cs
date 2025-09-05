using UnityEngine;
using System.Collections;

public class SummonPattern
{
    #region 패턴 실행
    public IEnumerator Execute(PatternContext context)
    {
        context.Boss.NotifyPatternStart();

        HitData info = context.Skill.hitInfo;
        SummonPatternData data = context.Skill.patternData as SummonPatternData;

        if (data == null)
        {
            Debug.LogWarning("SummonPattern: 패턴 데이터가 잘못 할당되었음.");
            yield break;
        }

        //  Windup
        if (info.WindupTime > 0f)
        {
            yield return new WaitForSeconds(info.WindupTime);
        }

        //  페이즈별 소환 개수 결정
        bool isPhase2 = context.Boss.IsPhase2;
        int minCount = isPhase2 ? data.summonCountMinPhase2 : data.summonCountMinPhase1;
        int maxCount = isPhase2 ? data.summonCountMaxPhase2 : data.summonCountMaxPhase1;

        Vector2 center = context.Target != null ? (Vector2)context.Target.Collider.bounds.center : (Vector2)context.Boss.transform.position;

        //  SummonHandler 호출
        context.SummonHandler.Summon(data, context.Boss.gameObject, center);

        yield return new WaitForSeconds(context.Skill.delay);

        context.Boss.NotifyPatternEnd();
    }

    #endregion
}
