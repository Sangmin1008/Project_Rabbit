using UnityEngine;

public enum BossState
{
    Idle = 0,
    Chasing = 1,
    Attack= 2,
    Pattern1= 3,   //  화염병
    Pattern2= 4,   //  연사
    Pattern3= 5,   //  돌진
    Pattern4 =6,   //  소환
    Pattern5 = 7,
    Evade =8,      //  회피 상태
    Die = 9
}
