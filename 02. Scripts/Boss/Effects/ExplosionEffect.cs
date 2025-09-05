using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _animator.Play("ExplosionEffect", 0, 0f);
    }

    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
