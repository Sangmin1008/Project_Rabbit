using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{ 
    #region 풀 설정
    [SerializeField] private EliteEnemyPool _pool;

    #endregion

    #region 웨이브 설정
    [Tooltip("웨이브당 몬스터 ID 배열")]
    [SerializeField] private int[] _spawnIDs;
    [Tooltip("각 몹 타입별 스폰 지연 시간(SpawnIDs와 길이 일치시킬 것)")]
    [SerializeField] private float[] _spawnDelays;
    [Tooltip("첫 웨이브 기본 크기")]
    [SerializeField] private int _baseWaveSize;
    [Tooltip("웨이브당 추가 증가치")]
    [SerializeField] private int _waveSizeStep;
    [Tooltip("웨이브 간 대기 시간")]
    [SerializeField] private float _respawnDelay;

    #endregion

    #region 스폰 퍼포먼스 분산 설정
    [Tooltip("타입별 배열이 비어 있을 때 사용할 기본 지연 시간")]
    [SerializeField] private float _defaultDelay;

    #endregion

    #region 겹침 방지 설정
    [Tooltip("스폰 충돌 검사에 사용할 레이어 마스크 (적 레이어)")]
    [SerializeField] private LayerMask _spawnMask;
    [Tooltip("흩어질 최대 반경")]
    [SerializeField] private float _scatterRadius;
    [Tooltip("몹 간 최소 거리")]
    [SerializeField] private float _clearance;

    #endregion

    #region 스폰 Y 고정 설정 & 오프셋
    [Tooltip("스폰될 고정 Y 좌표 (지면 높이)")]
    [SerializeField] private float _spawnY;
    [Tooltip("카메라 엣지에서 더 벗어날 X 오프셋")]
    [SerializeField] private float _spawnOffsetX;

    #endregion

    #region 내부 변수
    private int _currentWave;
    private int _aliveCount;
    private bool _started;

    #endregion

    #region 유니티 콜백
    private void Awake()
    {
        _pool = GetComponent<EliteEnemyPool>();

        if (_pool == null)
        {
            _pool = FindFirstObjectByType<EliteEnemyPool>();

            if (_pool == null)
            {
                Debug.LogError("[WaveManager] _pool이 할당되지 않았고, 씬에서도 EliteEnemyPool을 찾을 수 없습니다!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_started)
        {
            return;
        }

        if (collision.TryGetComponent<PlayerController>(out _))
        {
            _started = true;
            StartCoroutine(SpawnWave());
        }
    }

    #endregion

    #region 웨이브 스폰 로직
    private IEnumerator SpawnWave()
    {
        //  웨이브 시작 시점에만 이벤트 구독
        _pool.OnRelease += OnEnemyReleased;

        _currentWave++;

        int waveSize = _baseWaveSize + (_currentWave - 1) * _waveSizeStep;

        _aliveCount = waveSize;

        int spawned = 0;

        //  한 마리씩 순차적으로 스폰
        while (spawned < waveSize)
        {
            int typeIndex = spawned % _spawnIDs.Length;
            int id = _spawnIDs[typeIndex];

            //  카메라 엣지 + 오프셋
            Vector3 edgePos = GetOffscreenSpawnPosition();

            //  겹침 방지
            Vector3 spawnPos = FindNonOverlappingPosition(edgePos);

            //  스폰
            var enemy = _pool.Spawn(id, spawnPos);

            AdjustSpawnY(enemy.transform, _spawnY);

            spawned++;

            //  타입별 또는 기본 딜레이로 대기
            float delay = (typeIndex < _spawnDelays.Length) ? _spawnDelays[typeIndex] : _defaultDelay;

            yield return new WaitForSeconds(delay);
        }

        //  스폰한 모든 몹이 리턴될 때까지 대기
        while (_aliveCount > 0)
        {
            yield return null;
        }

        //  이 웨이브 전용 이벤트 구독 해제
        _pool.OnRelease -= OnEnemyReleased;

        //  이 트리거 오브젝트 비활성화
        gameObject.SetActive(false);
    }

    private void AdjustSpawnY(Transform transform, float spawnY)
    {
        var col = transform.GetComponent<Collider2D>();

        if (col != null)
        {
            float bottomY = col.bounds.min.y;
            float diff = spawnY - bottomY;
            transform.position += Vector3.up * diff;
        }

        else
        {
            // 콜라이더가 없으면 Pivot 기준으로 강제 고정
            transform.position = new Vector3(transform.position.x, spawnY, transform.position.z);
        }
    }

    #endregion

    #region 적 해제 콜백
    private void OnEnemyReleased(BaseEnemyController enemy)
    {
        --_aliveCount;
    }

    #endregion

    #region 카메라 엣지 위치 계산 메서드
    private Vector3 GetOffscreenSpawnPosition()
    {
        var cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        //  화면 우측 바깥 X 좌표 계산
        float x = cam.transform.position.x + halfWidth + _clearance;

        //  Y는 _spawnY 고정
        float y = _spawnY;

        return new Vector3(x, y, 0f);

    }
    #endregion

    #region 겹침 방지 위치 계산 메서드
    private Vector3 FindNonOverlappingPosition(Vector3 center)
    {
        for (int attempt = 0; attempt < _aliveCount; attempt++)
        {
            //  X만 랜덤, Y는 그대로
            float x = Random.Range(center.x - _scatterRadius, center.x + _scatterRadius);
            Vector2 cand = new Vector2(x, center.y);

            //  cand 반경 _clearance 내에 다른 콜라이더가 없으면 Ok
            if (Physics2D.OverlapCircle(cand, _clearance, _spawnMask) == null)
            {
                return new Vector3(cand.x, cand.y, center.z);
            }
        }

        //  모두 실패 시, center는 그대로
        return center;
        
    }
    #endregion
}
