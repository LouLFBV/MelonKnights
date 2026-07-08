using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private int damage;

    public void Initialize(Vector2 dir, int attackDamage)
    {
        direction = dir.normalized;
        damage = attackDamage;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out HealthSystem health))
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}