using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarCore;
    [SerializeField] private HealthSystem healthSystem;
    private void Start()
    {
        UpdateHealthBar();
    }

    private void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealthBar;
        }

    }

    private void OnDisable()
    {

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar()
    {
        healthBarCore.fillAmount = (float)healthSystem.currentHealth / healthSystem.maxHealth;
    }

}
