using UnityEngine;

//  보스 타겟팅 핸들러
[RequireComponent(typeof(BossController))]
public class TargetingHandler : MonoBehaviour
{
    [SerializeField] private PlayerController _player; 
    private BossController _boss;

    public IDamageable Current { get; private set; }

    private void Awake()
    {
        _boss = GetComponent<BossController>();
    }

    //  매 프레임에 호출되어 타겟을 찾는다
    public void Refresh()
    {
        if (Current != null && !Current.IsDead)
        {
            return;
        }

        if (_player != null)
        { 
            Current = _player.GetComponent<IDamageable>();
        }
    }
}
