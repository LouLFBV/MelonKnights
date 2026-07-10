using UnityEngine;

public class RacineTower : Tower
{
    [SerializeField] private Animator racineAnimator;
    [SerializeField] private Transform racineAttackPoint;

    private float _nextAttackTime;

    private Collider2D _currentEnemy;


    protected override void Update()
    {
        base.Update();
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