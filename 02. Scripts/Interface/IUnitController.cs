using System.Collections;
using UnityEngine;

//  Boss, Enemy 핸들러 관련 필요 인터페이스
public interface IUnitController : IAttackable, IDamageable
{
    StatManager StatManager { get; }
    Collider2D Collider { get; }
    IDamageable Target { get; }
    bool IsDead { get; }

    //  원거리 투사체 발사 위치
    Transform FirePoint { get; }

    void SetSpeed(float speed);
    void PlayBasicAttack();
    void PlayPattern(int patternIndex);
    void PlayDeath();

    //  상태 코루틴 실행
    Coroutine StartCoroutine(IEnumerator routine);
}
