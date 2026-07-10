using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBarParent;
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
            healthSystem.OnDeath += DesactiveUI;
            healthSystem.OnReset += ActiveUI;
        }

    }

    private void OnDisable()
    {

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthBar;
            healthSystem.OnDeath -= DesactiveUI;
            healthSystem.OnReset -= ActiveUI;
        }
    }

    private void DesactiveUI()
    {
        if (healthBarParent != null)
        {
            healthBarParent.SetActive(false);
        }
    }

    private void ActiveUI()
    {
        if (healthBarParent != null)
        {
            healthBarParent.SetActive(true);
        }
    }

    private void UpdateHealthBar()
    {
        healthBarCore.fillAmount = (float)healthSystem.currentHealth / healthSystem.maxHealth;
    }

}
