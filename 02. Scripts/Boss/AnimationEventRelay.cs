using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private BossController _boss;
    private AttackHandler _attackHandler;

    //  애니메이션 한 사이클 당 한 번만 불러주기 위한 플래그 변수
    private bool _fired = false;

    private void Awake()
    {
        _boss = GetComponentInParent<BossController>();
    }

    private void Start()
    {
        if (_boss != null)
        {
            _attackHandler = _boss.GetComponent<AttackHandler>();

            if (_attackHandler == null)
            {
                Debug.LogError("[AnimationEventRelay] AttackHandler가 BossController에 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[AnimationEventRelay] BossController를 찾을 수 없습니다!");
        }
    }

    #region 기본 공격 이벤트
    public void OnBasicAttackEvent()
    {
        if (_attackHandler?.LastAttackContext != null)
        {
            new BaseAttackPattern().Fire(_attackHandler.LastAttackContext);
        }
        else
        {
            Debug.LogWarning("[AnimationEventRelay] 기본 공격 context가 null입니다!");
        }
    }

    #endregion
}
