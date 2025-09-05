using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PlayerAnimationData
{
    [Header("Ground")] 
    [SerializeField] private string groundParameterName = "@Ground";
    [SerializeField] private string idleParameterName = "Idle";
    [SerializeField] private string moveParameterName = "Move";
    
    [Header("Air")]
    [SerializeField] private string airParameterName = "@Air";
    [SerializeField] private string jumpParameterName = "Jump";
    [SerializeField] private string fallParameterName = "Fall";
    [SerializeField] private string doubleJumpParameterName = "DoubleJump";
    
    [Header("Attack")]
    [SerializeField] private string attackParameterName = "@Attack";
    [SerializeField] private string comboAttackParameterName = "ComboAttack";
    [SerializeField] private string moveAttackParameterName = "MoveAttack";
    [SerializeField] private string airAttackParameterName = "AirAttack";
    [SerializeField] private string strongAttackParameterName = "StrongAttack";
    [SerializeField] private string attackTriggerParameterName = "AttackTrigger";
    
    [Header("Action")]
    [SerializeField] private string actionParameterName = "@Action";
    [SerializeField] private string defenseParameterName = "Defense";
    [SerializeField] private string parryParameterName = "Parry";
    [SerializeField] private string dodgeParameterName = "Dodge";
    [SerializeField] private string dashParameterName = "Dash";
    
    [Header("Hit")]
    [SerializeField] private string hitParameterName = "@Hit";
    [SerializeField] private string normalHitParameterName = "NormalHit";
    [SerializeField] private string strongHitParameterName = "StrongHit";
    [SerializeField] private string normalKnockbackParameterName = "NormalKnockback";
    [SerializeField] private string strongKnockbackParameterName = "StrongKnockback";
    
    [Header("Dead")]
    [SerializeField] private string deadParameterName = "@Dead";

    
    public int GroundParameterHash { get; private set; }
    public int IdleParameterHash { get; private set; }
    public int MoveParameterHash { get; private set; }
    public int AirParameterHash { get; private set; }
    public int JumpParameterHash { get; private set; }
    public int FallParameterHash { get; private set; }
    public int DoubleJumpParameterHash { get; private set; }
    public int AttackParameterHash { get; private set; }
    public int ComboAttackParameterHash { get; private set; }
    public int MoveAttackParameterHash { get; private set; }
    public int AirAttackParameterHash { get; private set; }
    public int StrongAttackParameterHash { get; private set; }
    public int AttackTriggerParameterHash { get; private set; }
    public int ActionParameterHash { get; private set; }
    public int DefenseParameterHash { get; private set; }
    public int ParryParameterHash { get; private set; }
    public int DodgeParameterHash { get; private set; }
    public int DashParameterHash { get; private set; }
    public int DeadParameterHash { get; private set; }
    public int HitParameterHash { get; private set; }
    public int NormalHitParameterHash { get; private set; }
    public int StrongHitParameterHash { get; private set; }
    public int NormalKnockbackParameterHash { get; private set; }
    public int StrongKnockbackParameterHash { get; private set; }

    public void Initialize()
    {
        GroundParameterHash = Animator.StringToHash(groundParameterName);
        IdleParameterHash = Animator.StringToHash(idleParameterName);
        MoveParameterHash = Animator.StringToHash(moveParameterName);
        
        AirParameterHash = Animator.StringToHash(airParameterName);
        JumpParameterHash = Animator.StringToHash(jumpParameterName);
        FallParameterHash = Animator.StringToHash(fallParameterName);
        DoubleJumpParameterHash = Animator.StringToHash(doubleJumpParameterName);
        
        AttackParameterHash = Animator.StringToHash(attackParameterName);
        ComboAttackParameterHash = Animator.StringToHash(comboAttackParameterName);
        MoveAttackParameterHash = Animator.StringToHash(moveAttackParameterName);
        AirAttackParameterHash = Animator.StringToHash(airAttackParameterName);
        StrongAttackParameterHash = Animator.StringToHash(strongAttackParameterName);
        AttackTriggerParameterHash = Animator.StringToHash(attackTriggerParameterName);

        ActionParameterHash = Animator.StringToHash(actionParameterName);
        DefenseParameterHash = Animator.StringToHash(defenseParameterName);
        ParryParameterHash = Animator.StringToHash(parryParameterName);
        DodgeParameterHash = Animator.StringToHash(dodgeParameterName);
        DashParameterHash = Animator.StringToHash(dashParameterName);

        HitParameterHash = Animator.StringToHash(hitParameterName);
        NormalHitParameterHash = Animator.StringToHash(normalHitParameterName);
        StrongHitParameterHash = Animator.StringToHash(strongHitParameterName);
        NormalKnockbackParameterHash = Animator.StringToHash(normalKnockbackParameterName);
        StrongKnockbackParameterHash = Animator.StringToHash(strongKnockbackParameterName);
        
        DeadParameterHash = Animator.StringToHash(deadParameterName);
    }
}