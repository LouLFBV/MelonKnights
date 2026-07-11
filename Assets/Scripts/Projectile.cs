using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Statistiques")]
    [SerializeField] private WeaponSO weaponData;
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float fallbackAttackRange = 2f; // <-- Ajout pour éviter le crash si weaponData est null

    [Header("Effets")]
    [SerializeField] private GameObject gameObjectExplosive; // Changé en GameObject (voir note plus bas)

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
            // Fait apparaître l'explosion si elle existe
            if (gameObjectExplosive != null)
            {
                Instantiate(gameObjectExplosive, transform.position, Quaternion.identity);
            }

            // Applique les dégâts
            DealDamage(collision);

            // Détruit le projectile après l'impact
            Destroy(gameObject);
        }
    }

    // Animation Event
    public void AE_Attack()
    {
        // Sécurise la portée (utilise celle du SO si dispo, sinon la valeur de secours)
        float range = (weaponData != null) ? weaponData.attackRange : fallbackAttackRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);

        // Note : Pas besoin de "if (hits.Length > 0)", le foreach gère déjà les listes vides !
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Boss"))
            {
                DealDamage(hit);
            }
        }
        Destroy(gameObject);
    }

    // --- NOUVELLE MÉTHODE CENTRALISÉE ---
    private void DealDamage(Collider2D targetCollider)
    {
        if(weaponData != null)
        {
            if (weaponData.weaponType == WeaponType.Staff)
            {
                AE_Attack();
            }
        }
        HealthSystem hs = targetCollider.GetComponent<HealthSystem>();

        if (hs == null)
        {
            hs = targetCollider.GetComponentInParent<HealthSystem>();
        }

        if (hs != null)
        {
            // Détermine les dégâts finaux en une seule ligne
            int finalDamage = (weaponData != null) ? weaponData.damage : damage;
            Debug.Log($"Calcul des dégâts pour {targetCollider.name} : {finalDamage} (WeaponSO: {(weaponData != null ? weaponData.weaponName : "null")})");
            hs.TakeDamage(finalDamage);
            Debug.Log($"Projectile a touché {targetCollider.name} pour {finalDamage} dégâts. (Santé trouvée sur : {hs.gameObject.name})");
        }
    }
}