using UnityEngine;
using System.Collections.Generic;

public enum ProjectileType
{
    Bullet, 
    Firebomb, 
    Missile,
    Shelling
}

public class ProjectilePool : MonoBehaviour
{
    [Header("총알 풀")]
    public GameObject bulletPrefab;
    public int bulletPoolSize;

    [Header("TNT 풀")]
    public GameObject firebombPrefab;
    public int firebombPoolSize;

    [Header("미사일 풀 (정예 원거리 몹 전용)")]
    public GameObject missilePrefab;
    public int missilePoolSize;

    [Header("유탄 풀 (보스 2페이즈 전용 패턴)")]
    public GameObject shellingPrefab;
    public int shellingPoolSize;

    private Queue<GameObject> _bulletPool = new Queue<GameObject>();
    private Queue<GameObject> _firebombPool = new Queue<GameObject>();
    private Queue<GameObject> _missilePool = new Queue<GameObject>();
    private Queue<GameObject> _shellingPool = new Queue<GameObject>();

    private void Awake()
    {
        InitPool(bulletPrefab, bulletPoolSize, _bulletPool);
        InitPool(firebombPrefab, firebombPoolSize, _firebombPool);
        InitPool(missilePrefab, missilePoolSize, _missilePool);
        InitPool(shellingPrefab, shellingPoolSize, _shellingPool);
    }

    private void InitPool(GameObject prefab, int count, Queue<GameObject> pool)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(ProjectileType type)
    {
        Queue<GameObject> pool = GetPool(type);

        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        GameObject prefab = GetPrefab(type);
        return Instantiate(prefab, transform);
    }

    public void Return(GameObject obj, ProjectileType type)
    {
        obj.SetActive(false);
        GetPool(type).Enqueue(obj);
    }

    private Queue<GameObject> GetPool(ProjectileType type)
    {
        return type switch
        {
            ProjectileType.Bullet => _bulletPool,
            ProjectileType.Firebomb => _firebombPool,
            ProjectileType.Missile => _missilePool,
            ProjectileType.Shelling => _shellingPool,
            _=>_bulletPool
        };
    }
  

    private GameObject GetPrefab(ProjectileType type)
    {
        return type switch
        {
            ProjectileType.Bullet => bulletPrefab,
            ProjectileType.Firebomb => firebombPrefab,
            ProjectileType.Missile => missilePrefab,
            ProjectileType.Shelling => shellingPrefab,
            _ => bulletPrefab
        };
    }
}
