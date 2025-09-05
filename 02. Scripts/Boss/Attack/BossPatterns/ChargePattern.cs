using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChargePattern
{
    // 넉백 힘/위치 보정값
    private const float KnockbackSafeOffsetX = 1.2f;
    private const float KnockbackSafeOffsetY = 0f;

    private const float KnockbackMultiplier = 0.1f;
    private const float PostHitYMultiplier = 0.2f;

    private class KnockbackProxy : IAttackable
    {
        private readonly BossController _boss;

        public KnockbackProxy(BossController boss) => _boss = boss;

        public StatBase AttackStat => _boss.AttackStat;

        public IDamageable Target => _boss.Target;

        public AttackType AttackType => AttackType.Launch;

        public void Attack()
        {
            _boss.Attack();
        }
    }

    public IEnumerator Execute(PatternContext context)
    {
        var boss = context.Boss;
        var audioHandler = boss.GetComponent<BossAudioHandler>();
        var rb = boss.Rigidbody;
        var phaseManager = boss.GetComponent<PhaseManager>();
        var effectHandler = boss.GetComponentInChildren<BossEffectHandler>();
        var data = context.Skill.patternData as ChargePatternData;

        if (data == null)
        {
            yield break;
        }

        // 1) 돌진 준비 사운드
        audioHandler?.PlayCharge();

        // 2) Afterimage 세팅 검사
        ValidateAfterimage(effectHandler);

        // 이동 관련 변수
        float fixedY = rb.position.y;
        float speed = phaseManager.IsPhase2 ? data.speedPhase2 : data.speedPhase1;
        float duration = data.chargeDuration;
        Vector2 dir = ((Vector2)((boss.Target as MonoBehaviour).transform.position) - rb.position).normalized;

        // 3) 돌진 이펙트 시작
        effectHandler?.StartDashEffects();
        var damagedTargets = new HashSet<IDamageable>();

        float elapsed = 0f;
        // 돌진 중엔 AttackType이 항상 Launch이므로 캐싱해 둡니다
        string hitTypeName = AttackType.Launch.ToString();

        // 4) 돌진 루프
        while (elapsed < duration)
        {
            MoveBoss(rb, dir, speed, data, fixedY, elapsed, duration);

            // 플레이어 충돌 감지
            var hits = DetectHits(rb.position, dir);

            foreach (var col in hits)
            {
                HandlePlayerHit(col, boss, phaseManager, data, rb, hitTypeName, damagedTargets);
            }

            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 5) 돌진 이펙트 종료
        effectHandler?.StopDashEffects();

        // 6) 패턴 종료 대기 & 알림
        yield return new WaitForSeconds(context.Skill.hitInfo.ActiveTime + context.Skill.delay);
        boss.NotifyPatternEnd();
    }

    // 보스 이동
    private void MoveBoss(Rigidbody2D rb, Vector2 dir, float speed, ChargePatternData data, float fixedY, float elapsed, float duration)
    {
        float t = Mathf.Clamp01(elapsed / duration);
        float curve = data.chargeCurve.Evaluate(t);
        float ms = speed * curve;

        Vector2 np = rb.position + dir * ms * Time.fixedDeltaTime;
        np.y = fixedY;
        rb.MovePosition(np);
    }

    // 플레이어 충돌 감지
    private Collider2D[] DetectHits(Vector2 bossPos, Vector2 dir)
    {
        int playerMask = 1 << LayerMask.NameToLayer("Player");
        Vector2 center = bossPos + dir * 2.5f;
        Vector2 size = new Vector2(8f, 4f);
        return Physics2D.OverlapBoxAll(center, size, 0f, playerMask);
    }

    // 충돌 처리
    private void HandlePlayerHit(Collider2D col, BossController boss, PhaseManager phaseManager, ChargePatternData data, Rigidbody2D bossRb, string hitTypeName, HashSet<IDamageable> damagedTargets)
    {
        if (!col.TryGetComponent<IDamageable>(out var target) || target.IsDead || damagedTargets.Contains(target))
        {
            return;
        }

        var player = col.GetComponent<PlayerController>();

        bool wasHitBefore = player != null && player.TookDamage;

        var prevType = player != null ? player.ReceivedAttackType : AttackType.Launch;

        // 1) LaunchProxy를 통해 강제 Launch 타입으로 데미지 호출
        var proxy = new KnockbackProxy(boss);

        target.TakeDamage(proxy);

        // 2) 물리적 넉백 적용
        var victimMb = target as MonoBehaviour;

        if (victimMb != null)
        {
            var rigid = victimMb.GetComponentInParent<Rigidbody2D>();

            if (rigid != null)
            {
                ApplyKnockback(boss, rigid, bossRb, rigid.position, phaseManager, data, hitTypeName, wasHitBefore, prevType);
            }
        }


        damagedTargets.Add(target);
    }

    // 넉백 적용
    private void ApplyKnockback(BossController boss, Rigidbody2D targetRb, Rigidbody2D bossRb, Vector3 targetPos, PhaseManager phaseManager, ChargePatternData data, string hitTypeName, bool wasHitBefore, AttackType prevType)
    {
        float dx = Mathf.Sign(targetPos.x - bossRb.position.x);

        // Launch는 강한 넉백으로 분기
        bool isKnockback = hitTypeName.Contains(nameof(AttackType.Launch));
        float basePowerX = isKnockback ? 10f : 5f;

        float speedFactor = phaseManager.IsPhase2 ? data.speedPhase2 : data.speedPhase1;

        float basePowerY = 5f;

        if (isKnockback)
        {
            if (wasHitBefore && prevType != AttackType.Launch)
            {
                basePowerY *= PostHitYMultiplier;
            }

            basePowerY *= KnockbackMultiplier;
        }

        float forceX = basePowerX + speedFactor * 0.2f;
        float forceY = isKnockback ? 0f : (basePowerY + speedFactor * 0.1f);

        // y축 오프셋으로 벽 박힘 방지
        float safeY = bossRb.position.y + KnockbackSafeOffsetY;
        Vector2 safePos = new Vector2(bossRb.position.x + dx * KnockbackSafeOffsetX, safeY);

        // boss에서 코루틴 실행
        boss.StartCoroutine(KnockbackPlayerOnce(targetRb, safePos, dx * forceX, forceY));
    }

    // 넉백 물리 효과
    private IEnumerator KnockbackPlayerOnce(Rigidbody2D rigid, Vector2 safePos, float forceX, float forceY)
    {
        //  넉백 위치로 강제 이동 (벽 바깥으로 꺼내기)
        rigid.MovePosition(safePos);

        rigid.linearVelocity = Vector2.zero;

        yield return new WaitForFixedUpdate();

        rigid.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
    }

    // Afterimage 세팅 검사
    private void ValidateAfterimage(BossEffectHandler effectHandler)
    {
        if (effectHandler?.afterimageController == null)
        {
            Debug.LogWarning("[ChargePattern] AfterimageController 미할당! 잔상 미출력.");
            return;
        }

        var ps = effectHandler.afterimageController.GetComponent<ParticleSystem>();

        if (ps && ps.main.simulationSpace != ParticleSystemSimulationSpace.World)
        {
            Debug.LogWarning("[ChargePattern] Afterimage 파티클의 Simulation Space를 World로 설정하세요.");
        }
    }
}