using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Statistiques")]
    [SerializeField] private WeaponSO weaponData;
    [SerializeField] private int damage = 1; // Utilisé par les tourelles si weaponData est null
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float fallbackAttackRange = 2f;

    [Header("Effets")]
    [SerializeField] private GameObject gameObjectExplosive; // Effet d'impact (éclair pour le staff, sang/étincelle pour le reste)

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
        if (collision.CompareTag("Boss"))
        {
            // --- LOGIQUE COMMUNE (Dagues, Tourelles, Flèches, Staff...) ---
            // 1. On applique d'abord les dégâts directs du projectile
            ApplyDirectDamage(collision);

            // 2. SÉCURITÉ : On vérifie que weaponData n'est pas NULL avant de lire le type d'arme
            if (weaponData != null && weaponData.weaponType == WeaponType.Staff)
            {
                // --- LOGIQUE DU BÂTON ---
                // On crée l'éclair et on lui donne les infos pour qu'il fasse les dégâts de zone à la fin de son animation
                if (gameObjectExplosive != null)
                {
                    GameObject explosionInstance = Instantiate(gameObjectExplosive, transform.position, Quaternion.identity);
                    if (explosionInstance.TryGetComponent(out AoEExplosion aoe))
                    {
                        aoe.Initialize(weaponData.damage, weaponData.attackRange);
                    }
                }
            }

            // Quel que soit le projectile, il se détruit après avoir touché
            Destroy(gameObject);
        }
    }

    // Méthode pour appliquer les dégâts normaux
    private void ApplyDirectDamage(Collider2D targetCollider)
    {
        HealthSystem hs = targetCollider.GetComponent<HealthSystem>();

        if (hs == null)
        {
            hs = targetCollider.GetComponentInParent<HealthSystem>();
        }

        if (hs != null)
        {
            // Si l'arme a un SO, on prend ses dégâts, sinon on prend la valeur 'damage' (pratique pour les tourelles)
            int finalDamage = (weaponData != null) ? weaponData.damage : damage;
            hs.TakeDamage(finalDamage);
            Debug.Log($"Projectile a touché {targetCollider.name} pour {finalDamage} dégâts directs.");
        }
    }
}