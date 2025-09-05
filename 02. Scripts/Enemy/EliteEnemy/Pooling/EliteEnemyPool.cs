using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class EliteEnemyPoolEntry
{
    public int ID;
    public BaseEnemyController Prefab;
    public int InitialCapacity;
}

public class EliteEnemyPool : MonoBehaviour
{
    [Header("풀 항목 목록")]
    [Tooltip("각 ID별 프리팹과 초기 개수 설정")]
    [SerializeField] private List<EliteEnemyPoolEntry> _entries;

    [Header("풀 전용 부모(비활성 오브젝트 보관용)")]
    [SerializeField] private Transform _poolRoot;

    [Header("스폰된 적 보관용 부모 (없으면 루트)")]
    [SerializeField] private Transform _spawnRoot;

    //  ID - Queue 매핑
    private Dictionary<int, Queue<BaseEnemyController>> _poolMap;

    public event Action<BaseEnemyController> OnRelease;

    private void Awake()
    {
        if (_poolRoot == null)
        {
            _poolRoot = transform;
        }

        _poolMap = new Dictionary<int, Queue<BaseEnemyController>>();

        foreach (var entry in _entries)
        {
            var queue = new Queue<BaseEnemyController>();

            for (int i = 0; i < entry.InitialCapacity; i++)
            {
                //  풀 전용 부모 아래에 생성
                var enemy = Instantiate(entry.Prefab, _poolRoot);
                enemy.gameObject.SetActive(false);
                enemy.SetPool(this);
                queue.Enqueue(enemy);
            }

            _poolMap.Add(entry.ID, queue);
        }
    }

    public BaseEnemyController Spawn(int id, Vector3 position)
    {
        if (!_poolMap.TryGetValue(id, out var queue))
        {
            throw new ArgumentException($"No pool entry for ID {id}");
        }

        BaseEnemyController enemy;

        if (queue.Count > 0)
        {
            enemy = queue.Dequeue();
        }

        else
        {
            //  풀에 남은게 없다면 새로 생성하기
            var entry = _entries.Find(x => x.ID == id);
            enemy = Instantiate(entry.Prefab, _poolRoot);
            enemy.SetPool(this);
            enemy.gameObject.SetActive(false);
        }

        //  부모 분리 혹은 spawnRoot로 이동
        if (_spawnRoot != null)
        {
            enemy.transform.SetParent(_spawnRoot, worldPositionStays: true);
        }

        else
        {
            enemy.transform.SetParent(null, worldPositionStays: true);
        }

        //  위치 지정 & 활성화
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);

        //  내부 상태 완전 초기화
        enemy.ResetForSpawn();

        return enemy;
    }

    public void Release(BaseEnemyController enemy)
    {
        if (enemy == null)
        {
            return;
        }

        OnRelease?.Invoke(enemy);

        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(_poolRoot, worldPositionStays: true);

        if (_poolMap.TryGetValue(enemy.EnemyTypeID, out var q))
        {
            q.Enqueue(enemy);
        }

        else
        {
            Destroy(enemy.gameObject);
        }
    }
}
