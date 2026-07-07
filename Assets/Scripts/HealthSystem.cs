using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private EnemySO enemySO;
    [SerializeField] private TowerSO towerSO;
    public int maxHealth = 8;
    [HideInInspector] public int currentHealth;
    public event System.Action OnHealthChanged;
    public event System.Action OnDeath;

    [Header("Configuration du Plafond de Carte")]
    [SerializeField] private float maxHealthBonusCap = 1.0f; // Plafond max (ex: 1.0f = +100% de vie max)
    private float _currentHealthBonus = 0.0f;           // Bonus actuel accumulé
    private int _baseMaxHealth;                         // Sauvegarde de la vie de base

    private void Awake()
    {
        if (enemySO != null)
            maxHealth = enemySO.hp;
        if (towerSO != null)
            maxHealth = towerSO.hp;

        _baseMaxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            OnHealthChanged?.Invoke();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke();
            }
        }
    }

    public void ResetHP()
    {
        Debug.Log($"Resetting HP for {this.gameObject.name}");
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    // La méthode complétée pour ta carte Vie
    public void IncreaseHealth(float percentage)
    {
        Debug.Log($"Tentative d'augmentation de la vie de {percentage * 100}% pour {this.gameObject.name}");
        // 1. Sécurité si le plafond est déjà atteint
        if (IsHealthCardMaxed()) return;

        // 2. On ajoute le pourcentage de la carte (ex: 0.20f pour +20%)
        _currentHealthBonus += percentage;

        // 3. On bloque au plafond si on le dépasse
        if (_currentHealthBonus > maxHealthBonusCap)
        {
            _currentHealthBonus = maxHealthBonusCap;
        }

        // 4. On calcule la nouvelle vie max à partir de la vie de base
        int oldMaxHealth = maxHealth;
        maxHealth = Mathf.RoundToInt(_baseMaxHealth * (1f + _currentHealthBonus));

        // 5. TRUC SYMPA : On donne le bonus de PV gagné à la vie actuelle 
        // (Comme ça, si la vie max augmente de 5 PV, le joueur gagne aussi 5 PV actuels)
        int healthGained = maxHealth - oldMaxHealth;
        currentHealth += healthGained;

        // 6. On notifie l'UI pour mettre à jour la barre de vie
        OnHealthChanged?.Invoke();

        Debug.Log($"Vie augmentée ! Nouveau Max : {maxHealth} (Bonus total : +{_currentHealthBonus * 100}%)");
    }

    public bool IsHealthCardMaxed()
    {
        return _currentHealthBonus >= maxHealthBonusCap;
    }
}