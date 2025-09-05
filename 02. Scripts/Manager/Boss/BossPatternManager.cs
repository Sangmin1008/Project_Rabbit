using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//  보스의 패턴 전환을 책임지는 Manager
[RequireComponent(typeof(BossController))]
public class BossPatternManager : MonoBehaviour
{
    #region 인스펙터 설정
    [Header("Phase 1에서 사용할 패턴 인덱스")]
    [SerializeField] private List<int> phase1Patterns = new List<int> { 0, 1, 2, 3 };

    [Header("Phase 2에서 사용할 패턴 인덱스 (기본)")]
    [SerializeField] private List<int> phase2Patterns = new List<int> { 0, 1, 2, 3 };
    #endregion

    #region 내부 상태
    private BossSO _data;
    private BossController _boss;
    private int _lastPatternIndex = -1;
    public int CurrentPatternIndex { get; private set; }

    // Phase2 직후 쉘링 패턴 삽입용 플래그
    private bool _phase2Pending;
    private bool _phase2Executed;
    private int _shellingPatternIndex;

    // 현재 패턴이 실행 중인지
    private bool _isPatternExecuting;
    #endregion

    #region 초기화
    private void Awake()
    {
        _boss = GetComponent<BossController>();
    }

    // <summary>BossController.Start() 직후에 호출하세요.</summary>
    public void Initialize(BossSO so)
    {
        _data = so;
        CurrentPatternIndex = 0;
        _lastPatternIndex = -1;
        _phase2Pending = false;
        _phase2Executed = false;
        _isPatternExecuting = false;

        // 쉘링 패턴 데이터가 몇 번째 SO인지 미리 찾기
        _shellingPatternIndex = System.Array.FindIndex(_data.patterns, p => p.patternData is ShellingPatternData);
    }
    #endregion

    #region 페이즈2 알림
    //  PhaseManager에서 페이즈2 진입 시 호출
    public void NotifyPhase2Pending()
    {
        _phase2Pending = true;
    }
    #endregion

    #region 패턴 선택
    public void PickNextPatternIndex()
    {
        // 이전 패턴이 아직 종료되지 않았다면 대기
        if (_isPatternExecuting)
        {
            return;
        }

        // Phase2 직후, 아직 쉘링 패턴을 실행하지 않았다면 삽입
        if (_boss.IsPhase2 && _phase2Pending && !_phase2Executed)
        {
            CurrentPatternIndex = _shellingPatternIndex;
            _lastPatternIndex = _shellingPatternIndex;
            _phase2Executed = true;
            _phase2Pending = false;

            return;
        }

        // 일반 패턴 풀에서 랜덤 선택
        var pool = _boss.IsPhase2 ? phase2Patterns : phase1Patterns;
        pool = pool.Where(i => i >= 0 && i < _data.patterns.Length).ToList();

        var candidates = pool.Count > 1 ? pool.Where(i => i != _lastPatternIndex).ToList() : pool;

        if (candidates.Count == 0)
        {
            Debug.LogError("[BossPatternManager] 후보 패턴 없음. 0번으로 강제 설정");

            CurrentPatternIndex = 0;
            _lastPatternIndex = 0;

            return;
        }

        int next = candidates[Random.Range(0, candidates.Count)];

        CurrentPatternIndex = next;
        _lastPatternIndex = next;
    }
    #endregion

    #region 패턴 실행 플래그
    // BasePatternState.OnEnter(true) / OnExit(false) 에서 호출됩니다.
    public void SetPatternExecuting(bool executing)
    {
        _isPatternExecuting = executing;
    }
    #endregion
}
