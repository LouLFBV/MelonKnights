using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform attackPoint;

    private Animator _playerAnimator;
    private PlayerInput _playerInput;

    public event Action<WeaponSO> OnWeaponEquipped;
    private WeaponSO _currentWeapon; 
    private WeaponVisual _currentWeaponVisual;

    private float _attackRange;
    private int _attackDamage;
    private float _attackCooldown;

    private float _nextAttackTime;

    private bool _canAttack = true;

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
        if (!_canAttack)
            return;

        if (Time.time < _nextAttackTime)
            return;

        _nextAttackTime = Time.time + _attackCooldown;

        _canAttack = false;
        _playerAnimator.SetTrigger("Attack");
    }

    public void AE_Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            _attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (!enemy.CompareTag("Boss"))
                continue;

            if (enemy.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(_attackDamage);
            }
        }
    }

    public void AE_OnAttackFinished()
    {
        _canAttack = true;
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        _currentWeapon = weapon;

        _attackDamage = weapon.damage;
        _attackRange = weapon.attackRange;
        _attackCooldown = weapon.attackCooldown;

        if (weapon.animator != null)
            _playerAnimator.runtimeAnimatorController = weapon.animator;

        PlayerController.Instance.EquipWeapon(weapon.weaponType);

        OnWeaponEquipped?.Invoke(weapon);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, _attackRange);
    }
}