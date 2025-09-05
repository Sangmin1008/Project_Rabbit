using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonsterObjectPool : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static MonsterObjectPool _instance;
    public static MonsterObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MonsterObjectPool>();
                if (_instance == null)
                {
                    GameObject poolObject = new GameObject("MonsterObjectPool");
                    _instance = poolObject.AddComponent<MonsterObjectPool>();
                }
            }
            return _instance;
        }
    }
    
    [System.Serializable]
    public class PoolInfo
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize = 10;
        public bool canExpand = true;
    }
    
    [Header("몬스터 풀 설정")]
    public List<PoolInfo> poolInfos = new List<PoolInfo>();
    
    // 풀 딕셔너리
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, PoolInfo> poolInfoDictionary = new Dictionary<string, PoolInfo>();
    private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();
    
    // 프리팹별 원본 scale 저장
    private Dictionary<string, Vector3> originalScales = new Dictionary<string, Vector3>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePools();
    }
    
    private void InitializePools()
    {
        foreach (var poolInfo in poolInfos)
        {
            CreatePool(poolInfo);
        }
    }
    
    private void CreatePool(PoolInfo poolInfo)
    {
        if (string.IsNullOrEmpty(poolInfo.poolName) || poolInfo.prefab == null)
        {
            Debug.LogWarning("Pool info is invalid!");
            return;
        }
        
        // 프리팹의 원본 scale 저장
        originalScales[poolInfo.poolName] = poolInfo.prefab.transform.localScale;
        //Debug.Log($"Pool {poolInfo.poolName} original scale: {originalScales[poolInfo.poolName]}");
        
        // 부모 오브젝트 생성
        GameObject parentObject = new GameObject($"Pool_{poolInfo.poolName}");
        parentObject.transform.SetParent(transform);
        poolParents[poolInfo.poolName] = parentObject.transform;
        
        // 큐 생성
        Queue<GameObject> objectPool = new Queue<GameObject>();
        
        // 초기 오브젝트 생성
        for (int i = 0; i < poolInfo.poolSize; i++)
        {
            GameObject obj = CreateNewObject(poolInfo.poolName, poolInfo.prefab);
            objectPool.Enqueue(obj);
        }
        
        poolDictionary[poolInfo.poolName] = objectPool;
        poolInfoDictionary[poolInfo.poolName] = poolInfo;
    }
    
    private GameObject CreateNewObject(string poolName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.name = $"{prefab.name}_{poolName}";
        obj.SetActive(false);
        
        if (poolParents.ContainsKey(poolName))
        {
            obj.transform.SetParent(poolParents[poolName]);
        }
        
        // 생성 직후 원본 scale 확인
        if (originalScales.ContainsKey(poolName))
        {
            obj.transform.localScale = originalScales[poolName];
        }
        
        return obj;
    }
    
    // 몬스터 가져오기
    public GameObject GetMonster(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool with name {poolName} doesn't exist!");
            return null;
        }
        
        GameObject objectToSpawn = null;
        
        // 사용 가능한 오브젝트 찾기
        if (poolDictionary[poolName].Count > 0)
        {
            objectToSpawn = poolDictionary[poolName].Dequeue();
        }
        else if (poolInfoDictionary[poolName].canExpand)
        {
            // 풀 확장
            objectToSpawn = CreateNewObject(poolName, poolInfoDictionary[poolName].prefab);
            //Debug.Log($"Pool {poolName} expanded!");
        }
        else
        {
            Debug.LogWarning($"Pool {poolName} is empty and cannot expand!");
            return null;
        }
        
        // 오브젝트 위치 설정 (활성화 전에)
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        
        // poolName 설정 (활성화 전에 설정하여 OnEnable에서 사용 가능하도록)
        EnemyController enemyController = objectToSpawn.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.poolName = poolName;
        }
        
        // 오브젝트 활성화
        objectToSpawn.SetActive(true);
        
        // 컴포넌트들이 Start/OnEnable을 실행할 시간을 주기 위해 다음 프레임에 초기화
        StartCoroutine(ResetMonsterNextFrame(objectToSpawn));
        
        return objectToSpawn;
    }
    
    // 다음 프레임에 몬스터 초기화
    private System.Collections.IEnumerator ResetMonsterNextFrame(GameObject monster)
    {
        yield return null; // 한 프레임 대기
        ResetMonster(monster);
    }
    
    // 몬스터 프리팹으로 가져오기 (편의 메서드)
    public GameObject GetMonsterByPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 프리팹 이름으로 풀 찾기
        var poolInfo = poolInfos.FirstOrDefault(p => p.prefab == prefab);
        if (poolInfo != null)
        {
            return GetMonster(poolInfo.poolName, position, rotation);
        }
        
        Debug.LogWarning($"No pool found for prefab {prefab.name}!");
        return null;
    }
    
    // 몬스터 반환
    public void ReturnMonster(GameObject monster, string poolName)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool with name {poolName} doesn't exist! Destroying object.");
            Destroy(monster);
            return;
        }
        
        // poolName 유지 (나중에 다시 사용할 때를 위해)
        EnemyController enemyController = monster.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.poolName = poolName;
        }
        
        monster.SetActive(false);
        
        if (poolParents.ContainsKey(poolName))
        {
            monster.transform.SetParent(poolParents[poolName]);
        }
        
        poolDictionary[poolName].Enqueue(monster);
    }
    
    // 몬스터 초기화
    private void ResetMonster(GameObject monster)
    {
        // EnemyController 초기화
        EnemyController enemyController = monster.GetComponent<EnemyController>();
        string poolName = "";
        
        if (enemyController != null)
        {
            poolName = enemyController.poolName;
            
            // StatManager가 초기화되었는지 확인
            if (enemyController.StatManager != null)
            {
                // 체력 회복 - SetValue 대신 Recover 사용
                float maxHp = enemyController.StatManager.GetValueSafe(StatType.MaxHp, 100f);
                float currentHp = enemyController.StatManager.GetValueSafe(StatType.CurHp, 0f);
                float healAmount = maxHp - currentHp;
                if (healAmount > 0)
                {
                    enemyController.StatManager.Recover(StatType.CurHp, StatModifierType.Base, healAmount);
                }
            }
            
            // 죽음 상태 초기화
            var isDead = typeof(EnemyController).GetField("_isDead", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isDead != null)
            {
                isDead.SetValue(enemyController, false);
            }
            
            // 상태 초기화 - 리플렉션을 통해 protected 메서드 호출
            var changeStateMethod = typeof(EnemyController).BaseType.GetMethod("ChangeState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (changeStateMethod != null)
            {
                changeStateMethod.Invoke(enemyController, new object[] { EnemyState.Idle });
            }
            
            enemyController.IsAttacking = false;
            enemyController.CanAttack = true;
        }
        
        // MonsterBase 초기화 (호환성)
        MonsterBase monsterBase = monster.GetComponent<MonsterBase>();
        if (monsterBase != null)
        {
            monsterBase.currentHealth = monsterBase.maxHealth;
            monsterBase.currentState = MonsterBase.MonsterState.Idle;
            
            // 프라이빗 필드 초기화
            var isDead = typeof(MonsterBase).GetField("isDead", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isDead != null)
            {
                isDead.SetValue(monsterBase, false);
            }
            
            var isAttacking = typeof(MonsterBase).GetField("isAttacking", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isAttacking != null)
            {
                isAttacking.SetValue(monsterBase, false);
            }
            
            var canAttack = typeof(MonsterBase).GetField("canAttack", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (canAttack != null)
            {
                canAttack.SetValue(monsterBase, true);
            }
        }
        
        // 컴포넌트 재활성화
        Collider2D collider = monster.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        // 스프라이트 초기화
        SpriteRenderer spriteRenderer = monster.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            
            // EnemyController의 원본 크기 사용
            if (enemyController != null)
            {
                Vector3 enemyOriginalScale = enemyController.GetOriginalScale();
                if (enemyOriginalScale != Vector3.zero)
                {
                    monster.transform.localScale = enemyOriginalScale;
                    //Debug.Log($"Monster {monster.name} scale restored to EnemyController original: {enemyOriginalScale}");
                }
                else if (!string.IsNullOrEmpty(poolName) && originalScales.ContainsKey(poolName))
                {
                    // EnemyController의 원본 크기가 없으면 풀의 원본 크기 사용
                    monster.transform.localScale = originalScales[poolName];
                    //Debug.Log($"Monster {monster.name} scale restored to pool original: {originalScales[poolName]}");
                }
                else
                {
                    // 둘 다 없으면 기본값 사용
                    monster.transform.localScale = Vector3.one;
                    Debug.LogWarning($"Original scale not found for {monster.name}, using Vector3.one");
                }
            }
            else if (!string.IsNullOrEmpty(poolName) && originalScales.ContainsKey(poolName))
            {
                // EnemyController가 없으면 풀의 원본 크기 사용
                monster.transform.localScale = originalScales[poolName];
                //Debug.Log($"Monster {monster.name} scale restored to pool original: {originalScales[poolName]}");
            }
            else
            {
                // 모두 없으면 기본값 사용
                monster.transform.localScale = Vector3.one;
                Debug.LogWarning($"Original scale not found for {monster.name}, using Vector3.one");
            }
        }
        
        // Rigidbody 초기화
        Rigidbody2D rb = monster.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // CharacterController 재활성화
        CharacterController characterController = monster.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // 애니메이터 초기화
        Animator animator = monster.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
    
    // 런타임에 새로운 풀 추가
    public void AddPool(PoolInfo poolInfo)
    {
        if (poolDictionary.ContainsKey(poolInfo.poolName))
        {
            Debug.LogWarning($"Pool {poolInfo.poolName} already exists!");
            return;
        }
        
        poolInfos.Add(poolInfo);
        CreatePool(poolInfo);
    }
    
    // 특정 풀의 활성화된 몬스터 수 가져오기
    public int GetActiveMonsterCount(string poolName)
    {
        if (!poolParents.ContainsKey(poolName))
            return 0;
            
        int count = 0;
        foreach (Transform child in poolParents[poolName])
        {
            if (child.gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }
    
    // 모든 몬스터 비활성화 (씬 전환 등에 사용)
    public void DeactivateAllMonsters()
    {
        foreach (var poolParent in poolParents.Values)
        {
            foreach (Transform child in poolParent)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        // 모든 큐 재구성
        foreach (var poolName in poolDictionary.Keys.ToList())
        {
            poolDictionary[poolName].Clear();
            
            if (poolParents.ContainsKey(poolName))
            {
                foreach (Transform child in poolParents[poolName])
                {
                    poolDictionary[poolName].Enqueue(child.gameObject);
                }
            }
        }
    }
}