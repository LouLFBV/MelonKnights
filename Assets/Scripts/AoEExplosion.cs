using UnityEngine;

public class AoEExplosion : MonoBehaviour
{
    private int _damage;
    private float _range;
    private bool _hasExploded = false; // Sécurité pour éviter que l'AE ne se déclenche deux fois

    // Cette fonction est appelée par le projectile au moment où il fait apparaître l'éclair
    public void Initialize(int damage, float range)
    {
        _damage = damage;
        _range = range;
    }

    // --- L'ANIMATION EVENT À PLACER À LA FIN DE TON ANIMATION ---
    public void AE_ExplosionDamage()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        // 1. Détection de toutes les cibles dans le rayon de l'éclair
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _range);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Boss"))
            {
                if (hit.TryGetComponent(out HealthSystem hs))
                {
                    hs.TakeDamage(_damage);
                    Debug.Log($"⚡ L'éclair a frappé {hit.name} pour {_damage} dégâts !");
                }
            }
        }

        // 2. L'éclair s'autodétruit immédiatement après avoir appliqué les dégâts
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Permet de voir la zone de l'éclair dans l'éditeur en cliquant dessus
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _range > 0 ? _range : 2f);
    }
#endif
}