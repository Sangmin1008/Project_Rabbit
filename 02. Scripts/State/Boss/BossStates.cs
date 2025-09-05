using UnityEngine;
using System.Collections;

namespace BossStates
{
    using IState = IState<BossController, BossState>;

    #region 보스 IdleState
    public class IdleState : IState
    {
        public void OnEnter(BossController owner)
        {
            //  X축 잠금
            owner.LockPosition(true, false);
            owner.SetSpeed(0f);
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController owner)
        {
            owner.LockPosition(false, true);
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.Target == null || owner.Target.IsDead)
            {
                return BossState.Idle;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            return distance <= owner.DetectingRange ? BossState.Chasing : BossState.Idle;
        }
    }
    #endregion

    #region 보스 ChasingState
    public class ChasingState : IState
    {
        public void OnEnter(BossController owner)
        {
            owner.LockPosition(false, true);
            owner.FaceToTarget();
        }

        public void OnUpdate(BossController owner)
        {
            if (owner.IsPerformingPattern)
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
                owner.SetSpeed(0f);
                return;
            }

            owner.FaceToTarget();
            owner.Movement();
        }

        public void OnExit(BossController owner)
        {
            owner.LockPosition(true, false);
            owner.SetSpeed(0f);
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            if (owner.Target == null || owner.Target.IsDead)
            {
                return BossState.Idle;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            return distance <= owner.AttackRange ? BossState.Attack : BossState.Chasing;
        }
    }
    #endregion

    #region 보스 AttackState
    public class AttackState : IState
    {
        private Coroutine _attackCoroutine;
        private bool _shouldContinue;

        public void OnEnter(BossController owner)
        {
            owner.LockPosition(true, false);
            owner.SetSpeed(0f);
            owner.FaceToTarget();

            _shouldContinue = true;
            _attackCoroutine = owner.StartCoroutine(AttackLoop(owner));
        }

        private IEnumerator AttackLoop(BossController owner)
        {
            while (_shouldContinue)
            {
                if (owner.IsDead || owner.Target == null || owner.Target.IsDead)
                {
                    break;
                }

                var attackHandler = owner.GetComponent<AttackHandler>();

                // 실제 기본 공격 (여기서 게이지도 오르고 후딜도 처리됨)
                yield return owner.StartCoroutine(attackHandler.BasicAttack());

                // 게이지가 가득 차면 패턴에 진입
                if (owner.TryConsumeGaugeForPattern())
                {
                    owner.ResetBasicGauge();

                    var bossPatternManager = owner.GetComponent<BossPatternManager>();

                    bossPatternManager.PickNextPatternIndex();
                    var next = (BossState)((int)BossState.Pattern1 + bossPatternManager.CurrentPatternIndex);
                    owner.RequestStateChange(next);
                    yield break;
                }
            }
        }

        public void OnUpdate(BossController owner) { }

        public void OnExit(BossController owner)
        {
            owner.LockPosition(false, true);
            if (_attackCoroutine != null)
            {
                owner.StopCoroutine(_attackCoroutine);
            }

            _shouldContinue = false;

            owner.FaceToTarget();
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }


            if (owner.Target == null || owner.Target.IsDead)
            {
                return BossState.Idle;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            if (distance > owner.AttackRange)
            {
                return BossState.Chasing;
            }

            return BossState.Attack;
        }
    }
    #endregion

    #region 보스 BasePatternState → 패턴들 클래스에게 상속하기 위한 추상 클래스
    public abstract class BasePatternState : IState
    {
        private bool _started, _finished;

        protected BasePatternState() { }

        public void OnEnter(BossController owner)
        {
            var so = owner.CurrentPattern;

            var bossPatternManager = owner.GetComponent<BossPatternManager>();

            owner.LockPosition(true, false);
            owner.SetSpeed(0f);
            owner.LockPosition(false, true);
            owner.PlayPattern(owner.CurrentPatternIndex);
            bossPatternManager.SetPatternExecuting(true);

            _started = false;
            _finished = false;
        }

        public void OnUpdate(BossController owner)
        {
            if (_started)
            {
                return;
            }

            _started = true;

            owner.StartCoroutine(RunPattern(owner));
        }

        public void OnExit(BossController owner)
        {
            var so = owner.CurrentPattern;

            var bossPatternManager = owner.GetComponent<BossPatternManager>();

            owner.LockPosition(false, false);
            owner.SetSpeed(owner.StatHandler.MoveSpeed);
            bossPatternManager.SetPatternExecuting(false);
            owner.NotifyPatternEnd();
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            return _finished ? BossState.Chasing : (BossState)((int)BossState.Pattern1 + owner.CurrentPatternIndex);
        }

