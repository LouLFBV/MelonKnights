using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Weapons Settings")]
    [SerializeField] private WeaponPointAttack swordAttackPoints;
    [SerializeField] private WeaponPointAttack daggerAttackPoints;
    [SerializeField] private WeaponPointAttack staffAttackPoints;

    private PlayerInput _playerInput;

    public event Action<WeaponSO> OnWeaponEquipped;
    private WeaponSO _currentWeapon;

    private float _attackRange;
    private int _attackDamage;
    private float _attackCooldown;
    private float _nextAttackTime;

    // Gardé au cas où tu as besoin de bloquer l'attaque depuis un autre script (ex: si le joueur est étourdi)
    public bool canAttack = true;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable() => _playerInput.actions["Attack"].performed += OnAttack;
    private void OnDisable() => _playerInput.actions["Attack"].performed -= OnAttack;

    private void OnAttack(InputAction.CallbackContext context)
    {
        // 1. Vérification du cooldown et si on est autorisé à attaquer
        if (!canAttack || Time.time < _nextAttackTime || _currentWeapon == null) return;

        // 2. On met à jour le timer pour le prochain coup
        _nextAttackTime = Time.time + _attackCooldown;

        // 3. On lance l'attaque IMMÉDIATEMENT
        PerformAttack();
    }

    // Anciennement AE_Attack, maintenant appelée directement par l'Input
    private void PerformAttack()
    {
        Vector2 dir = PlayerController.Instance.LastDirection;
        Transform activeAttackPoint = GetActiveAttackPoint(dir);

        if (_currentWeapon.weaponType != WeaponType.Sword)
        {
            HandleRangedAttack(dir, activeAttackPoint);
        }
        else
        {
            HandleMeleeAttack(dir, activeAttackPoint);
        }
    }

    private void HandleRangedAttack(Vector2 dir, Transform attackPoint)
    {
        if (_currentWeapon.projectilePrefab == null || attackPoint == null) return;

        // On instancie SANS rotation (Quaternion.identity)
        GameObject proj = Instantiate(_currentWeapon.projectilePrefab, attackPoint.position, Quaternion.identity);

        if (_currentWeapon.weaponType == WeaponType.Dagger)
        {
            Transform childTransform = proj.transform.GetChild(0);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            childTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (proj.TryGetComponent<Projectile>(out var p))
        {
            p.Initialize(dir);
        }
    }

    private void HandleMeleeAttack(Vector2 dir, Transform attackPoint)
    {
        if (attackPoint == null) return;

        PlaySlashVisual(dir, attackPoint);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, _attackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Boss") && enemy.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(_attackDamage);
            }
        }
    }

    private void PlaySlashVisual(Vector2 dir, Transform attackPoint)
    {
        if (_currentWeapon.slashPrefab == null || attackPoint == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        Instantiate(_currentWeapon.slashPrefab, attackPoint.position, rotation, transform);
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        _currentWeapon = weapon;
        _attackDamage = weapon.damage;
        _attackRange = weapon.attackRange;
        _attackCooldown = weapon.attackCooldown;

        PlayerController.Instance.EquipWeapon(weapon.weaponType);
        OnWeaponEquipped?.Invoke(weapon);
    }

    private Transform GetActiveAttackPoint(Vector2 dir)
    {
        WeaponPointAttack activeWeaponPoints = swordAttackPoints;

        switch (_currentWeapon.weaponType)
        {
            case WeaponType.Sword: activeWeaponPoints = swordAttackPoints; break;
            case WeaponType.Dagger: activeWeaponPoints = daggerAttackPoints; break;
            case WeaponType.Staff: activeWeaponPoints = staffAttackPoints; break;
        }

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? activeWeaponPoints.right : activeWeaponPoints.left;
        }
        else
        {
            return dir.y > 0 ? activeWeaponPoints.back : activeWeaponPoints.forward;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _currentWeapon == null || PlayerController.Instance == null) return;

        Transform activePoint = GetActiveAttackPoint(PlayerController.Instance.LastDirection);

        if (activePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(activePoint.position, _attackRange);
        }
    }
}

[Serializable]
public class WeaponPointAttack
{
    public Transform forward, back, left, right;
}