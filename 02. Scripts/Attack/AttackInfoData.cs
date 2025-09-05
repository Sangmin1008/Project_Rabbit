using System;
using UnityEngine;

[Serializable]
public enum AttackType
{
    None = 0,
    Light,
    Heavy,
    Knockback,
    Launch,
}

[Serializable]
public class AttackInfoData
{
    [field:SerializeField] public int ComboStateIndex { get; private set; }
    [field:SerializeField] public string AttackName { get; private set; }
    [field:SerializeField] public int ExtraDamage { get; private set; }
    [field:SerializeField] public AttackType AttackType  { get; private set; }
    [field:SerializeField][field:Range(0f, 2f)] public float ComboTransitionTime { get; private set; }
    [field:SerializeField][field:Range(0f, 2f)] public float DealingStartTransitionTime { get; private set; }
    [field:SerializeField][field:Range(0f, 2f)] public float DealingEndTransitionTime { get; private set; }
    [field:SerializeField] public Vector2 HitBoxCenter { get; private set; }
    [field:SerializeField] public Vector2 HitBoxSize { get; private set; }
    [field:SerializeField] public bool IsStrongAttack { get; private set; }
}