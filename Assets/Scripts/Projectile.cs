using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private WeaponSO weaponData;
    [SerializeField] private int damage = 1;
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
            HealthSystem hs = collision.GetComponent<HealthSystem>();
            if (hs == null)
            {
                hs = collision.GetComponentInParent<HealthSystem>();
            }

            if (hs != null)
            {
                Debug.Log($"Santé trouvée sur : {hs.gameObject.name}");
                if (weaponData != null)
                {
                    hs.TakeDamage(weaponData.damage);
                    Debug.Log($"Projectile hit {collision.name} for {weaponData.damage} damage.");
                }
                else
                {
                    hs.TakeDamage(damage);
                }
            }
            else
            {
                Debug.LogError($"Attention : Collision avec {collision.name}, mais aucun HealthSystem trouvé !");
            }            
            Destroy(gameObject);
        }
    }
}