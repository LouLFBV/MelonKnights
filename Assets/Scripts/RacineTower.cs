using UnityEngine;

public class RacineTower : MonoBehaviour
{
    [SerializeField] private TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Animator racineAnimator;
    [SerializeField] private Transform racineAttackPoint;

    private float _nextAttackTime;
    private bool _isDestroyed;

    private Collider2D _currentEnemy;

    private void OnEnable()
    {
        if (healthSystem != null)
            healthSystem.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (healthSystem != null)
            healthSystem.OnDeath -= OnDeath;
    }

    private void Update()
    {
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

    private void OnDeath()
    {
        _isDestroyed = true;
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;
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