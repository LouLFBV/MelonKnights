using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private HealthSystem bossHealth;

    private float currentTime = 0f;
    private bool isTimerRunning = false;

    void Start()
    {
        // Le timer commence dès le début de la scène (ou du combat)
        currentTime = 0f;
        isTimerRunning = true;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay(currentTime, timerText);
        }
    }

    private void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDeath += StopTimer;
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnDeath -= StopTimer;
    }

    private void StopTimer()
    {
        if (!isTimerRunning) return;

        isTimerRunning = false;
        CheckAndSaveBestTime(currentTime);
    }

    // Formate le temps en MM:SS.CC (Minutes:Secondes:Centièmes)
    public static void UpdateTimerDisplay(float timeToDisplay, TextMeshProUGUI textComponent)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        int centiseconds = Mathf.FloorToInt((timeToDisplay % 1) * 100);

        textComponent.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }

    private void CheckAndSaveBestTime(float finalTime)
    {
        // On récupère le record actuel. Si aucun record n'existe, on met une valeur immense (999999)
        // pour être sûr que le premier essai du joueur devienne le nouveau record.
        float currentBestTime = PlayerPrefs.GetFloat("BestBossTime", 999999f);

        if (finalTime < currentBestTime)
        {
            PlayerPrefs.SetFloat("BestBossTime", finalTime);
            PlayerPrefs.Save(); // Force la sauvegarde sur le disque
            Debug.Log("Nouveau record battu ! : " + finalTime);
        }
    }
}