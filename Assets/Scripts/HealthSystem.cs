using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 8;
    [HideInInspector] public int currentHealth;
    public event System.Action OnHealthChanged;
    public event System.Action OnDeath;

    private void Start()
    {
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

    public bool IsMidLife()
    {
        return currentHealth <= maxHealth/2;
    }
}