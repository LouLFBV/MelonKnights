using UnityEngine;

public class XpSystem : MonoBehaviour
{
    [Header("Configuration XP de Base")]
    [HideInInspector] public int maxXp = 50;
    [HideInInspector] public int currentXp;
    [HideInInspector] public int levelPlayer = 0;

    public event System.Action OnXpChanged;
    public event System.Action OnLevelUp;

    [Header("Configuration Progression (Le truc sympa !)")]
    [Tooltip("Multiplier par 1.2 signifie qu'on demande +20% d'XP en plus au niveau suivant.")]
    [SerializeField] private float xpRequirementMultiplier = 1.2f;

    [Header("Configuration du Plafond de Carte")]
    [SerializeField] private float maxXpBonus = 1.0f; // +100% d'XP max
    private float _currentXpBonus = 0.0f;

    [Header("Audio")]
    [SerializeField] private AudioClip levelUpAudioClip;
    [SerializeField] private AudioSource audioSource;


    private void Awake()
    {
        currentXp = 0;
    }

    public void AddXP(int xpAdded)
    {
        int finalXp = Mathf.RoundToInt(xpAdded * (1f + _currentXpBonus));
        currentXp += finalXp;

        // Boucle de Level Up
        while (currentXp >= maxXp)
        {
            currentXp -= maxXp;
            levelPlayer++;

            OnLevelUp?.Invoke();
            if(audioSource != null && levelUpAudioClip != null)
            {
                audioSource.PlayOneShot(levelUpAudioClip);
            }
        }
        OnXpChanged?.Invoke();
    }

    public void IncreaseXP(float percentage)
    {
        if (IsXpCardMaxed()) return;

        _currentXpBonus += percentage;

        if (_currentXpBonus > maxXpBonus)
        {
            _currentXpBonus = maxXpBonus;
        }

        Debug.Log($"Bonus XP augmenté ! Bonus actuel : +{_currentXpBonus * 100}%");
    }

    public bool IsXpCardMaxed()
    {
        return _currentXpBonus >= maxXpBonus;
    }
}