        protected abstract IEnumerator RunPattern(BossController owner);

        protected void EndPattern()
        {
            _finished= true;
        }
    }

    #endregion

    #region 보스 패턴 1: TNT 투척 패턴
    public class PatternFirebombState : BasePatternState
    {
        public PatternFirebombState() : base()
        {
        }

        protected override IEnumerator RunPattern(BossController owner)
        {
            var attackHandler = owner.GetComponent<AttackHandler>();

            yield return owner.StartCoroutine(attackHandler.ExecutePattern(owner.CurrentPattern));

            EndPattern();
        }
    }
    #endregion

    #region 보스 패턴 2: 총알 연사 패턴
    public class PatternShootState : BasePatternState
    {
        public PatternShootState() : base()
        {
        }

        protected override IEnumerator RunPattern(BossController owner)
        {
            var attackHandler = owner.GetComponent<AttackHandler>();

            yield return owner.StartCoroutine(attackHandler.ExecutePattern(owner.CurrentPattern));

            EndPattern();
        }
    }
    #endregion

    #region 보스 패턴 3: 돌진 패턴
    public class PatternChargeState : BasePatternState
    {
        public PatternChargeState() : base()
        {
        }

        protected override IEnumerator RunPattern(BossController owner)
        {
            var attackHandler = owner.GetComponent<AttackHandler>();

            yield return owner.StartCoroutine(attackHandler.ExecutePattern(owner.CurrentPattern));

            EndPattern();
        }
    }
    #endregion

    #region 보스 패턴 4: 소환 패턴
    public class PatternSummonState : BasePatternState
    {
        public PatternSummonState() : base()
        {
        }

        protected override IEnumerator RunPattern(BossController owner)
        {
            var attackHandler = owner.GetComponent<AttackHandler>();

            yield return owner.StartCoroutine(attackHandler.ExecutePattern(owner.CurrentPattern));

            EndPattern();
        }
    }

    public class PatternShellingState : BasePatternState
    {
        protected override IEnumerator RunPattern(BossController owner)
        {
            var attackHandler = owner.GetComponent<AttackHandler>();

            owner.StartCoroutine(attackHandler.ExecutePattern(owner.CurrentPattern));

            EndPattern();

            yield break;
        }
    }
    #endregion

    #region 보스 EvadeState (회피 패턴)
    public class EvadeState : IState
    {
        private readonly float _distance;
        private readonly float _duration;
        private readonly float _chance;

        private bool _done, _shouldEvade;

        public EvadeState(float distance, float duration, float chance)
        {
            _distance = distance;
            _duration = duration;
            _chance = chance;
        }

        public void OnEnter(BossController owner)
        {
            _done = false;

            _shouldEvade = Random.value < _chance;      //  들어오기 전에 피격 확률 검사
            owner.SetSpeed(0f);
            owner.StartCoroutine(DoEvade(owner));
        }

        private IEnumerator DoEvade(BossController owner)
        {
            if (!_shouldEvade)
            {
                //  확률에 걸리지 않으면 즉시 끝내고 추격으로 전환
                _done = true;
                yield break;
            }

            //  시작 지점
            Vector2 start = owner.transform.position;

            //  플레이어와의 X축 차이로 부호 취하기
            float sign = Mathf.Sign(start.x - owner.Target.Collider.bounds.center.x);

            //  y축은 그대로, X축으로만 RetreatDistance만큼 이동하기
            Vector2 end = start + new Vector2(sign * _distance, 0f);

            float halfWidth = owner.Collider.bounds.extents.x;

            end.x = Mathf.Clamp(end.x, owner.RoomMinX + halfWidth, owner.RoomMaxX -  halfWidth);

            end.y = start.y;

            //  선형 보간으로 매 프레임 위치 갱신
            float timer = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime / _duration;
                owner.transform.position = Vector2.Lerp(start, end, timer);
                yield return null;
            }

            _done = true;
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController owner)
        {
            owner.FaceToTarget();
            owner.SetSpeed(owner.StatHandler.MoveSpeed);
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            return _done ? BossState.Chasing : BossState.Evade;
        }
    }
    #endregion

    #region 보스 DieState
    public class DieState : IState
    {
        public void OnEnter(BossController owner)
        {
            if (owner.IsDead)
            {
                return;
            }

            owner.Dead();
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController owner)
        {
        }

        public BossState CheckTransition(BossController owner)
        {
            return BossState.Die;
        }
    }
    #endregion
}