using UnityEngine;

public class FlowerTower : MonoBehaviour
{
    [SerializeField] private TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;

    [Header("Nuage Toxic")]
    [SerializeField] private Transform transformNuageToxic;
    [SerializeField] private Animator animatorFlowerTower;
    [SerializeField] private Animator animatorNuageToxic;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On vérifie si on est détruit, si c'est un boss, ET si le cooldown est passé
        if (_isDestroyed || !collision.CompareTag("Boss")) return;

        if (Time.time >= _nextAttackTime)
        {
            animatorFlowerTower.SetTrigger("Attack");
            _nextAttackTime = Time.time + towerData.attackSpeed; // Utilise ton attackSpeed du SO
        }
    }
    public void AE_AttackFlower()
    {
        animatorNuageToxic.SetTrigger("Attack");
    }

    public void AE_Attack()
    {
        if (_isDestroyed) return;

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transformNuageToxic.position, towerData.range);

        foreach (var enemyCollider in enemiesInRange)
        {
            // Vérifie que c'est bien un Boss avant d'infliger des dégâts
            if (enemyCollider.CompareTag("Boss") && enemyCollider.TryGetComponent<HealthSystem>(out var enemy))
            {
                enemy.TakeDamage(towerData.damage);
            }
        }
    }

    private void OnDeath()
    {
        _isDestroyed = true;
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;
        animatorNuageToxic.enabled = false; // Arrête l'anim si détruite
        animatorFlowerTower.enabled = false; // Arrête l'anim si détruite
    }
}