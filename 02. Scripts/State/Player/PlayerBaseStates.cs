using System.Collections;
using UnityEngine;

public abstract class PlayerGroundState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirParameterHash, false);
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.GroundParameterHash, true);
        owner.CanDoubleJump = true;
    }
    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerAirState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.GroundParameterHash, false);
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirParameterHash, true);
    }
    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
        owner.PlayerAttackHandler.ComboIndex = 0;
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerAttackState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AttackParameterHash, true);
    }
    protected abstract IEnumerator DoAttack(PlayerController owner);
    public virtual void OnUpdate(PlayerController owner) { }

    public virtual void OnExit(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AttackParameterHash, false);
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
    
    protected float GetNormalizedTime(Animator animator, string tag)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            
        if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }
        else if (!animator.IsInTransition(0) && currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }
        else
        {
            return 0f;
        }
    }
}

public abstract class PlayerActionState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ActionParameterHash, true);
    }
    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ActionParameterHash, false);
        owner.PlayerAttackHandler.ComboIndex = 0;
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerHitState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        CinemachineEffects.Instance.ShakeCamera(0.6f, 0.08f);
        owner.PlayerMovement.StopMove();
        owner.IsInvincible = true;
        owner.IsMovementLocked = true;
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.HitParameterHash, true);
        owner.TookDamage = false;
        owner.StartCoroutine(owner.PlayerHitHandler.HitPauseCoroutine(0.07f));
    }

    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
        owner.ComboAttackTriggered = false;
        owner.IsComboAttacking = false;
        owner.JumpTriggered = false;
        owner.DashTriggered = false;
        owner.IsDefensing = false;
        owner.CanAttack = true;
        owner.CanDash = true;
        owner.CanDefense = true;
        // owner.IsInvincible = false;
        owner.IsMovementLocked = false;
        
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.HitParameterHash, false);
        owner.StartCoroutine(owner.PlayerHitHandler.InvincibilityCoroutine(1f));
    }

    public abstract PlayerState CheckTransition(PlayerController owner);
}

public class PlayerDeadState : IState<PlayerController, PlayerState>
{
    public void OnEnter(PlayerController owner)
    {
        owner.PlayerMovement.StopMove();
        owner.IsInvincible = true;
        owner.IsMovementLocked = true;
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DeadParameterHash, true);
        owner.TookDamage = false;
        owner.Dead();
    }

    public void OnUpdate(PlayerController owner)
    {
        
    }

    public void OnExit(PlayerController owner)
    {
        owner.ComboAttackTriggered = false;
        owner.IsComboAttacking = false;
        owner.JumpTriggered = false;
        owner.DashTriggered = false;
        owner.IsDefensing = false;
        owner.CanAttack = true;
        owner.CanDash = true;
        owner.CanDefense = true;
        owner.IsInvincible = false;
        owner.IsMovementLocked = false;
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DeadParameterHash, false);
    }

    public PlayerState CheckTransition(PlayerController owner)
    {
        return PlayerState.Dead;
    }
}