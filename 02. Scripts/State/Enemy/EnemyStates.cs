using UnityEngine;
using System.Collections;

namespace EnemyStates
{
    public class IdleState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
            // 이동 정지 - X축 속도를 0으로
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // 애니메이션
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }

        public void OnUpdate(EnemyController owner)
        {
        }

        public void OnExit(EnemyController owner)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Die;
            }

            if (owner.Target != null && Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position) <= owner.Data.DetectionRange)
            {
                return EnemyState.Chasing;
            }

            return EnemyState.Idle;
        }
    }

    public class ChasingState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
            // 애니메이션
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
            }
        }

        public void OnUpdate(EnemyController owner)
        {
            // 몬스터 컴포넌트에 따른 이동
            var rangedMonster = owner.GetComponent<RangedMonster>();
            var homingMonster = owner.GetComponent<HomingMonster>();
            
            if (rangedMonster != null || homingMonster != null)
            {
                owner.MovementWithDistance(owner.minAttackDistance);
            }
            else
            {
                owner.Movement();
            }
        }

        public void OnExit(EnemyController entity)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Die;
            }

            if (owner.Target == null)
            {
                return EnemyState.Idle;
            }

            float distance = Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position);

            if (distance <= owner.StatManager.GetValueSafe(StatType.AttackRange, 3.0f))
            {
                return EnemyState.Attack;
            }

            return EnemyState.Chasing;
        }
    }

    // 통합 공격 상태 (각 몬스터 컴포넌트로 위임)
    public class AttackState : IState<EnemyController, EnemyState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public AttackState(float atkSpd, float atkRange)
        {
            _atkSpd = atkSpd;
            _atkRange = atkRange;
        }

        public void OnEnter(EnemyController owner)
        {
            owner.StartCoroutine(DoAttack(owner));
        }

        private IEnumerator DoAttack(EnemyController owner)
        {
            _attackDone = false;
            owner.IsAttacking = true;
            owner.CanAttack = false;
            owner.LastAttackTime = Time.time;
            
            // 이동 정지 - X축 속도를 0으로
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // 공격 애니메이션 트리거
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // 애니메이션 대기
            yield return new WaitForSeconds(0.3f);
            
            // 몬스터 타입별 공격 로직을 각 몬스터 스크립트로 위임
            var meleeMonster = owner.GetComponent<MeleeMonster>();
            var rangedMonster = owner.GetComponent<RangedMonster>();
            var homingMonster = owner.GetComponent<HomingMonster>();
            var multiShotMonster = owner.GetComponent<MultiShotMonster>();
            
            if (meleeMonster != null)
            {
                // 근거리 공격: 범위 내 플레이어 탐지 후 Attack 호출
                if (owner.attackPoint != null)
                {
                    Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(owner.attackPoint.position, owner.attackRadius, owner.playerLayer);
                    
                    foreach (Collider2D hit in hitPlayers)
                    {
                        if (hit.CompareTag("Player"))
                        {
                            IDamageable damageable = hit.GetComponent<IDamageable>();
                            if (damageable != null)
                            {
                                // 타겟에 저장하고 Attack 호출
                                owner.SetAttackTarget(damageable);
                                owner.Attack();
                            }
                            
                            // 넉백 효과
                            Vector2 knockbackDir = (hit.transform.position - owner.transform.position).normalized;
                            Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                            if (playerRb != null)
                            {
                                playerRb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
                            }
                        }
                    }
                }
            }
            else if (rangedMonster != null)
            {
                // 원거리 공격: RangedMonster에게 위임 - 직접 공격 메서드 호출
                rangedMonster.PerformRangedAttack();
            }
            else if (homingMonster != null)
            {
                // 유도 공격: HomingMonster에게 위임
                homingMonster.PerformHomingAttack();
            }
            else if (multiShotMonster != null)
            {
                // 멀티샷 공격: MultiShotMonster에게 위임
                multiShotMonster.PerformMultiShotAttack();
            }
            
            // 공격 애니메이션 종료 대기
            yield return new WaitForSeconds(0.2f);
            
            owner.IsAttacking = false;
            
            // 공격 속도에 따른 대기
            float cooldown = 1f / _atkSpd;
            if (homingMonster != null)
            {
                cooldown *= 1.5f; // 유도 미사일은 더 긴 쿨다운
            }
            else if (multiShotMonster != null)
            {
                cooldown *= 1.2f; // 멀티샷도 약간 긴 쿨다운
            }
            
            yield return new WaitForSeconds(cooldown);
            
            owner.CanAttack = true;
            _attackDone = true;
        }

        public void OnUpdate(EnemyController owner)
        {
        }

        public void OnExit(EnemyController owner)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Die;
            }

            if (!_attackDone)
            {
                return EnemyState.Attack;
            }

            // 타겟과의 거리 확인
            if (owner.Target != null)
            {
                float distance = Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position);
                
                // 원거리/유도 몬스터의 경우 최소 거리 체크
                var rangedMonster = owner.GetComponent<RangedMonster>();
                var homingMonster = owner.GetComponent<HomingMonster>();
                var multiShotMonster = owner.GetComponent<MultiShotMonster>();
                
                if ((rangedMonster != null || homingMonster != null || multiShotMonster != null) && distance < owner.minAttackDistance)
                {
                    return EnemyState.Chasing;
                }
                
                // 타겟이 범위를 벗어났으면 추격
                if (distance > _atkRange)
                {
                    return EnemyState.Chasing;
                }
                
                // 범위 내에 있으면 다시 공격
                return EnemyState.Attack;
            }

            return EnemyState.Chasing;
        }
    }

    public class DieState : IState<EnemyController, EnemyState>
    {
        private bool deathProcessStarted = false;

        public void OnEnter(EnemyController owner)
        {
            if (!deathProcessStarted)
            {
                deathProcessStarted = true;
                owner.StartCoroutine(ProcessDeath(owner));
            }
        }

        private IEnumerator ProcessDeath(EnemyController owner)
        {
            //Debug.Log($"{owner.name} entered Die state");
            
            // Rigidbody2D 중력 및 움직임 완전 중지
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            // 충돌체 비활성화
            var collider = owner.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // CharacterController가 있으면 비활성화
            var characterController = owner.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            // 사망 애니메이션 재생
            var animator = owner.GetComponent<Animator>();
            float deathAnimationDuration = 1.0f;
            
            if (animator != null)
            {
                // 모든 bool 파라미터를 false로 리셋
                animator.SetBool("isMoving", false);
                
                // Animator 업데이트 강제 실행
                animator.Update(0f);
                
                // Death 트리거 설정
                bool triggerSet = false;
                
                // deathTrig 파라미터 체크 (이미지에서 보이는 파라미터)
                if (HasParameter(animator, "deathTrig", AnimatorControllerParameterType.Trigger))
                {
                    animator.SetTrigger("deathTrig");
                    //Debug.Log("deathTrig trigger was set");
                    triggerSet = true;
                }
                
                // 다른 가능한 트리거들도 체크
                if (!triggerSet)
                {
                    string[] possibleTriggers = { "Death", "Die", "death", "die" };
                    foreach (string trigger in possibleTriggers)
                    {
                        if (HasParameter(animator, trigger, AnimatorControllerParameterType.Trigger))
                        {
                            animator.SetTrigger(trigger);
                            //Debug.Log($"Death animation trigger '{trigger}' was set");
                            triggerSet = true;
                            break;
                        }
                    }
                }
                
                // 트리거 설정 후 강제로 Death 상태로 전환
                if (triggerSet)
                {
                    // 짧은 대기 후 강제 전환
                    yield return new WaitForSeconds(0.1f);
                    
                    // 직접 Death 상태로 강제 전환
                    animator.Play("Death", 0, 0f);
                    //Debug.Log("Forced play Death animation");
                }
                else
                {
                    // 트리거가 없으면 바로 Death 상태로 전환
                    animator.Play("Death", 0, 0f);
                    //Debug.Log("Direct play Death animation (no trigger found)");
                }
                
                // 애니메이션이 실제로 재생되는지 확인
                yield return new WaitForSeconds(0.1f);
                
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                //Debug.Log($"Current animator state: {stateInfo.fullPathHash} (Death hash would be different)");
                
                // Death 상태 이름 확인
                if (stateInfo.IsName("Death") || stateInfo.IsName("Die") || stateInfo.IsName("death"))
                {
                    deathAnimationDuration = stateInfo.length;
                    //Debug.Log($"Death animation is playing correctly, duration: {deathAnimationDuration}");
                }
                else
                {
                    Debug.LogWarning($"Death animation still not playing! Trying layer-based approach");
                    
                    // 레이어 0의 모든 상태 이름을 출력해보기
                    int layerIndex = 0;
                    int stateCount = animator.GetCurrentAnimatorClipInfoCount(layerIndex);
                    //Debug.Log($"Current clip count: {stateCount}");
                    
                    // 다시 한번 강제 재생 시도
                    animator.CrossFade("Death", 0.1f, 0);
                    yield return new WaitForSeconds(0.2f);
                    
                    // 최종 확인
                    stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    if (!stateInfo.IsName("Death"))
                    {
                        Debug.LogError("Failed to play Death animation! Check Animator Controller setup.");
                    }
                }
            }
            
            // 사망 이펙트 생성
            if (owner.deathEffectPrefab != null)
            {
                GameObject deathEffect = Object.Instantiate(owner.deathEffectPrefab, owner.transform.position, Quaternion.identity);
            }
            
            // 사망 사운드 재생
            if (owner.deathSound != null)
            {
                AudioSource audioSource = owner.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(owner.deathSound);
                }
            }
            
            // 사망 애니메이션 재생 대기
            yield return new WaitForSeconds(deathAnimationDuration);
            
            // 페이드아웃 효과 (단순화)
            float fadeOutDuration = 1f;
            var spriteRenderer = owner.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                float elapsed = 0f;

                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / fadeOutDuration;
                    
                    // 투명도 조절
                    float alpha = Mathf.Lerp(1f, 0f, t);
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    
                    yield return null;
                }
            }
            
            // 몬스터를 풀로 반환
            string poolName = owner.GetPoolName();
            if (!string.IsNullOrEmpty(poolName) && MonsterObjectPool.Instance != null)
            {
                MonsterObjectPool.Instance.ReturnMonster(owner.gameObject, poolName);
            }
            else
            {
                Debug.LogWarning($"No pool found for {owner.name}, destroying object");
                Object.Destroy(owner.gameObject);
            }
        }
        
        private bool HasParameter(Animator animator, string paramName, AnimatorControllerParameterType? expectedType = null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    if (expectedType == null || param.type == expectedType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void OnUpdate(EnemyController owner)
        {
            // 죽음 상태에서는 아무것도 하지 않음
        }

        public void OnExit(EnemyController owner)
        {
            deathProcessStarted = false;
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            return EnemyState.Die;
        }
    }
}