using UnityEngine;

[CreateAssetMenu(fileName = "New Tower", menuName = "Tower Defense/Tower")]
public class TowerSO : ScriptableObject
{
    public string towerName;
    public int damage;
    public int hp;
    public float stunDuration;
    public float attackSpeed;
    public float range;
    public GameObject towerPrefab;
    public GameObject projectile;
}