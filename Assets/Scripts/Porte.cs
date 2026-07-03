using UnityEngine;

public class Porte : MonoBehaviour
{
    [SerializeField] private UIPlayer uiPlayer;
    [SerializeField] private HealthSystem bossHealth;

    private void OnEnable()
    {
        if (uiPlayer != null)
            uiPlayer.OnStarAmountChanged += CheckStarAmount;

        if (bossHealth != null)
            bossHealth.OnDeath += OuvrirPorte;
    }

    private void OnDisable()
    {
        if (uiPlayer != null)
            uiPlayer.OnStarAmountChanged -= CheckStarAmount;

        if (bossHealth != null)
            bossHealth.OnDeath -= OuvrirPorte;
    }

    private void CheckStarAmount()
    {
        Debug.Log(uiPlayer.GetStarAmount());
        if (uiPlayer != null && uiPlayer.GetStarAmount() == 15)
        {
            OuvrirPorte();
        }
    }

    private void OuvrirPorte()
    {
        Destroy(gameObject); 
    }
}