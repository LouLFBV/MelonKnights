using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private EnemySO enemySO;
    public int maxHealth = 8;
    [HideInInspector] public int currentHealth;
    public event System.Action OnHealthChanged;
    public event System.Action OnDeath;

    private void Start()
    {
        if (enemySO != null)
            maxHealth = enemySO.pv;
        currentHealth = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            OnHealthChanged?.Invoke();
            Debug.Log($"Current Health: {currentHealth}/{maxHealth}");
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke();
            }
        }
    }
}