using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(Collider2D))]
public class CoreDefense : MonoBehaviour
{
    public static CoreDefense Instance { get; private set; }

    [Header("Références")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject shopPanel; // Panel UI du shop, laissé vide pour l'instant
    [SerializeField] private Animator coreAnimator;
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private GameObject _firstButtonToSelect;
    [SerializeField] private PlayerAttack _playerAttack;

    [Header("Game Over")]
    [SerializeField] private GameObject panelDefeat;
    [SerializeField] private Animator panelDefeatAnimator;
    [SerializeField] private GameObject _firstDieButtonToSelect;


    [Header("Win")]
    [SerializeField] private WaveManager wazeManager;
    [SerializeField] private GameObject panelVictory;
    [SerializeField] private Animator panelVictoryAnimator;
    [SerializeField] private GameObject _firstWinButtonToSelect;

    private HealthSystem _healthSystem;
    private bool _isShopOpen = false;
    private bool _isDead = false;

    public bool IsShopOpen => _isShopOpen;

    // Permet à d'autres scripts (game manager, UI, ennemis) de réagir à la destruction du coeur
    public static event Action OnCoreDestroyed;


    [Header("Audio")]
    [SerializeField] private AudioClip closeOpenAudioClip;
    [SerializeField] private AudioSource audioSource;

    void Awake()
    {
        Instance = this;

        _healthSystem = GetComponent<HealthSystem>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Le shop doit être fermé au lancement
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (_healthSystem != null)
            _healthSystem.OnDeath += OnCoreDeath;

        // On réutilise le système d'annulation déjà en place sur le joueur
        // pour fermer le shop avec le même bouton que les autres menus (Cancel)
        if (PlayerController.Instance != null)
            PlayerController.Instance.OnCancelPressed += CloseShop;

        if(wazeManager != null)
            wazeManager.OnAllWavesCompleted += OnVictory;
    }

    private void OnDisable()
    {
        if (_healthSystem != null)
            _healthSystem.OnDeath -= OnCoreDeath;

        if (PlayerController.Instance != null)
            PlayerController.Instance.OnCancelPressed -= CloseShop;

        if (wazeManager != null)
            wazeManager.OnAllWavesCompleted -= OnVictory;
    }

    private void OnVictory()
    {
        panelVictory.SetActive(true);
        Time.timeScale = 0f; // Met le jeu en pause
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.SelectUI(_firstWinButtonToSelect);
        }
    }

    public void ToggleShop(InputAction.CallbackContext ctx)
    {
        Debug.LogWarning("Tentaive d'ouverture de shop");
        if (_isShopOpen)
            CloseShop();
        else
            OpenShop();
    }

    private void OpenShop()
    {
        if (_isShopOpen) return;

        _isShopOpen = true;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            PlayerController.Instance.SetCanMove(false);
            _playerAttack.SetCanAttack(false);
        }

        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.SelectUI(_firstButtonToSelect);
        }

        if (audioSource != null && closeOpenAudioClip != null)
            audioSource.PlayOneShot(closeOpenAudioClip);

        Debug.Log("Shop du Coeur ouvert.");
    }

    private void CloseShop()
    {
        if (!_isShopOpen) return;

        _isShopOpen = false;

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            PlayerController.Instance.SetCanMove(true);
            _playerAttack.SetCanAttack(true);
        }

        // Vide la sélection mémorisée une fois le shop fermé
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.SelectUI(null);
        }

        if (audioSource != null && closeOpenAudioClip != null)
            audioSource.PlayOneShot(closeOpenAudioClip);

        Debug.Log("Shop du Coeur fermé.");
    }

    private void OnCoreDeath()
    {
        if (_isDead) return;

        _isDead = true;
        CloseShop();
        coreAnimator.SetTrigger("EndGame");

        OnCoreDestroyed?.Invoke();

    }

    public void AE_OnCoreDeath()
    {
        panelDefeat.SetActive(true);
        Time.timeScale = 0f; // Met le jeu en pause
        panelDefeatAnimator.SetTrigger("EndGame");
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.SelectUI(_firstDieButtonToSelect);
        }
        Debug.Log("Le Coeur a été détruit. Game Over.");
    }


    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}