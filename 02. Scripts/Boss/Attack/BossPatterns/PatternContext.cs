using UnityEngine;

public class PatternContext
{
    #region 내부 필드값 세팅
    private IUnitController _unit;
    private Animator _animator;
    private BossController _boss;
    private ProjectilePool _projectilePool;
    private Transform _firePoint;
    private SummonHandler _summonHandler;
    private SkillSO _skill;
    #endregion

    #region 외부에 노출할 프로퍼티들
    public IUnitController Unit
    {
        get { return _unit; }
    }

    public IDamageable Target
    {
        get { return _boss.Target; }
    }

    public Animator Animator
    {
        get { return _animator; }
    }

    public BossController Boss
    {
        get { return _boss; }
    }

    public ProjectilePool ProjectilePool
    {
        get { return _projectilePool; }
    }

    public Transform FirePoint
    {
        get { return _firePoint; }
    }

    public SummonHandler SummonHandler
    {
        get { return _summonHandler; }
    }

    public SkillSO Skill
    {
        get { return _skill; }
    }
    #endregion

    #region 생성자
    public PatternContext (IUnitController unit, Animator animator, BossController boss, ProjectilePool projectilePool, Transform firePoint, SummonHandler summonHandler, SkillSO skill)
    {
        _unit = unit; 
        _animator = animator;
        _boss = boss;
        _projectilePool = projectilePool;
        _firePoint = firePoint;
        _summonHandler = summonHandler;
        _skill = skill;
    }
    #endregion
}
