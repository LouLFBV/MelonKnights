using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}
