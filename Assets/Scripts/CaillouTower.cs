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
        if (_isDestroyed) return;

        // On nettoie la liste des ennemis (si un ennemi meurt dans la zone)
        _enemiesInRange.RemoveAll(enemy => enemy == null);

        // Si on a une cible et qu'on peut attaquer
        if (_enemiesInRange.Count > 0 && Time.time >= _nextAttackTime)
        {
            animatorCaillouTower.SetTrigger("Attack");
        }
    }

    public void AE_AttackCaillou()
    {
        if (_isDestroyed) return;
        Attack(_enemiesInRange[0]);
    }

    private void Attack(BossController target)
    {
        _nextAttackTime = Time.time + towerData.attackSpeed; // Suppose un float fireRate dans ton SO

        GameObject proj = Instantiate(towerData.projectile, projectileSpawnPoint.position, Quaternion.identity);

        // Si ton projectile a un script de poursuite ou d'initialisation
        if (proj.TryGetComponent<Projectile>(out var p))
        {
            Vector2 dir = (target.transform.position - projectileSpawnPoint.position).normalized;
            p.Initialize(dir);
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