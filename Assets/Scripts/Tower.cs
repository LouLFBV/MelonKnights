using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] protected TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;

    [Header("Configuration Reconstruction")]
    [SerializeField] private Transform transformTower; // Distance d'approche requise
    [SerializeField] private float rebuildDistance = 2f; // Distance d'approche requise

    protected bool _isDestroyed = false;

    private void OnEnable()
    {
        if (healthSystem != null) healthSystem.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (healthSystem != null) healthSystem.OnDeath -= OnDeath;
    }

    protected virtual void OnDeath()
    {
        _isDestroyed = true;
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;
    }

    protected virtual void Update()
    {
        // Si le mur n'est pas détruit, on ne perd pas de temps à calculer
        if (!_isDestroyed) return;

        // On vérifie en continu si le joueur existe et s'il a son outil
        if (PlayerController.Instance != null && PlayerController.Instance.outilEquipped)
        {
            // Calcul de la distance entre le mur et le joueur
            float distance = Vector2.Distance(transformTower.position, PlayerController.Instance.transform.position);

            // Si le joueur est assez proche : RECONSTRUCTION AUTOMATIQUE !
            if (distance <= rebuildDistance)
            {
                Rebuild();
            }
        }
    }

    protected virtual void Rebuild()
    {
        _isDestroyed = false;
        spriteOK.enabled = true;
        spriteDestroy.enabled = false;

        if (healthSystem != null)
        {
            healthSystem.ResetHP();
        }

        Debug.Log("Le mur s'est reconstruit automatiquement à l'approche du joueur !");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transformTower.position, rebuildDistance);
    }
}