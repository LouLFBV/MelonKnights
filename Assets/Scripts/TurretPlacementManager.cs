using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurretPlacementManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LayerMask obstacleLayer; // Pour éviter de poser une tour sur un mur/autre tour
    [SerializeField] private bool snapToGrid = true;   // Si tu veux aligner les tours proprement
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float overlapRadius = 0.1f;

    private TurretShopItem _activeItem;
    private GameObject _previewInstance;
    private SpriteRenderer _previewSpriteRenderer;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;

        // On s'abonne à l'événement de l'inventaire
        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.OnTurretSelected += OnTurretSelectedFromInventory;
        }

        // On s'abonne à l'événement de validation global du PlayerController (Clavier/Manette)
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnValidatePressed += ExecutePlacement;
            PlayerController.Instance.OnCancelPressed += CancelPlacement;
        }
    }

    private void OnDestroy()
    {
        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.OnTurretSelected -= OnTurretSelectedFromInventory;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnValidatePressed -= ExecutePlacement;
            PlayerController.Instance.OnCancelPressed -= CancelPlacement;
        }
    }

    private void Update()
    {
        // Si aucune tourelle n'est sélectionnée, on ne fait rien
        if (_activeItem == null) return;

        Vector3 targetPosition = snapToGrid ? SnapPosition(GetMouseWorldPosition()) : GetMouseWorldPosition();

        // 1. Met à jour la position de la preview visuelle
        if (_previewInstance != null)
        {
            _previewInstance.transform.position = targetPosition;

            bool isValid = CheckPlacementValidity(targetPosition);
            _previewSpriteRenderer.color = isValid ? new Color(1, 1, 1, 0.6f) : new Color(1, 0, 0, 0.6f);
        }

        // 2. Validation alternative (Clic gauche souris direct)
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceTurret(targetPosition);
        }

        // 3. Annulation (Clic droit souris OU touche Escape)
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    // Méthode intermédiaire sans paramètre pour s'adapter à OnValidatePressed
    private void ExecutePlacement()
    {
        if (_activeItem == null) return;

        Vector3 targetPosition = snapToGrid ? SnapPosition(GetMouseWorldPosition()) : GetMouseWorldPosition();
        TryPlaceTurret(targetPosition);
    }


    private void OnTurretSelectedFromInventory(TurretShopItem item)
    {
        // Nettoie uniquement l'aperçu précédent sans toucher à l'InventoryBar
        ClearPlacement();

        if (item == null || item.turretPrefab == null) return;

        _activeItem = item;

        _previewInstance = new GameObject("TurretPreview");
        _previewSpriteRenderer = _previewInstance.AddComponent<SpriteRenderer>();
        _previewSpriteRenderer.sprite = item.icon;
        _previewSpriteRenderer.sortingOrder = 10;
    }

    private void TryPlaceTurret(Vector3 position)
    {
        if (!CheckPlacementValidity(position))
        {
            Debug.LogWarning("Placement impossible ici !");
            return;
        }

        Instantiate(_activeItem.turretPrefab, position, Quaternion.identity);

        TurretShopItem placedItem = _activeItem;

        // 1. On nettoie l'état de placement
        ClearPlacement();

        // 2. On retire la tourelle et on repasse sur le slot 0 (arme)
        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.RemoveTurretSlot(placedItem);
            InventoryBar.Instance.SelectSlot(0);
        }
    }

    private void CancelPlacement()
    {
        ClearPlacement();

        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.SelectSlot(0);
        }
    }

    // Méthode dédiée uniquement à la réinitialisation locale du manager
    private void ClearPlacement()
    {
        _activeItem = null;
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
        }
    }

    private bool CheckPlacementValidity(Vector3 position)
    {
        // 1. On récupère TOUS les colliders présents dans le rayon
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, overlapRadius, obstacleLayer);

        // 2. On vérifie chaque élément touché un par un
        foreach (Collider2D hit in hits)
        {
            // Si on tombe sur un collider physique qui N'EST PAS un trigger...
            if (!hit.isTrigger)
            {
                // ... alors c'est un vrai obstacle (mur, base d'une autre tour). On bloque le placement !
                return false;
            }
        }

        // 3. Si on arrive ici, c'est qu'on a soit rien touché, soit touché UNIQUEMENT des triggers. Le placement est valide !
        return true;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos;

        // Si le CursorController existe, on récupère sa position (Souris ou Curseur Virtuel Manette)
        if (CursorController.Instance != null)
        {
            screenPos = CursorController.Instance.CurrentScreenPosition;
        }
        else
        {
            // Sécurité de secours si le CursorController n'est pas trouvé dans la scène
            screenPos = Input.mousePosition;
        }

        // On force la coordonnée Z à la distance de la caméra pour la projection 2D
        screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f; // On reste en 2D pure
        return worldPos;
    }

    private Vector3 SnapPosition(Vector3 rawPosition)
    {
        // Aligne la position sur une grille
        float snappedX = Mathf.Round(rawPosition.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(rawPosition.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, 0f);
    }
}