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
        if (!canAttack || Time.time < _nextAttackTime || _currentWeapon == null || PlayerController.Instance.outilEquipped) return;

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

        // Déterminer la rotation fixe selon le point d'attaque
        Quaternion rotation = Quaternion.identity;

        WeaponPointAttack points = GetWeaponPoints(); // Petite méthode helper ci-dessous

        if (attackPoint == points.back) rotation = Quaternion.Euler(0, 0, 90);  // Haut
        else if (attackPoint == points.forward) rotation = Quaternion.Euler(0, 0, -90); // Bas
        else if (attackPoint == points.left) rotation = Quaternion.Euler(0, 0, 180); // Gauche
        else if (attackPoint == points.right) rotation = Quaternion.Euler(0, 0, 0);   // Droite

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
    private WeaponPointAttack GetWeaponPoints()
    {
        return _currentWeapon.weaponType switch
        {
            WeaponType.Sword => swordAttackPoints,
            WeaponType.Dagger => daggerAttackPoints,
            _ => staffAttackPoints
        };
    }
    private Transform GetActiveAttackPoint(Vector2 dir)
    {
        WeaponPointAttack activeWeaponPoints = GetWeaponPoints();
        // Calcul de l'angle en degrés
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360; // Pour avoir un cercle de 0 à 360

        if (angle >= 22.5f && angle < 67.5f) return activeWeaponPoints.right;   // →
        if (angle >= 67.5f && angle < 112.5f) return activeWeaponPoints.back;    // ↑
        if (angle >= 112.5f && angle < 157.5f) return activeWeaponPoints.left; // ←
        if (angle >= 157.5f && angle < 202.5f) return activeWeaponPoints.left;   // ←
        if (angle >= 202.5f && angle < 247.5f) return activeWeaponPoints.left; // ←
        if (angle >= 247.5f && angle < 292.5f) return activeWeaponPoints.forward; // ↓
        if (angle >= 292.5f && angle < 337.5f) return activeWeaponPoints.forward; // ↓
        return activeWeaponPoints.right; // → (Par défaut)
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