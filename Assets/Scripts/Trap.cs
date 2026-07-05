using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private TowerSO towerSO;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            if (collision.TryGetComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(towerSO.damage);
            }
        }
    }
}
