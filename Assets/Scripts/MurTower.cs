using UnityEngine;

public class MurTower : MonoBehaviour
{
    [SerializeField] private TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;

    [Header("Configuration Reconstruction")]
    [SerializeField] private float rebuildDistance = 2f; // Distance d'approche requise

    private bool _isDestroyed = false;

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

    private void Update()
    {
        // Si le mur n'est pas détruit, on ne perd pas de temps à calculer
        if (!_isDestroyed) return;

        // On vérifie en continu si le joueur existe et s'il a son outil
        if (PlayerController.Instance != null && PlayerController.Instance.outilEquipped)
        {
            // Calcul de la distance entre le mur et le joueur
            float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

            // Si le joueur est assez proche : RECONSTRUCTION AUTOMATIQUE !
            if (distance <= rebuildDistance)
            {
                Rebuild();
            }
        }
    }

    private void Rebuild()
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
        Gizmos.DrawWireSphere(transform.position, rebuildDistance);
    }
}