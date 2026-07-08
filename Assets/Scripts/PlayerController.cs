using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Configuration Déplacement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera mainCamera;


    [Header("Composants")]
    [SerializeField] private Animator panelTransitionAnimator; // Nom de la scène à charger
    [SerializeField] private GameObject panelDeathGameObject; // Nom de la scène à charger
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private XpSystem xpSystem;
    [SerializeField] private PlayerAttack playerAttack;


    [Header("Weapon Visual")]
    [SerializeField] private WeaponVisual[] weaponVisuals;
    private Dictionary<WeaponType, WeaponVisual> visuals;



    private WeaponVisual currentWeaponVisual;
    public bool outilEquipped = false;


    public XpSystem XpSystem => xpSystem;

    private Rigidbody2D rb;
    public Animator animator;
    private Vector2 moveInput;
    private PlayerInput _playerInput;
    private InputAction moveAction;
    private InputAction valideAction;
    private InputAction cancelAction;
    private bool _canMove = true;
    public bool CanMove => _canMove;
    private Vector2 _lastDirection = Vector2.down;
    public Vector2 LastDirection => _lastDirection;
    public bool JustReleasedThisFrame { get; private set; }
    public event Action OnValidatePressed;
    public event Action OnCancelPressed;

    [Header("Configuration des Plafonds de Cartes")]
    [SerializeField] private float maxMoveSpeedBonusCap = 1.0f; // Plafond Vitesse (ex: +100% de vitesse max)
    [SerializeField] private float maxCoinBonusCap = 2.0f;      // Plafond Pièces (ex: +200% de gains max)

    private float _baseMoveSpeed;              // Sauvegarde de la vitesse initiale
    private float _currentMoveSpeedBonus = 0f; // Bonus de vitesse cumulé (0.2 = +20%)
    private float _currentCoinBonus = 0f;      // Bonus de pièces cumulé

    public float CoinMultiplier => 1f + _currentCoinBonus;
    void Awake()
    {
        Instance = this;
        _playerInput = GetComponent<PlayerInput>();

        if (_playerInput == null)
        {
            Debug.LogError("Il manque le composant PlayerInput sur le joueur !");
            return;
        }

        moveAction = _playerInput.actions["Move"];
        valideAction = _playerInput.actions["Valide"];

        // On récupère l'action d'annulation (Vérifie bien le nom dans ton Input Action Asset, souvent c'est "Cancel")
        cancelAction = _playerInput.actions["Cancel"];

        visuals = new Dictionary<WeaponType, WeaponVisual>();

        foreach (WeaponVisual visual in weaponVisuals)
        {
            visuals.Add(visual.weaponType, visual);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        if (animator == null || !_canMove) return;

        if (moveInput != Vector2.zero)
        {

            _lastDirection = moveInput.normalized;

            animator.SetFloat("X", moveInput.x);
            animator.SetFloat("Y", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);

            UpdateWeaponVisual();
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    private void UpdateWeaponVisual()
    {
        if (currentWeaponVisual != null)
        {
            currentWeaponVisual.SetDirection(_lastDirection);
        }
    }

    public void EquipWeapon(WeaponType weaponType)
    {
        Debug.Log($"EquipWeapon called with weaponType: {weaponType}");

        if (weaponType == WeaponType.Outil)
        {
            outilEquipped = true;
        }
        else
        {
            outilEquipped = false;
        }
        if (currentWeaponVisual != null)
            currentWeaponVisual.gameObject.SetActive(false);

        currentWeaponVisual = visuals[weaponType];

        currentWeaponVisual.gameObject.SetActive(true);
        currentWeaponVisual.SetDirection(_lastDirection);
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }
        if (valideAction != null)
        {
            valideAction.performed += OnValidePerformed;
        }
        if (cancelAction != null)
        {
            cancelAction.performed += OnCancelPerformed; // <--- NOUVEAU
        }
        if (healthSystem != null)
        {
            healthSystem.OnDeath += PlayerDeath;
        }
    }


    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
        if (valideAction != null)
        {
            valideAction.performed -= OnValidePerformed;
        }
        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancelPerformed;
        }
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= PlayerDeath;
        }
    }



    private void OnValidePerformed(InputAction.CallbackContext context)
    {
        OnValidatePressed?.Invoke();
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnCancelPressed?.Invoke();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!_canMove) return;
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void PlayerDeath()
    {
        SetCanMove(false);
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    public void AE_OnPlayerDeath()
    {
        panelDeathGameObject.SetActive(true);
        panelTransitionAnimator.SetTrigger("StartTransition");
    }
    void FixedUpdate()
    {
        if (rb != null && _canMove)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        }
    }


    // 2. Modifie ta méthode SetCanMove pour détecter la libération immédiate
    public void SetCanMove(bool canMove)
    {
        // Si on était bloqué et qu'on nous redonne le mouvement, on active le drapeau de sécurité
        if (!_canMove && canMove)
        {
            JustReleasedThisFrame = true;
        }

        _canMove = canMove;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            if (animator != null) animator.SetFloat("Speed", 0f);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    // 3. Ajoute une fonction LateUpdate pour réinitialiser le drapeau à la fin de la frame
    private void LateUpdate()
    {
        JustReleasedThisFrame = false;
    }


    #region Carte System Methods

    public void IncreaseMovementSpeed(float percentage)
    {
        if (IsMovementSpeedMaxed()) return;

        _currentMoveSpeedBonus += percentage;

        if (_currentMoveSpeedBonus > maxMoveSpeedBonusCap)
            _currentMoveSpeedBonus = maxMoveSpeedBonusCap;

        // On applique le modificateur sur la vitesse de base
        moveSpeed = _baseMoveSpeed * (1f + _currentMoveSpeedBonus);

        Debug.Log($"Vitesse augmentée ! Nouvelle vitesse : {moveSpeed} (Bonus : +{_currentMoveSpeedBonus * 100}%)");
    }

    // MÉTHODE CARTE : Multiplicateur d'Or
    public void IncreaseCoin(float percentage)
    {
        if (IsCoinMaxed()) return;

        _currentCoinBonus += percentage;

        if (_currentCoinBonus > maxCoinBonusCap)
            _currentCoinBonus = maxCoinBonusCap;

        Debug.Log($"Multiplicateur d'or augmenté ! Bonus actuel : +{_currentCoinBonus * 100}% (Multiplicateur : x{CoinMultiplier})");
    }

    // Fonctions de vérification pour ton CarteSystem
    public bool IsMovementSpeedMaxed() => _currentMoveSpeedBonus >= maxMoveSpeedBonusCap;
    public bool IsCoinMaxed() => _currentCoinBonus >= maxCoinBonusCap;
    #endregion
}