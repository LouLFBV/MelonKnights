using UnityEngine;
using System.Collections.Generic;

public class CaillouTower : Tower
{
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Animator animatorCaillouTower;

    private List<BossController> _enemiesInRange = new List<BossController>();
    private float _nextAttackTime;

    // Nouvelle variable pour détecter le moment précis de la réparation
    private bool _wasDestroyedLastFrame;

    protected virtual void Start()
    {
        // On initialise notre tracker avec l'état de départ de la tour
        _wasDestroyedLastFrame = _isDestroyed;
    }

    protected override void Update()
    {
        base.Update();

        // 1. Détection de la réparation : on était détruit la frame d'avant, et on ne l'est plus maintenant !
        if (_wasDestroyedLastFrame && !_isDestroyed)
        {
            ScanForEnemiesAlreadyInside();
        }
        _wasDestroyedLastFrame = _isDestroyed;

        if (_isDestroyed)
            return;

        _enemiesInRange.RemoveAll(enemy => enemy == null);

        if (_enemiesInRange.Count == 0)
            return;

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + towerData.attackSpeed;
            animatorCaillouTower.SetTrigger("Attack");
        }
    }

    // --- NOUVELLE MÉTHODE DE SCAN DE SÉCURITÉ ---
    private void ScanForEnemiesAlreadyInside()
    {
        _enemiesInRange.Clear();

        // On cherche spécifiquement le trigger de portée de notre tour
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

        // Si on a un trigger, on regarde ce qui est déjà dedans
        if (rangeTrigger != null)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true; // Indispensable pour scanner à travers un trigger

            List<Collider2D> results = new List<Collider2D>();
            int count = rangeTrigger.Overlap(filter, results);

            for (int i = 0; i < count; i++)
            {
                if (results[i].TryGetComponent<BossController>(out var enemy))
                {
                    if (!_enemiesInRange.Contains(enemy))
                    {
                        _enemiesInRange.Add(enemy);
                    }
                }
            }

            Debug.Log($"[CaillouTower] Réparée ! {_enemiesInRange.Count} ennemi(s) détecté(s) immédiatement dans la zone.");
        }
    }

    public void AE_AttackCaillou()
    {
        if (_isDestroyed)
            return;

        BossController target = GetClosestEnemy();

        if (target != null)
        {
            Attack(target);
        }
    }

    private BossController GetClosestEnemy()
    {
        BossController closest = null;
        float bestDistance = Mathf.Infinity;

        foreach (BossController enemy in _enemiesInRange)
        {
            if (enemy == null)
                continue;

            float distance = (enemy.transform.position - transform.position).sqrMagnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private void Attack(BossController target)
    {
        if (target == null)
            return;

        GameObject proj = Instantiate(
            towerData.projectile,
            projectileSpawnPoint.position,
            Quaternion.identity);

        if (proj.TryGetComponent(out Projectile projectile))
        {
            Vector2 dir = (target.transform.position - projectileSpawnPoint.position).normalized;
            projectile.Initialize(dir);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDestroyed) return;

        if (collision.TryGetComponent<BossController>(out var enemy))
        {
            if (!_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<BossController>(out var enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        // Empêche la tour d'attaquer une fois détruite
        _enemiesInRange.Clear();
    }
}