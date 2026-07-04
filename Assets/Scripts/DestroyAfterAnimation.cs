using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    // À appeler via un Animation Event à la toute dernière frame de ton animation de Slash !
    public void AE_DestroyMe()
    {
        Destroy(gameObject);
    }
}