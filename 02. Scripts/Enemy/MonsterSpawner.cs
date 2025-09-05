using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public string poolName = "MeleeMonsterPool";
        public GameObject monsterPrefab; // 풀에 없을 경우 사용할 프리팹
        public int maxSpawnCount = 1; // 기본값을 1로 변경 (한 번만 스폰)
        public float spawnDelay = 2f;
        public Transform[] spawnPoints;
        [Header("한 번만 스폰 설정")]
        public bool spawnOnlyOnce = true; // 한 번만 스폰할지 여부
        [HideInInspector]
        public bool hasSpawned = false; // 이미 스폰했는지 추적
    }
    
    [Header("스폰 설정")]
    public List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
    public bool autoSpawn = true;
    public float initialDelay = 1f;
    
    [Header("스폰 제한")]
    public int maxTotalMonsters = 20; // 전체 최대 몬스터 수
    public bool spawnInWaves = false; // 웨이브 방식 스폰
    public float waveInterval = 30f; // 웨이브 간격
    public int monstersPerWave = 10; // 웨이브당 몬스터 수
    
    // 스폰 상태
    private Dictionary<string, int> currentSpawnCounts = new Dictionary<string, int>();
    private bool isSpawning = false;
    private int totalMonstersSpawned = 0;
    private int currentWave = 0;
    
    private void Start()
    {
        // 각 풀의 현재 스폰 수 초기화
        foreach (var spawnInfo in spawnInfos)
        {
            currentSpawnCounts[spawnInfo.poolName] = 0;
            // 한 번만 스폰 옵션이 켜져있으면 maxSpawnCount를 1로 설정
            if (spawnInfo.spawnOnlyOnce)
            {
                spawnInfo.maxSpawnCount = 1;
            }
        }
        
        // 자동 스폰 시작
        if (autoSpawn)
        {
            StartCoroutine(AutoSpawnRoutine());
        }
    }
    
    private IEnumerator AutoSpawnRoutine()
    {
        yield return new WaitForSeconds(initialDelay);
        
        isSpawning = true;
        
        if (spawnInWaves)
        {
            // 웨이브 방식 스폰 (한 번만 스폰이면 첫 웨이브만)
            if (HasAnyUnspawnedMonsters())
            {
                yield return StartCoroutine(SpawnWave());
            }
        }
        else
        {
            // 연속 스폰 (각 몬스터를 한 번씩만)
            foreach (var spawnInfo in spawnInfos)
            {
                if (CanSpawn(spawnInfo))
                {
                    SpawnMonster(spawnInfo);
                    yield return new WaitForSeconds(spawnInfo.spawnDelay);
                }
            }
        }
        
        isSpawning = false;
        //Debug.Log("All monsters spawned once. Spawning complete.");
    }
    
    // 아직 스폰되지 않은 몬스터가 있는지 확인
    private bool HasAnyUnspawnedMonsters()
    {
        foreach (var spawnInfo in spawnInfos)
        {
            if (spawnInfo.spawnOnlyOnce && !spawnInfo.hasSpawned)
            {
                return true;
            }
            else if (!spawnInfo.spawnOnlyOnce && currentSpawnCounts[spawnInfo.poolName] < spawnInfo.maxSpawnCount)
            {
                return true;
            }
        }
        return false;
    }
    
    private IEnumerator SpawnWave()
    {
        currentWave++;
        //Debug.Log($"Starting Wave {currentWave} (One-time spawn)");
        
        // 모든 스폰 정보를 한 번씩 처리
        foreach (var spawnInfo in spawnInfos)
        {
            if (CanSpawn(spawnInfo))
            {
                SpawnMonster(spawnInfo);
                yield return new WaitForSeconds(0.5f); // 각 스폰 사이의 간격
            }
        }
        
        //Debug.Log($"Wave {currentWave} complete! All monsters spawned once.");
    }
    
    private bool CanSpawn(SpawnInfo spawnInfo)
    {
        // 한 번만 스폰 옵션이 켜져있고 이미 스폰했다면 false
        if (spawnInfo.spawnOnlyOnce && spawnInfo.hasSpawned)
        {
            return false;
        }
        
        // 전체 몬스터 수 체크
        if (GetTotalActiveMonsters() >= maxTotalMonsters)
        {
            return false;
        }
            
        // 개별 풀 최대 수 체크
        if (currentSpawnCounts[spawnInfo.poolName] >= spawnInfo.maxSpawnCount)
        {
            return false;
        }
            
        // 스폰 포인트 체크
        if (spawnInfo.spawnPoints == null || spawnInfo.spawnPoints.Length == 0)
        {
            return false;
        }
            
        return true;
    }
    
    private void SpawnMonster(SpawnInfo spawnInfo)
    {
        // 랜덤 스폰 위치 선택
        Transform spawnPoint = spawnInfo.spawnPoints[Random.Range(0, spawnInfo.spawnPoints.Length)];
        
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is null!");
            return;
        }
        
        // 오브젝트 풀에서 몬스터 가져오기
        GameObject monster = MonsterObjectPool.Instance.GetMonster(
            spawnInfo.poolName,
            spawnPoint.position,
            spawnPoint.rotation
        );
        
        // 풀에서 가져오지 못한 경우 프리팹으로 직접 생성 시도
        if (monster == null && spawnInfo.monsterPrefab != null)
        {
            Debug.LogWarning($"Failed to get monster from pool {spawnInfo.poolName}, creating new instance");
            
            // 풀에 해당 정보 추가
            var poolInfo = new MonsterObjectPool.PoolInfo
            {
                poolName = spawnInfo.poolName,
                prefab = spawnInfo.monsterPrefab,
                poolSize = 10,
                canExpand = true
            };
            
            MonsterObjectPool.Instance.AddPool(poolInfo);
            
            // 다시 시도
            monster = MonsterObjectPool.Instance.GetMonster(
                spawnInfo.poolName,
                spawnPoint.position,
                spawnPoint.rotation
            );
        }
        
        if (monster != null)
        {
            // 풀 이름 설정
            EnemyController enemyController = monster.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.poolName = spawnInfo.poolName;
            }
            
            // 스폰 카운트 증가
            currentSpawnCounts[spawnInfo.poolName]++;
            totalMonstersSpawned++;
            
            // 한 번만 스폰 플래그 설정
            if (spawnInfo.spawnOnlyOnce)
            {
                spawnInfo.hasSpawned = true;
                //Debug.Log($"Monster from {spawnInfo.poolName} spawned once at {spawnPoint.position}");
            }
            
            // 사망 이벤트 구독
            if (enemyController != null)
            {
                EnemyController.OnAnyEnemyDie += OnMonsterDie;
            }
            
            // 스폰 이펙트 (선택사항)
            PlaySpawnEffect(spawnPoint.position);
        }
    }
    
    private void OnMonsterDie(EnemyController enemy)
    {
        // 해당 풀의 카운트 감소
        if (!string.IsNullOrEmpty(enemy.poolName) && currentSpawnCounts.ContainsKey(enemy.poolName))
        {
            currentSpawnCounts[enemy.poolName]--;
            currentSpawnCounts[enemy.poolName] = Mathf.Max(0, currentSpawnCounts[enemy.poolName]);
        }
        
        // 이벤트 구독 해제
        EnemyController.OnAnyEnemyDie -= OnMonsterDie;
        
        // 한 번만 스폰 모드에서는 몬스터가 죽어도 다시 스폰하지 않음
        //Debug.Log($"Monster {enemy.name} died. One-time spawn mode: will not respawn.");
    }
    
    private int GetTotalActiveMonsters()
    {
        int total = 0;
        foreach (var count in currentSpawnCounts.Values)
        {
            total += count;
        }
        return total;
    }
    
    private void PlaySpawnEffect(Vector3 position)
    {
        // 스폰 이펙트 재생 (구현 필요)
        // 예: Instantiate(spawnEffectPrefab, position, Quaternion.identity);
    }
    
    // 공개 메서드들
    public void StartSpawning()
    {
        if (!isSpawning && HasAnyUnspawnedMonsters())
        {
            StartCoroutine(AutoSpawnRoutine());
        }
        else
        {
            //Debug.Log("All monsters already spawned or spawning in progress.");
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    public void SpawnMonsterManually(string poolName, Vector3 position)
    {
        var spawnInfo = spawnInfos.Find(s => s.poolName == poolName);
        if (spawnInfo != null)
        {
            // 한 번만 스폰 모드에서는 이미 스폰했으면 수동 스폰도 불가
            if (spawnInfo.spawnOnlyOnce && spawnInfo.hasSpawned)
            {
                Debug.LogWarning($"Monster {poolName} already spawned once. Manual spawn ignored.");
                return;
            }
            
            if (CanSpawn(spawnInfo))
            {
                GameObject monster = MonsterObjectPool.Instance.GetMonster(
                    poolName,
                    position,
                    Quaternion.identity
                );
                
                if (monster != null)
                {
                    EnemyController enemyController = monster.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        enemyController.poolName = poolName;
                    }
                    
                    currentSpawnCounts[poolName]++;
                    
                    // 한 번만 스폰 플래그 설정
                    if (spawnInfo.spawnOnlyOnce)
                    {
                        spawnInfo.hasSpawned = true;
                    }
                    
                    EnemyController.OnAnyEnemyDie += OnMonsterDie;
                }
            }
        }
    }
    
    // 모든 스폰 상태 리셋 (새 게임이나 재시작용)
    public void ResetSpawnStates()
    {
        foreach (var spawnInfo in spawnInfos)
        {
            spawnInfo.hasSpawned = false;
            currentSpawnCounts[spawnInfo.poolName] = 0;
        }
        totalMonstersSpawned = 0;
        currentWave = 0;
        isSpawning = false;
        
        //Debug.Log("Spawn states reset. Ready for new spawning cycle.");
    }
    
    // 특정 풀의 스폰 상태 확인
    public bool HasPoolSpawned(string poolName)
    {
        var spawnInfo = spawnInfos.Find(s => s.poolName == poolName);
        return spawnInfo != null && spawnInfo.hasSpawned;
    }
    
    // 모든 몬스터가 스폰되었는지 확인
    public bool AllMonstersSpawned()
    {
        return !HasAnyUnspawnedMonsters();
    }
    
    
    // 에디터에서 스폰 포인트 시각화
    private void OnDrawGizmos()
    {
        if (spawnInfos == null) return;
        
        foreach (var spawnInfo in spawnInfos)
        {
            if (spawnInfo.spawnPoints == null) continue;
            
            // 이미 스폰한 포인트는 빨간색, 아직 안 한 포인트는 초록색
            Gizmos.color = spawnInfo.hasSpawned ? Color.red : Color.green;
            
            foreach (var point in spawnInfo.spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
                }
            }
        }
    }
}