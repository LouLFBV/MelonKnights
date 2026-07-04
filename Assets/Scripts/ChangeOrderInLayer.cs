using UnityEngine;

public class ChangeOrderInLayer : MonoBehaviour
{
    [SerializeField] private Transform transformPosition;
    [SerializeField] private int orderInLayerDown = 9;
    [SerializeField] private int orderInLayerUp = 12;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        int targetOrder = playerTransform.position.y < transformPosition.position.y
            ? orderInLayerDown
            : orderInLayerUp;

        if (spriteRenderer.sortingOrder != targetOrder)
            spriteRenderer.sortingOrder = targetOrder;
    }
}
