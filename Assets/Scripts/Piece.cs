using System.Collections;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("Piece Settings")]
    [SerializeField] private float distanceToMove = 1f;
    [SerializeField] private int coinAmount = 1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 2f;

    private Transform _playerPosition;
    private bool _isCollecting = false;

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerPosition = PlayerController.Instance.transform;
        }
    }

    void Update()
    {
        // On vérifie la distance et si on n'est pas déjà en train de ramasser
        if (!_isCollecting && _playerPosition != null &&
            Vector3.Distance(transform.position, _playerPosition.position) <= distanceToMove)
        {
            StartCoroutine(CollectRoutine());
        }
    }

    private IEnumerator CollectRoutine()
    {
        _isCollecting = true;
        float currentSpeed = moveSpeed;

        // Tant que la pièce n'est pas très proche du joueur
        while (Vector3.Distance(transform.position, _playerPosition.position) > 0.1f)
        {
            // Déplacement vers le joueur
            transform.position = Vector3.MoveTowards(transform.position, _playerPosition.position, currentSpeed * Time.deltaTime);

            // Augmentation de la vitesse
            currentSpeed += acceleration * Time.deltaTime;

            yield return null;
        }

        // Ajout des coins et destruction
        UIPlayer.Instance.AddCoin(coinAmount);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToMove);
    }
}