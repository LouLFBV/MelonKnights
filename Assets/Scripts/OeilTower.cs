using System;
using UnityEngine;

public class OeilTower : Tower
{
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
