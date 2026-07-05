using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    // Référence vers ton script principal
    [SerializeField] private RacineTower mainTowerScript;
    [SerializeField] private FlowerTower mainFlowerScript;

    // Cette fonction est appelée par l'Animation Event
    public void AE_RacineAttack()
    {
        if (mainTowerScript != null)
        {
            mainTowerScript.AE_RacineAttack();
        }
        if (mainFlowerScript != null)
        {
            mainFlowerScript.AE_Attack();
        }
    }
}