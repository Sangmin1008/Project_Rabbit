using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySetActive : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemies;

    private void Awake()
    {
        foreach (var enemy in enemies)
        {
            enemy.SetActive(false);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var enemy in enemies)
        {
            enemy?.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.SetActive(false);
        }
    }
}
