using UnityEngine;
using System.Collections;

namespace BaseEnemyStates
{
    using IState = IState<BaseEnemyController, BaseEnemyState>;

    #region Idle State
    public class IdleState : IState
    {
        public void OnEnter(BaseEnemyController owner)
        {
            owner.LockXPosition();
            owner.SetSpeed(0);
        }

        public void OnUpdate(BaseEnemyController owner)
        {
        }

        public void OnExit(BaseEnemyController owner)
        {
        }

        public BaseEnemyState CheckTransition(BaseEnemyController owner)
        {
            if (owner.IsDead)
            {
                return BaseEnemyState.Die;
            }

            if (owner.IsAttacking)
            {
                return BaseEnemyState.Attack;
            }

            if (owner.Target == null || owner.Target.IsDead)
            {
                return BaseEnemyState.Idle;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);
            float cooldown = Time.time - owner.LastAttackTime;

            //  사거리 안 & 쿨다운 끝났으면 공격하기
            if (distance <= owner.AttackRange && cooldown >= owner.AttackCooldown)
            {
                return BaseEnemyState.Attack;
            }

            //  사거리 밖이면 추격
            if (distance > owner.AttackRange)
            {
                return BaseEnemyState.Chasing;
            }

            //  사거리 안인데 아직도 쿨타임 중이면 Idle 유지하기
            return BaseEnemyState.Idle;
        }
    }

    #endregion

    #region Chasing State
    public class ChasingState : IState
    {
        public void OnEnter(BaseEnemyController owner)
        {
            owner.UnLockXPosition();

            //  추격 시작 시 이동 속도 세팅
            owner.FaceToTarget();
            owner.SetSpeed(owner.MoveSpeed);
        }

        public void OnUpdate(BaseEnemyController owner)
        {
            if (owner.IsAttacking)
            {
                return;
            }

            if (owner.Target == null || owner.Target.IsDead)
            {
                return;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            if (distance <= owner.AttackRange)
            {
                owner.MovementHandler.StopXMovement();
                owner.SetSpeed(0);
                owner.FaceToTarget();
                return;
            }

            //  원거리형이면 최소 거리 유지, 근거리는 그냥 추격하기
            if (owner is EliteRangedEnemyController)
            {
                owner.MovementWithDistance(owner.MinAttackDistance);
            }

            else
            {
                owner.Movement();
            }

            owner.FaceToTarget();
        }

        public void OnExit(BaseEnemyController owner)
        {
            owner.LockXPosition();
            //  추격 종료 시 마지막 바라보기 유지
            owner.FaceToTarget();
            owner.OnChaseEnter();
        }

        public BaseEnemyState CheckTransition(BaseEnemyController owner)
        {
            float distance = Vector2.Distance(owner.transform.position,owner.Target?.Collider.bounds.center ?? owner.transform.position);

            if (owner.IsAttacking)
            {
                return BaseEnemyState.Attack;
            }

            if (owner.IsDead)
            {
                return BaseEnemyState.Die;
            }

            if (owner.Target == null || owner.Target.IsDead)
            {
                return BaseEnemyState.Idle;
            }

            return (distance <= owner.AttackRange) ? BaseEnemyState.Attack : BaseEnemyState.Chasing;
        }
    }
    #endregion

    #region Attack State
    public class AttackState : IState
    {
        private bool _attackFinished;
        private Coroutine _attackCoroutine;

        public void OnEnter(BaseEnemyController owner)
        {
            owner.LockXPosition();

            owner.MovementHandler.StopXMovement();
            owner.SetSpeed(0);
            owner.FaceToTarget();

            if (_attackCoroutine == null)
            {
                _attackFinished = false;

                owner.Attack();

                _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            }
        }

        private IEnumerator DoAttack(BaseEnemyController owner)
        {
            while (owner.IsAttacking)
            {
                yield return null;
            }

            owner.LastAttackTime = Time.time;
            _attackFinished = true;

            _attackCoroutine = null;
        }

        public void OnUpdate(BaseEnemyController owner)
        {
        }

        public void OnExit(BaseEnemyController owner)
        {
            if (_attackCoroutine != null)
            {
                owner.StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            _attackFinished = false;
        }

        public BaseEnemyState CheckTransition(BaseEnemyController owner)
        {
            if (owner.IsDead)
            {
                return BaseEnemyState.Die;
            }

            if (!_attackFinished)
            {
                return BaseEnemyState.Attack;
            }

            return BaseEnemyState.Cooldown;
        }
    }
    #endregion

    #region Cooldown State
    public class CooldownState : IState
    {
        private float _startTime;

        public void OnEnter(BaseEnemyController owner)
        {
            _startTime = Time.time;

            owner.MovementHandler.StopXMovement();
            owner.SetSpeed(0);
        }

        public void OnUpdate(BaseEnemyController owner)
        {
        }

        public void OnExit(BaseEnemyController entity)
        {
        }

        public BaseEnemyState CheckTransition(BaseEnemyController owner)
        {
            if (owner.IsDead)
            {
                return BaseEnemyState.Die;
            }

            if (Time.time - owner.LastAttackTime < owner.AttackCooldown)
            {
                return BaseEnemyState.Cooldown;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            return (distance <= owner.AttackRange) ? BaseEnemyState.Attack : BaseEnemyState.Chasing;
        }
    }

    #endregion

    #region Die State
    public class DieState : IState
    {
        public void OnEnter(BaseEnemyController owner)
        {
            owner.Dead();
        }

        public void OnUpdate(BaseEnemyController owner)
        {
        }

        public void OnExit(BaseEnemyController owner)
        {
        }

        public BaseEnemyState CheckTransition(BaseEnemyController owner)
        {
            return BaseEnemyState.Die;
        }
    }

    #endregion
}
