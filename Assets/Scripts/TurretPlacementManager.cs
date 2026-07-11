using UnityEngine;

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
    }

    private void OnDestroy()
    {
        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.OnTurretSelected -= OnTurretSelectedFromInventory;
        }
    }

    private void Update()
    {
        // Si aucune tourelle n'est sélectionnée, on ne fait rien
        if (_activeItem == null) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 targetPosition = snapToGrid ? SnapPosition(mouseWorldPos) : mouseWorldPos;

        // 1. Met à jour la position de la preview visuelle
        if (_previewInstance != null)
        {
            _previewInstance.transform.position = targetPosition;

            // Optionnel : Change la couleur de la preview (Rouge si bloqué, Blanc/Vert si OK)
            bool isValid = CheckPlacementValidity(targetPosition);
            _previewSpriteRenderer.color = isValid ? new Color(1, 1, 1, 0.6f) : new Color(1, 0, 0, 0.6f);
        }

        // 2. Clic gauche pour placer
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceTurret(targetPosition);
        }

        // 3. Clic droit pour annuler le placement
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    private void OnTurretSelectedFromInventory(TurretShopItem item)
    {
        // Si on change d'item ou qu'on sélectionne un outil/arme (item == null)
        CancelPlacement();

        if (item == null || item.turretPrefab == null) return;

        _activeItem = item;

        // Crée l'objet de preview (le fantôme qui suit la souris)
        _previewInstance = new GameObject("TurretPreview");
        _previewSpriteRenderer = _previewInstance.AddComponent<SpriteRenderer>();
        _previewSpriteRenderer.sprite = item.icon; // Utilise l'icône ou le sprite de la tourelle
        _previewSpriteRenderer.sortingOrder = 10;  // Toujours au-dessus du sol
    }

    private void TryPlaceTurret(Vector3 position)
    {
        if (!CheckPlacementValidity(position))
        {
            Debug.LogWarning("Placement impossible ici !");
            return;
        }

        // Apparition de la vraie tourelle
        Instantiate(_activeItem.turretPrefab, position, Quaternion.identity);

        // On stocke l'item qu'on vient de placer pour le supprimer de la barre
        TurretShopItem placedItem = _activeItem;

        // On nettoie le mode placement
        CancelPlacement();

        // On retire la tourelle de la barre d'inventaire
        InventoryBar.Instance.RemoveTurretSlot(placedItem);
    }

    private void CancelPlacement()
    {
        _activeItem = null;
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
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
        Vector3 mouseScreenPos = Input.mousePosition;
        // On force la coordonnée Z à la distance de la caméra pour la projection 2D
        mouseScreenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f; // On reste en 2D pure
        return worldPos;
    }

    private Vector3 SnapPosition(Vector3 rawPosition)
    {
        // Aligne la position sur une grille (ex: 0.5, 1.5, 2.5 ou des entiers selon ton repère)
        float snappedX = Mathf.Round(rawPosition.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(rawPosition.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, 0f);
    }
}