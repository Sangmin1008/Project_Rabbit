using System.Collections;
using UnityEngine;

namespace PlayerGroundStates
{
    public class IdleState : PlayerGroundState
    {
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.IdleParameterHash, true);
            owner.Rigidbody2D.constraints =
                RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.IdleParameterHash, false);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDead)
                return PlayerState.Dead;
            
            if (owner.ComboAttackTriggered)
                return PlayerState.ComboAttack;

            if (owner.CanStrongAttack)
                return PlayerState.StrongAttack;
            
            if (owner.JumpTriggered)
                return PlayerState.Jump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.IsDefensing && owner.CanDefense)
                return PlayerState.Defense;

            if (owner.TookDamage)
                return owner.PlayerHitHandler.ResolveHitState(owner.ReceivedAttackType);
            
            if (owner.VelocityY < -0.01f || !owner.IsGrounded)
                return PlayerState.Fall;
            
            if (owner.MoveInput.sqrMagnitude > 0.01f)
                return PlayerState.Move;
            
            return PlayerState.Idle;
        }
    }

    public class MoveState : PlayerGroundState
    {
        private WaitForSeconds _seconds = new WaitForSeconds(0.15f);
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.MoveParameterHash, true);
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.MoveParameterHash, false);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDead)
                return PlayerState.Dead;
            
            if (owner.ComboAttackTriggered)
                return PlayerState.ComboAttack;
            
            if (owner.JumpTriggered)
                return PlayerState.Jump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.IsDefensing && owner.CanDefense)
                return PlayerState.Defense;
            
            if (owner.TookDamage)
                return owner.PlayerHitHandler.ResolveHitState(owner.ReceivedAttackType);

            if (owner.VelocityY < -0.01f || !owner.IsGrounded)
                return PlayerState.Fall;
            
            if (owner.MoveInput.sqrMagnitude < 0.01f)
                return PlayerState.Idle;
            
            return PlayerState.Move;
        }
    }
}