using System.Collections;
using UnityEngine;

namespace PlayerHitStates
{
    public class NormalHitState : PlayerHitState
    {
        private float _timer;
        private const float _hitDuration = 0.4f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.NormalHitParameterHash, true);
            _timer = 0f;
            //Debug.Log("일반 공격 받음");
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.NormalHitParameterHash, false);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (_timer >= _hitDuration && owner.IsGrounded)
            {
                if (owner.IsDead)
                    return PlayerState.Dead;
                
                return PlayerState.Idle;
            }

            return PlayerState.NormalHit;
        }
    }
    
    public class StrongHitState : PlayerHitState
    {
        private float _timer;
        private const float _hitDuration = 0.8f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.StrongHitParameterHash, true);
            _timer = 0f;
            //Debug.Log("강한 공격 받음");
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.StrongHitParameterHash, false);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (_timer >= _hitDuration && owner.IsGrounded)
            {
                if (owner.IsDead)
                    return PlayerState.Dead;

                return PlayerState.Idle;
            }

            return PlayerState.StrongHit;
        }
    }
    
    public class NormalKnockbackState : PlayerHitState
    {
        private float _timer;
        private bool _knockbackDone = false;
        private const float _hitDuration = 0.4f;
        private const float _knockbackForce = 10f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.NormalKnockbackParameterHash, true);
            _timer = 0f;
            _knockbackDone = false;

            owner.StartCoroutine(KnockbackCoroutine(owner));
            //Debug.Log("일반 넉백 받음");
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.NormalKnockbackParameterHash, false);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (_timer >= _hitDuration && _knockbackDone)
            {
                if (owner.IsDead)
                    return PlayerState.Dead;
                
                return PlayerState.Idle;
            }

            return PlayerState.NormalKnockback;
        }

        private IEnumerator KnockbackCoroutine(PlayerController owner)
        {
            Vector2 knockbackDirection = new Vector2((owner.IsFlipX ? 1f : -1f), 1f).normalized;
            
            owner.Rigidbody2D.linearVelocity = knockbackDirection * _knockbackForce;

            while (!owner.IsGrounded)
            {
                yield return null;
            }

            _knockbackDone = true;
        }
    }
    
    public class StrongKnockbackState : PlayerHitState
    {
        private float _timer;
        private bool _knockbackDone = false;
        private const float _hitDuration = 0.8f;
        private const float _knockbackForce = 15f;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.StrongKnockbackParameterHash, true);
            _timer = 0f;
            _knockbackDone = false;

            owner.StartCoroutine(KnockbackCoroutine(owner));
            //Debug.Log("강한 넉백 받음");
        }

        public override void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.StrongKnockbackParameterHash, false);
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (_timer >= _hitDuration && _knockbackDone)
            {
                if (owner.IsDead)
                    return PlayerState.Dead;

                return PlayerState.Idle;
            }

            return PlayerState.StrongKnockback;
        }
        
        private IEnumerator KnockbackCoroutine(PlayerController owner)
        {
            Vector2 knockbackDirection = new Vector2((owner.IsFlipX ? 1f : -1f), 1f).normalized;
            
            owner.Rigidbody2D.linearVelocity = knockbackDirection * _knockbackForce;

            while (!owner.IsGrounded)
            {
                yield return null;
            }

            _knockbackDone = true;
        }
    }
}