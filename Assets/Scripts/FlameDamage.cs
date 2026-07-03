using UnityEngine;

public class FlameDamage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<HealthSystem>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(1);
            }
        }
    }
}
