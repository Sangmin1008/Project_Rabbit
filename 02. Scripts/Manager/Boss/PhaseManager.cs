using System.Collections;
using UnityEngine;

//  보스 페이즈 관리 코드
[RequireComponent(typeof(BossController))]
[RequireComponent(typeof(BossPhaseTransitionHandler))]
public class PhaseManager : MonoBehaviour
{
    [Range(0f, 1f)][SerializeField] private float _threshold = 0.5f;

    private BossController _boss;
    private BossPhaseTransitionHandler _transitionHandler;
    private bool _entered;

    public bool IsPhase2
    {
        get { return _entered; }
    }

    public bool HasTriggeredPhase2 { get; set; } = false;

    private void Awake()
    {
        _boss = GetComponent<BossController>();

        _transitionHandler = GetComponent<BossPhaseTransitionHandler>();
    }

    // BossController.Update() 직전에 호출해서 페이즈2 진입 여부를 체크
    public void CheckPhase()
    {
        if (_entered)
        {
            return;
        }

        float curHp = _boss.StatManager.GetValueSafe(StatType.CurHp, 0f);
        float maxHp = _boss.StatManager.GetValueSafe(StatType.MaxHp, 1f);

        if (curHp <= maxHp * _threshold)
        {
            _entered = true;

            _transitionHandler.StartPhase2();
        }
    }
}
