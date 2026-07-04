using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(Collider2D))]
public class CoreDefense : MonoBehaviour
{
    public static CoreDefense Instance { get; private set; }

    [Header("Références")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject shopPanel; // Panel UI du shop, laissé vide pour l'instant

    [Header("Game Over")]
    [SerializeField] private Animator panelTransitionAnimator; // Optionnel, même logique que TriggerScene/PlayerController
    [SerializeField] private string gameOverSceneName;

    private HealthSystem _healthSystem;
    private Collider2D _collider;
    private bool _isShopOpen = false;
    private bool _isDead = false;

    public bool IsShopOpen => _isShopOpen;

    // Permet à d'autres scripts (game manager, UI, ennemis) de réagir à la destruction du coeur
    public static event System.Action OnCoreDestroyed;

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
    }

    private void OnDisable()
    {
        if (_healthSystem != null)
            _healthSystem.OnDeath -= OnCoreDeath;

        if (PlayerController.Instance != null)
            PlayerController.Instance.OnCancelPressed -= CloseShop;
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

        Debug.Log("Shop du Coeur fermé.");
    }

    private void OnCoreDeath()
    {
        if (_isDead) return;

        _isDead = true;
        CloseShop();

        OnCoreDestroyed?.Invoke();

        Debug.Log("Le Coeur a été détruit. Game Over.");

        if (panelTransitionAnimator != null)
        {
            panelTransitionAnimator.SetTrigger("StartTransition");
        }
        else if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            AE_ChangeToGameOverScene();
        }
    }

    // A appeler depuis un Animation Event sur la transition (même pattern que TriggerScene.AE_ChangeScene)
    public void AE_ChangeToGameOverScene()
    {
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}