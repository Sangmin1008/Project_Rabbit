using System.Collections;
using UnityEngine;

public interface IEnemyAttackStrategy 
{
    //  이 전략이 유효한 거리
    float Range { get; }

    //  공격 실행 시 호출되는 코루틴
    IEnumerator Attack();
}
