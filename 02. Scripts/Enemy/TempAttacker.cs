// 임시 공격자 클래스 (투사체용)
public class TempAttacker : IAttackable
{
    public StatBase AttackStat { get; private set; }
    public IDamageable Target { get; set; }

    //  이 부분 수정 필요합니다!
    public AttackType AttackType => AttackType.None;

    public TempAttacker(float damage)
    {
        AttackStat = new CalculatedStat(StatType.AttackPow, damage);
    }
    
    public void Attack()
    {
        // 투사체는 직접 공격하지 않음
    }
} 