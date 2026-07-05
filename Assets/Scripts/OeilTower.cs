using System;
using UnityEngine;

public class OeilTower : MonoBehaviour
{
    [SerializeField] private TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;

    private void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnDeath;
        }
    }
    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnDeath;
        }
    }

    private void OnDeath()
    {
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            if (collision.TryGetComponent<BossController>(out var enemy))
            {
                enemy.Stun(towerData.stunDuration);
            }
        }
    }
}
