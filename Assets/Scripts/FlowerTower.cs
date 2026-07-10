using UnityEngine;

public class FlowerTower : Tower
{
    [Header("Nuage Toxic")]
    [SerializeField] private Transform transformNuageToxic;
    [SerializeField] private Animator animatorFlowerTower;
    [SerializeField] private Animator animatorNuageToxic;

    private float _nextAttackTime;
    private Collider2D _currentEnemy;


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
            animatorFlowerTower.SetTrigger("Attack");
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
        //animatorNuageToxic.enabled = false; // Arrête l'anim si détruite
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