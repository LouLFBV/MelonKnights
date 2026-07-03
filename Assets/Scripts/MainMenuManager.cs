using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bestTimeText;

    void Start()
    {
        // On vérifie si le joueur a déjà battu le boss au moins une fois
        if (PlayerPrefs.HasKey("BestBossTime"))
        {
            float bestTime = PlayerPrefs.GetFloat("BestBossTime");

            // On réutilise la même logique de formatage
            int minutes = Mathf.FloorToInt(bestTime / 60);
            int seconds = Mathf.FloorToInt(bestTime % 60);
            int centiseconds = Mathf.FloorToInt((bestTime % 1) * 100);

            bestTimeText.text = "Meilleur Temps : " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
        }
        else
        {
            // Si le joueur n'a pas encore battu le boss
            bestTimeText.text = "Meilleur Temps : --:--.--";
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}