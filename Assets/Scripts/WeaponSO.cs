using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public int damage;
    public float attackCooldown;
    public float attackRange;
    public RuntimeAnimatorController animator;
    public WeaponType weaponType;

    public GameObject slashPrefab; // L'effet visuel du coup d'épée/dague
    public GameObject projectilePrefab; // Pour les armes magiques/arcs
}

public enum WeaponType
{
    Sword,
    Dagger,
    Staff
}