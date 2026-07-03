using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Transform attackPoint;

    private bool _canAttack = true;
    private Animator _playerAnimator;
    private PlayerInput _playerInput;
    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
    }
    private void OnEnable()
    {
        _playerInput.actions["Attack"].performed += OnAttack;
    }
    private void OnDisable()
    {
        _playerInput.actions["Attack"].performed -= OnAttack;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!_canAttack) return;
        _playerAnimator.SetTrigger("Attack");
        _canAttack = false;
    }

    public void AE_Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Boss"))
            {
                if (enemy.TryGetComponent<HealthSystem>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void AE_OnAttackFinished() => _canAttack = true;

    private void OnDrawGizmos()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
