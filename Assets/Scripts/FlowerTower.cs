using UnityEngine;
using System.Collections.Generic; // Ajouté pour la détection

public class FlowerTower : Tower
{
    [Header("Nuage Toxic")]
    [SerializeField] private Transform transformNuageToxic;
    [SerializeField] private Animator animatorFlowerTower;
    [SerializeField] private Animator animatorNuageToxic;

    private float _nextAttackTime;
    private Collider2D _currentEnemy;

    // Détection de la réparation
    private bool _wasDestroyedLastFrame;

    protected virtual void Start()
    {
        _wasDestroyedLastFrame = _isDestroyed;
    }

    protected override void Update()
    {
        base.Update();

        // Scan automatique dès que la fleur est sur pied
        if (_wasDestroyedLastFrame && !_isDestroyed)
        {
            ScanForEnemyAlreadyInside();
        }
        _wasDestroyedLastFrame = _isDestroyed;

        if (_isDestroyed)
            return;

        if (_currentEnemy == null)
            return;

        if (!_currentEnemy.gameObject.activeInHierarchy)
        {
            _currentEnemy = null;
            return;
        }

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + towerData.attackSpeed;
            animatorFlowerTower.SetTrigger("Attack");
        }
    }

    private void ScanForEnemyAlreadyInside()
    {
        _currentEnemy = null;

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
                    _currentEnemy = results[i];
                    Debug.Log($"[FlowerTower] Réparée ! Cible trouvée dans la zone : {_currentEnemy.name}");
                    break; // Un seul ennemi suffit
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDestroyed)
            return;

        if (!collision.CompareTag("Boss"))
            return;

        _currentEnemy = collision;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == _currentEnemy)
        {
            _currentEnemy = null;
        }
    }

    public void AE_AttackFlower()
    {
        if (_isDestroyed || _currentEnemy == null)
            return;

        animatorNuageToxic.SetTrigger("Attack");
    }

    public void AE_Attack()
    {
        if (_isDestroyed)
            return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transformNuageToxic.position,
            towerData.range);

        foreach (Collider2D hit in enemies)
        {
            if (!hit.CompareTag("Boss"))
                continue;

            if (!hit.TryGetComponent(out HealthSystem health))
                continue;

            if (health.currentHealth <= 0)
                continue;

            health.TakeDamage(towerData.damage);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        animatorFlowerTower.enabled = false; // Arrête l'anim si détruite
    }

    protected override void Rebuild()
    {
        base.Rebuild(); // Fait revivre la tour et restaure les HP

        // On relance les animations !
        animatorNuageToxic.enabled = true;
        animatorFlowerTower.enabled = true;
    }

    private void OnDrawGizmos()
    {
        if (transformNuageToxic != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transformNuageToxic.position, towerData.range);
        }
    }
}