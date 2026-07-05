using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    public WeaponType weaponType;
    [SerializeField] private GameObject visualForward; // Bas
    [SerializeField] private GameObject visualBack;    // Haut
    [SerializeField] private GameObject visualLeft;    // Gauche (Bas-Gauche)
    [SerializeField] private GameObject visualRight;   // Droite (Bas-Droite)

    public void SetDirection(Vector2 direction)
    {
        // Désactiver tout
        visualForward.SetActive(false);
        visualBack.SetActive(false);
        visualLeft.SetActive(false);
        visualRight.SetActive(false);

        // Calcul de l'angle en degrés (-180 à 180)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Logique par secteurs
        if (angle > 67.5f && angle <= 112.5f)
            visualBack.SetActive(true);      // Haut pur
        else if (angle > 112.5f && angle <= 157.5f)
            visualLeft.SetActive(true);     // Haut-Gauche
        else if (angle > 157.5f || angle <= -157.5f)
            visualLeft.SetActive(true);      // Gauche pur (ou bas-gauche)
        else if (angle > -157.5f && angle <= -112.5f)
            visualLeft.SetActive(true);      // Bas-Gauche
        else if (angle > -112.5f && angle <= -67.5f)
            visualForward.SetActive(true);   // Bas pur
        else if (angle > -67.5f && angle <= -22.5f)
            visualForward.SetActive(true);     // Bas-Droite
        else if (angle > -22.5f && angle <= 22.5f)
            visualRight.SetActive(true);     // Droite pur
        else
            visualRight.SetActive(true);    // Haut-Droite
    }
}