using UnityEngine;
using System.Collections.Generic; // Nécessaire pour la détection de liste

public class RacineTower : Tower
{
    [SerializeField] private Animator racineAnimator;
    [SerializeField] private Transform racineAttackPoint;

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

        // Si on vient d'être réparé, on cherche immédiatement une cible
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
            racineAnimator.SetTrigger("Attack");
        }
    }

    private void ScanForEnemyAlreadyInside()
    {
        _currentEnemy = null;

        Collider2D[] colliders = GetComponents<Collider2D>();
        Collider2D rangeTrigger = null;

        // On cherche le collider configuré en Trigger
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
                    Debug.Log($"[RacineTower] Réparée ! Cible trouvée dans la zone : {_currentEnemy.name}");
                    break; // Un seul ennemi suffit pour cette tour
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

    // Animation Event
    public void AE_RacineAttack()
    {
        if (_isDestroyed)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            racineAttackPoint.position,
            towerData.range);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Boss"))
                continue;

            if (hit.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(towerData.damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (racineAttackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(racineAttackPoint.position, towerData.range);
    }
}