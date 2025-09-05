using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerAttackStates
{
    public class ComboAttackState : PlayerAttackState
    {
        private AttackInfoData _attackInfoData;
        private Coroutine _attackCoroutine;
        private Coroutine _attackCounterCoroutine;
        private bool _alreadyAppliedCombo;

        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            if (_attackCounterCoroutine != null)
                owner.StopCoroutine(_attackCounterCoroutine);
            owner.PlayerAnimation.Animator.SetInteger("Combo", owner.PlayerAttackHandler.ComboIndex);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ComboAttackParameterHash, true);
            
            if (Mathf.Abs(owner.MoveInput.x) > 0.01 && owner.PlayerAttackHandler.ComboIndex == 0)
            {
                _attackInfoData = owner.PlayerAttackHandler.MoveAttackInfoData;
                owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.MoveAttackParameterHash, true);
            }
            else 
                _attackInfoData = owner.PlayerAttackHandler.ComboAttackInfoDatas[owner.PlayerAttackHandler.ComboIndex];
            
            owner.PlayerAttackHandler.SetHitBox(_attackInfoData.HitBoxCenter, _attackInfoData.HitBoxSize);
            owner.PlayerAttackHandler.CurrentAttackInfoData = _attackInfoData;
            
            owner.IsComboAttacking = true;
            owner.ComboAttackTriggered = false;
            owner.CanAttack = false;
            owner.StopMoving();
            if (_attackInfoData == owner.PlayerAttackHandler.MoveAttackInfoData)
            {
                owner.StartCoroutine(MoveAttack(owner));
            }
            
            _alreadyAppliedCombo = false;
            
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            //Debug.Log(_attackInfoData.AttackName);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = "Attack";

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);
            //owner.Attack(); Animation Event에서 호출하게 수정함

            yield return new WaitUntil(() => _alreadyAppliedCombo || GetNormalizedTime(animator, attackName) >= 1f);

            if (_alreadyAppliedCombo)
            {
                owner.PlayerAttackHandler.ComboIndex = _attackInfoData.ComboStateIndex != -1 ? _attackInfoData.ComboStateIndex + 1 : 0;
            }
            else
            {
                owner.PlayerAttackHandler.ComboIndex = 0;
            }
            owner.IsComboAttacking = false;
            owner.PlayerAnimation.Animator.SetInteger("Combo", owner.PlayerAttackHandler.ComboIndex);
        }

        private IEnumerator MoveAttack(PlayerController owner)
        {
            float originGravity = owner.Rigidbody2D.gravityScale;
            owner.Rigidbody2D.gravityScale = 0f;
            owner.Rigidbody2D.linearVelocity = owner.StatManager.GetValue(StatType.DashForce) *
                                               (owner.IsFlipX ? Vector2.left : Vector2.right);

            yield return new WaitForSeconds(0.05f);
            
            owner.Rigidbody2D.linearVelocity = Vector2.zero;
            owner.Rigidbody2D.gravityScale = originGravity;
        }

        public override void OnUpdate(PlayerController owner)
        {
            TryComboAttack(owner);
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.MoveAttackParameterHash, false);
            if (!_alreadyAppliedCombo)
                owner.PlayerAttackHandler.ComboIndex = 0;
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ComboAttackParameterHash, false);
            owner.ComboAttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.CanAttack = true;
            owner.IsComboAttacking = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
            _attackCounterCoroutine = owner.StartCoroutine(owner.PlayerAttackHandler.AttackCounter());
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsComboAttacking)
                return PlayerState.Idle;
            
            if (owner.IsDefensing && owner.CanDefense)
                return PlayerState.Defense;

            return PlayerState.ComboAttack;
        }
        
        private void TryComboAttack(PlayerController owner)
        {
            if (_alreadyAppliedCombo) return;
            float normalizedTime = GetNormalizedTime(owner.PlayerAnimation.Animator, "Attack");
    
            if (normalizedTime < _attackInfoData.ComboTransitionTime) return;
            if (normalizedTime > _attackInfoData.DealingEndTransitionTime) return;
            owner.CanAttack = true;
            if (_attackInfoData.ComboStateIndex == -1) return;
            if (!owner.ComboAttackTriggered) return;

            _alreadyAppliedCombo = true;
            owner.ComboAttackTriggered = false;
            owner.PlayerAttackHandler.ComboIndex = _attackInfoData.ComboStateIndex + 1;
        }
    }

    public class AirAttackState : PlayerAttackState
    {
        private AttackInfoData _attackInfoData;
        private Coroutine _attackCoroutine;

        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirAttackParameterHash, true);
            owner.IsAirAttacking = true;
            _attackInfoData = owner.PlayerAttackHandler.AirAttackInfoData;
                        
            owner.PlayerAttackHandler.SetHitBox(_attackInfoData.HitBoxCenter, _attackInfoData.HitBoxSize);
            owner.PlayerAttackHandler.CurrentAttackInfoData = _attackInfoData;
            
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            //Debug.Log(_attackInfoData.AttackName);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = "Attack";

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);
            // owner.Attack(); Animation Event에서 호출하게 수정함

            yield return new WaitUntil(() => GetNormalizedTime(animator, attackName) >= 1f);

            owner.IsAirAttacking = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirAttackParameterHash, false);
            owner.AirAttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.IsAirAttacking = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
            owner.PlayerAttackHandler.ComboIndex = 0;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsAirAttacking)
                return PlayerState.Fall;

            return PlayerState.AirAttack;
        }
    }
    
    public class StrongAttackState : PlayerAttackState
    {
        private AttackInfoData _attackInfoData;
        private Coroutine _attackCoroutine;

        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.StrongAttackParameterHash, true);
            owner.IsAttacking = true;
            owner.IsMovementLocked = true;
            owner.IsInvincible = true;
            owner.StopMoving();
            owner.PlayerAttackHandler.HitBox.transform.SetParent(null);
            owner.DashEffect.Play(owner.GroundDetector.GroundCheckBlock.position, owner.IsFlipX);
            PlayerUIEvents.OnStrongAttack.Invoke();
            
            _attackInfoData = owner.PlayerAttackHandler.StrongAttackInfoData;
                        
            owner.PlayerAttackHandler.SetHitBox(_attackInfoData.HitBoxCenter, _attackInfoData.HitBoxSize);
            owner.PlayerAttackHandler.CurrentAttackInfoData = _attackInfoData;
            
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            owner.StartCoroutine(owner.PlayerActionHandler.Dash(0.1f, true, 1.3f));
            //Debug.Log(_attackInfoData.AttackName);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = "Attack";

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);

            yield return new WaitUntil(() => GetNormalizedTime(animator, attackName) >= 1f);

            owner.IsAttacking = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.StrongAttackParameterHash, false);
            owner.AttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.IsAttacking = false;
            owner.IsMovementLocked = false;
            owner.IsInvincible = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
            owner.PlayerAttackHandler.ComboIndex = 0;
            owner.PlayerAttackHandler.HitBox.transform.SetParent(owner.transform);
            owner.PlayerAttackHandler.HitBox.transform.localPosition = new Vector2(1, 0);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsAttacking)
                return PlayerState.Idle;

            return PlayerState.StrongAttack;
        }
    }
}