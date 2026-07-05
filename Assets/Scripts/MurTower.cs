using UnityEngine;

public class MurTower : MonoBehaviour
{
    [SerializeField] private TowerSO towerData;
    [SerializeField] private SpriteRenderer spriteOK;
    [SerializeField] private SpriteRenderer spriteDestroy;
    [SerializeField] private HealthSystem healthSystem;

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
        spriteOK.enabled = false;
        spriteDestroy.enabled = true;
    }
}
