using UnityEngine;

public class AnimationHandler
{
    private readonly Animator _animator;
    private readonly IUnitController _owner;

    public AnimationHandler(Animator animator, IUnitController owner)
    {
        _animator = animator;
        _owner = owner;
    }

    //  BlendTree용 속도 값 세팅
    public void SetSpeed(float speed)
    {
        if (_animator == null)
        {
            Debug.LogError("애니메이터 세팅 안됨!");
            return;
        }

        _animator.SetFloat("Speed", speed);
    }

    //  기본 공격
    public void PlayBasicAttack()
    {
        _animator.SetTrigger("BasicAttack");
    }

    //  패턴 공격
    public void PlayPattern(int index)
    {
        if (index == 0)
        {
            _animator.SetTrigger("Pattern1");
        }

        else if (index == 1)
        {
            _animator.SetTrigger("Pattern2");
        }

        else if (index == 2)
        {
            _animator.SetTrigger("Pattern3");
        }

        else if (index == 3)
        {
            _animator.SetTrigger("Pattern4");
        }
    }

    public void PlayDeath()
    {
        _animator.SetTrigger("Die");
    }

    public void ResetTriggers()
    {
        _animator.ResetTrigger("BasicAttack");
        _animator.ResetTrigger("Die");
    }
}
