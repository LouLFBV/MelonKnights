using System;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("Piece Settings")]
    [SerializeField] private float distanceToMove = 1f;
    [SerializeField] private int coinAmount = 1;

    private Transform _playerPosition;
    void Start()
    {
        if (_playerPosition == null)
        {
            _playerPosition = PlayerController.Instance.transform;
        }
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, _playerPosition.position) <= distanceToMove)
        {
            CollectPiece();
        }
    }

    private void CollectPiece()
    {
        // M�thode qui va faire glisser la pi�ce vers le joueur et la d�truire ensuite
        // la vitesse de la pi�ce augmente sur la dur�e
        // UIPlayer.Instance.AddCoin(coinAmount);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToMove);
    }
}
