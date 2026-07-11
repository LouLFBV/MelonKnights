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

    [Header("Game Over")]
    [SerializeField] private GameObject panelDefeat;
    [SerializeField] private Animator panelDefeatAnimator;


    [Header("Win")]
    [SerializeField] private WaveManager wazeManager;
    [SerializeField] private GameObject panelVictory;
    [SerializeField] private Animator panelVictoryAnimator;

    private HealthSystem _healthSystem;
    private Collider2D _collider;
    private bool _isShopOpen = false;
    private bool _isDead = false;

    public bool IsShopOpen => _isShopOpen;

    // Permet à d'autres scripts (game manager, UI, ennemis) de réagir à la destruction du coeur
    public static event System.Action OnCoreDestroyed;


    [Header("Audio")]
    [SerializeField] private AudioClip closeOpenAudioClip;
    [SerializeField] private AudioSource audioSource;

    void Awake()
    {
        Instance = this;

        _healthSystem = GetComponent<HealthSystem>();
        _collider = GetComponent<Collider2D>();

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
        panelVictoryAnimator.SetTrigger("EndGame");
    }

    void Update()
    {
        if (_isDead) return;

        DetectClick();
    }

    private void DetectClick()
    {
        // Utilise le nouveau Input System (Mouse.current), cohérent avec le reste du projet
        if (Mouse.current == null || mainCamera == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Si la souris est au-dessus d'un élément UI (bouton du shop, etc.),
            // on ignore le clic sur le Core pour éviter que ça ne le referme aussitôt
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit == _collider)
            {
                ToggleShop();
            }
        }
    }

    public void ToggleShop()
    {
        if (_isShopOpen)
            CloseShop();
        else
            OpenShop();
    }

    public void OpenShop()
    {
        if (_isShopOpen) return;

        _isShopOpen = true;

        if (shopPanel != null)
            shopPanel.SetActive(true);

        if(audioSource != null && closeOpenAudioClip != null)
            audioSource.PlayOneShot(closeOpenAudioClip);

        // Optionnel : on pourrait bloquer le mouvement du joueur ici
        // via PlayerController.Instance.SetCanMove(false) si tu veux
        // que le joueur ne se déplace pas pendant qu'il achète.

        Debug.Log("Shop du Coeur ouvert.");
    }

    public void CloseShop()
    {
        if (!_isShopOpen) return;

        _isShopOpen = false;

        if (shopPanel != null)
            shopPanel.SetActive(false);

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