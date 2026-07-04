using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private WeaponSO weaponData;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    private Vector2 direction;
    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime);
    }
    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the projectile hits an enemy
        if (collision.CompareTag("Boss"))
        {
            // Assuming the enemy has a method to take damage
            collision.GetComponent<HealthSystem>().TakeDamage(weaponData.damage);
            Destroy(gameObject);
        }
    }
}