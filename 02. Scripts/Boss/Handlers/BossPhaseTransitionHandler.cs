using System.Collections;
using UnityEngine;

[RequireComponent (typeof(PhaseManager))]
public class BossPhaseTransitionHandler : MonoBehaviour
{
    #region 필드 세팅
    [Header("Phase 2 연출 이펙트")]
    [SerializeField] private Phase2TransitionEffects transitionEffects;
    [SerializeField] private float airStrikeDelay = 0.5f;

    #endregion

    #region 내부 컴포넌트
    private PhaseManager _phaseManager;
    private BossController _boss;
    private BossAudioHandler _audioHandler;
    private BossVisualHandler _visualHandler;

    #endregion

    #region 초기화
    private void Awake()
    {
        _phaseManager = GetComponent<PhaseManager>();
        _boss = GetComponent<BossController>();
        _audioHandler = _boss.GetComponent<BossAudioHandler>();
        _visualHandler = _boss.GetComponentInChildren<BossVisualHandler>();
    }

    #endregion

    #region 페이즈2 진입 모니터링
    public void StartPhase2()
    {
        //  이미 실행된 적이 있다면 무시하기
        if (_phaseManager.HasTriggeredPhase2)
        {
            return;
        }

        _phaseManager.HasTriggeredPhase2 = true;

        StartCoroutine(PlayPhase2Sequence());
    }
    #endregion

    #region 연출 코루틴
    private IEnumerator PlayPhase2Sequence()
    {
        //  사운드
        _audioHandler?.PlayPhase2();

        //  전환 이펙트
        if (transitionEffects != null)
        {
            yield return transitionEffects.Play();
        }

        //  잠깐 대기
        if (airStrikeDelay > 0f)
        {
            yield return new WaitForSeconds(airStrikeDelay);
        }

        //  보스 쪽에 Phase2 시작했음을 알리기
        _boss.OnEnterPhase2();
    }
    #endregion
}
