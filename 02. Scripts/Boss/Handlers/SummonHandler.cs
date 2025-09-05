using UnityEngine;
using System.Collections;

public class SummonHandler
{
    private readonly MonoBehaviour _context;

    public SummonHandler(MonoBehaviour context)
    {
        _context = context;
    }

    public void Summon(SummonPatternData data, GameObject owner, Vector2 playerCenter)
    {
        if (data == null)
        {
            Debug.LogWarning("SummonHandler: 소환 데이터가 없음!");
            return;
        }

        if (data.summonPrefabs == null || data.summonPrefabs.Length == 0)
        {
            Debug.LogWarning("SummonHandler: 소환할 적 프리팹이 없거나 null임!");
            return;
        }

        _context.StartCoroutine(SummonCoroutine(data, owner, playerCenter));
    }

    private IEnumerator SummonCoroutine(SummonPatternData data, GameObject owner, Vector2 playerCenter)
    {

        //  페이즈 체크해서 min / max 결정하기
        var bossController = owner.GetComponent<BossController>();
        bool isPhase2 = bossController != null && bossController.IsPhase2;

        int minCount = isPhase2 ? data.summonCountMinPhase2 : data.summonCountMinPhase1;
        int maxCount = isPhase2 ? data.summonCountMaxPhase2 : data.summonCountMaxPhase1;

        int count = Random.Range(minCount, maxCount + 1);
        
        float groundY = data.spawnY;
        float height = data.spawnHeight;

        for (int i = 0; i < count; i++)
        {
            var prefab = data.summonPrefabs[Random.Range(0, data.summonPrefabs.Length)];
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(data.minDistance, data.summonRadius);

            float spawnX = playerCenter.x + offset.x;
            float spawnY = groundY;

            GameObject enemy = GameObject.Instantiate(prefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);

            if (enemy.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            }

            if (enemy.TryGetComponent<Rigidbody2D>(out var fallRb))
            {
                yield return WaitForGroundContact(fallRb, groundY);
            }

            SetupEnemyDefaults(enemy);

            if (enemy.TryGetComponent<ISummonedEnemy>(out var summoned))
            {
                summoned.Initialize(owner);
            }

            yield return new WaitForSeconds(data.summonInterval);
        }

    }

    private void SetupEnemyDefaults(GameObject enemy)
    {
        // Rigidbody 초기화 (중력 끄고 Y 고정)
        if (enemy.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        enemy.layer = LayerMask.NameToLayer("SummonedEnemy");
    }

    private IEnumerator WaitForGroundContact(Rigidbody2D rb, float groundY)
    {
        float timer = 0f, maxWait = 2f;

        while (timer < maxWait)
        {
            timer += Time.deltaTime;

            // Y 위치가 groundY 에 닿으면 종료
            if (rb.position.y <= groundY + 0.01f)
            {
                yield break;

            }

            yield return null;
        }
    }
}
