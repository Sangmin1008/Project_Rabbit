using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EffectController : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Transform effectTransform;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Vector2 offset = new Vector2(0, 0);
    
    private Vector2 _flipScale = new Vector2(-1, 1);
    private Vector2 _normalScale = new Vector2(1, 1);
    
    private Queue<GameObject> pool = new Queue<GameObject>();
    
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject dustEffect = Instantiate(effectPrefab, effectTransform);
            dustEffect.SetActive(false);
            var handler = dustEffect.AddComponent<DustEffectHandler>();
            handler.Init(ReturnToPool);
            pool.Enqueue(dustEffect);
        }
    }

    public void Play(Vector2 position, bool isFlipX)
    {
        if (pool.Count == 0)
        {
            Debug.LogError("풀 사이즈 늘리셈");
            return;
        }

        position.y += offset.y;
        position.x += offset.x * (isFlipX ? -1 : 1);
        
        GameObject dustEffect = pool.Dequeue();
        dustEffect.transform.localScale = _normalScale;
        dustEffect.transform.position = position;

        dustEffect.transform.parent = null;
        
        dustEffect.SetActive(true);
    }

    private void ReturnToPool(GameObject dustEffect)
    {
        dustEffect.SetActive(false);
        dustEffect.transform.parent = this.transform;
        pool.Enqueue(dustEffect);
    }
}
