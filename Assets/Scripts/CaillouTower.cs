using UnityEngine;
using System.Collections.Generic;

public class CaillouTower : MonoBehaviour
{
    [SerializeField] private TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Animator animatorCaillouTower;



    private List<BossController> _enemiesInRange = new List<BossController>();
    private float _nextAttackTime;
    private bool _isDestroyed;

    private void OnEnable()
    {
        if (healthSystem != null) healthSystem.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (healthSystem != null) healthSystem.OnDeath -= OnDeath;
    }

    private void Update()
    {
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
            _enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<BossController>(out var enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
    }

    private void OnDeath()
    {
        _isDestroyed = true;
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;

        // Empêche la tour d'attaquer une fois détruite
        _enemiesInRange.Clear();
    }
}