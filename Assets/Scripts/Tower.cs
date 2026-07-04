using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Components")]
    [SerializeField] private HealthSystem healthSystem;
}