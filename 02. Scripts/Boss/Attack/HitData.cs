using UnityEngine;
using System;

[Serializable]
public class HitData
{
    [field: SerializeField] public int Damage { get; private set; }
    [field: SerializeField] public AttackType Type { get; private set; } //  넉백 / 런치 구분
    [field: SerializeField, Range(0f, 2f)] public float WindupTime { get; private set; }
    [field: SerializeField, Range(0f, 2f)] public float ActiveTime { get; private set; }
    [field: SerializeField] public float ProjectileSpeed { get; private set; }

    [field:SerializeField] public float Range { get; private set; }
    [field:SerializeField] public float Cooldown { get; private set; }

    [Header("원거리 전용")]
    [field : SerializeField] public int shotCount { get; private set; }
    [field:SerializeField] public float ShotInterval { get; private set; }
}
