using UnityEngine;

namespace PlayerAirStates
{
    public class JumpState : PlayerAirState
    {
        private float _timer = 0f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.JumpParameterHash, true);
            owner.Jump();
            // owner.CanDoubleJump = true;
            _timer = 0f;
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.JumpParameterHash, false);
            owner.JumpTriggered = false;
            owner.AirAttackTriggered = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDead)
                return PlayerState.Dead;

            if (owner.AirAttackTriggered)
            {
                owner.PlayerAnimation.Animator.SetTrigger(owner.PlayerAnimation.AnimationData.AttackTriggerParameterHash);
                return PlayerState.AirAttack;
            }
            
            if (owner.TookDamage)
                return owner.PlayerHitHandler.ResolveHitState(owner.ReceivedAttackType);
            
            if (owner.JumpTriggered && owner.CanDoubleJump)
                return PlayerState.DoubleJump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.VelocityY < -0.01f)
                return PlayerState.Fall;
            
            if (owner.IsGrounded)
            {
                if (_timer > 0.4f) owner.LandingEffect.Play(owner.GroundDetector.GroundCheckBlock.position, owner.IsFlipX);
                return PlayerState.Idle;
            }

            return PlayerState.Jump;
        }
    }
    
    public class FallState : PlayerAirState
    {
        private float _timer = 0f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.FallParameterHash, true);
            _timer = 0f;
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.FallParameterHash, false);
            owner.JumpTriggered = false;
            owner.AirAttackTriggered = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDead)
                return PlayerState.Dead;

            if (owner.AirAttackTriggered)
            {
                owner.PlayerAnimation.Animator.SetTrigger(owner.PlayerAnimation.AnimationData.AttackTriggerParameterHash);
                return PlayerState.AirAttack;
            }
            
            if (owner.TookDamage)
                return owner.PlayerHitHandler.ResolveHitState(owner.ReceivedAttackType);

            if (owner.JumpTriggered && owner.CanDoubleJump)
                return PlayerState.DoubleJump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.IsGrounded)
            {
                if (_timer > 0.4f) owner.LandingEffect.Play(owner.GroundDetector.GroundCheckBlock.position, owner.IsFlipX);
                return PlayerState.Idle;
            }
            
            return PlayerState.Fall;
        }
    }
    
    public class DoubleJumpState : PlayerAirState
    {
        private float _timer = 0f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DoubleJumpParameterHash, true);
            owner.Jump();
            owner.CanDoubleJump = false;
            _timer = 0f;
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DoubleJumpParameterHash, false);
            owner.JumpTriggered = false;
            owner.AirAttackTriggered = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDead)
                return PlayerState.Dead;

            if (owner.AirAttackTriggered)
            {
                owner.PlayerAnimation.Animator.SetTrigger(owner.PlayerAnimation.AnimationData.AttackTriggerParameterHash);
                return PlayerState.AirAttack;
            }
            
            if (owner.TookDamage)
                return owner.PlayerHitHandler.ResolveHitState(owner.ReceivedAttackType);

            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.VelocityY < -0.01f)
                return PlayerState.Fall;
            
            if (owner.IsGrounded)
            {
                if (_timer > 0.4f) owner.LandingEffect.Play(owner.GroundDetector.GroundCheckBlock.position, owner.IsFlipX);
                return PlayerState.Idle;
            }

            return PlayerState.DoubleJump;
        }
    }
}