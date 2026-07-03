using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Configuration Déplacement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera mainCamera;


    [Header("Composants")]
    [SerializeField] private GameObject facePlayer;
    [SerializeField] private Animator panelTransitionAnimator; // Nom de la scène à charger
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private GameObject[] faceImage;

    private Rigidbody2D rb;
    public Animator animator;
    private Vector2 moveInput;
    private PlayerInput _playerInput;
    private InputAction moveAction;
    private InputAction valideAction;
    private InputAction cancelAction; 
    private bool _canMove = true;
    private int _currentFaceIndex = -1; 
    public bool CanMove => _canMove;

    public bool JustReleasedThisFrame { get; private set; }
    public event Action OnValidatePressed;
    public event Action OnCancelPressed; 

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
            animator.SetFloat("X", moveInput.x);
            animator.SetFloat("Y", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
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
            healthSystem.OnHealthChanged += ChangeFace;
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
            cancelAction.performed -= OnCancelPerformed; // <--- NOUVEAU
        }
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= PlayerDeath;
            healthSystem.OnHealthChanged -= ChangeFace;
        }
    }


    private void ChangeFace()
    {
        // Sécurité : on vérifie que le tableau contient bien des objets
        if (faceImage == null || faceImage.Length == 0)
        {
            Debug.LogWarning("Le tableau faceImage est vide ou non assigné dans l'inspecteur !");
            return;
        }

        int randomIndex = _currentFaceIndex;

        // Si on a plusieurs visages disponibles, on cherche un index DIFFÉRENT du précédent
        if (faceImage.Length > 1)
        {
            while (randomIndex == _currentFaceIndex)
            {
                randomIndex = UnityEngine.Random.Range(0, faceImage.Length);
            }
        }
        else
        {
            // S'il n'y a qu'un seul visage dans le tableau, on n'a pas le choix de toute façon
            randomIndex = 0;
        }

        // On mémorise ce nouvel index pour le prochain changement
        _currentFaceIndex = randomIndex;

        // Boucle pour activer le nouveau visage et éteindre les autres
        for (int i = 0; i < faceImage.Length; i++)
        {
            if (faceImage[i] != null)
            {
                faceImage[i].SetActive(i == randomIndex);
            }
        }
    }

    private void OnValidePerformed(InputAction.CallbackContext context)
    {
        OnValidatePressed?.Invoke();
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnCancelPressed?.Invoke(); // <--- NOUVEAU : On prévient qu'on veut quitter
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
        facePlayer.SetActive(false);
        SetCanMove(false);
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    public void AE_OnPlayerDeath()
    {
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
}