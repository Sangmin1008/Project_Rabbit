using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BossController))]
public class AttackHandler : MonoBehaviour
{
    private IUnitController _unit;
    private Animator _animator;
    private BossController _boss;
    private ProjectilePool _projectilePool;
    private Transform _firePoint;
    private SummonHandler _summonHandler;

    // context 캐싱 (애니메이션 이벤트용)
    public PatternContext LastAttackContext { get; private set; }

    public void Initialize(IUnitController unit, Animator animator, BossController boss, ProjectilePool projectilePool, Transform firePoint)
    {
        _unit = unit;
        _animator = animator;
        _boss = boss;
        _projectilePool = projectilePool;
        _firePoint = firePoint;
        _summonHandler = new SummonHandler(boss);
    }

    // 기본 공격 (애니메이션 & 이벤트)
    public IEnumerator BasicAttack()
    {
        LastAttackContext = new PatternContext(_unit, _animator, _boss, _projectilePool, _firePoint, _summonHandler, null);

        var audio = _boss.GetComponent<BossAudioHandler>();
        audio?.PlayAttack();

        _animator.Play("Boss_BasicAttack");

        // 기본 공격 후딜 (애니메이션 길이에 따라 조정)
        yield return new WaitForSeconds(_boss.Data.basicAttackPostDelay);

        if (_unit is IHasGauge gaugeUnit)           //  기본 공격 시 게이지 충전
        {
            gaugeUnit.AddBasicGauge();
        }
    }

    // 패턴 공격 실행 (패턴별로 분기)
    public IEnumerator ExecutePattern(SkillSO skill)
    {
        if (skill == null)
        {
            yield break;
        }

        LastAttackContext = new PatternContext(_unit, _animator, _boss, _projectilePool, _firePoint, _summonHandler, skill);

        switch (skill.type)
        {
            case SkillType.Fireball:
                yield return new TNTPattern().Execute(LastAttackContext); 
                break;
            case SkillType.Shoot:
                yield return new ShootPattern().Execute(LastAttackContext); 
                break;
            case SkillType.Charge:
                yield return new ChargePattern().Execute(LastAttackContext); 
                break;
            case SkillType.Summon:
                yield return new SummonPattern().Execute(LastAttackContext);
                break;
            case SkillType.Shelling:
                yield return new ShellingPattern().Execute(LastAttackContext);
                break;
            default: yield break;
        }
    }
}