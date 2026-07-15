using System;
using System.Collections.Generic; // Ajouté pour la détection
using UnityEngine;

public class OeilTower : Tower
{
    private bool _wasDestroyedLastFrame;

    protected virtual void Start()
    {
        _wasDestroyedLastFrame = _isDestroyed;
    }

    protected override void Update()
    {
        base.Update();

        // Si l'œil est réparé, il punit immédiatement les ennemis présents dans son périmètre !
        if (_wasDestroyedLastFrame && !_isDestroyed)
        {
            StunEnemiesAlreadyInside();
        }
        _wasDestroyedLastFrame = _isDestroyed;
    }

    private void StunEnemiesAlreadyInside()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        Collider2D rangeTrigger = null;

        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                rangeTrigger = col;
                break;
            }
        }

        if (rangeTrigger != null)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;

            List<Collider2D> results = new List<Collider2D>();
            int count = rangeTrigger.Overlap(filter, results);

            for (int i = 0; i < count; i++)
            {
                if (results[i].CompareTag("Boss"))
                {
                    if (results[i].TryGetComponent<BossController>(out var enemy))
                    {
                        enemy.Stun(towerData.stunDuration);
                        Debug.Log($"[OeilTower] Réparée ! Ennemi étourdi à la volée : {enemy.name}");
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDestroyed) return; // Sécurité si la tour est détruite

        if (collision.CompareTag("Boss"))
        {
            if (collision.TryGetComponent<BossController>(out var enemy))
            {
                enemy.Stun(towerData.stunDuration);
            }
        }
    }
}