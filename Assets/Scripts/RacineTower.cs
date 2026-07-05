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

    private void OnEnable()
    {
        if (healthSystem != null) healthSystem.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (healthSystem != null) healthSystem.OnDeath -= OnDeath;
    }

    private void OnDeath()
    {
        _isDestroyed = true;
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDestroyed || !collision.CompareTag("Boss")) return;

        if (Time.time >= _nextAttackTime)
        {
            // 1. Flip le sprite si le boss est à gauche
            // On compare la position X du boss par rapport à la tour
            spriteOK.flipX = collision.transform.position.x < transform.position.x;

            // 2. Lance l'animation
            racineAnimator.SetTrigger("Attack");

            // On met à jour le cooldown
            _nextAttackTime = Time.time + towerData.attackSpeed;
        }
    }

    // Appelée par l'Animation Event
    public void AE_RacineAttack()
    {
        if (_isDestroyed) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(racineAttackPoint.position, towerData.range);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Boss") && hit.TryGetComponent(out HealthSystem bossHealth))
            {
                bossHealth.TakeDamage(towerData.damage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (racineAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(racineAttackPoint.position, towerData.range);
        }
    }
}