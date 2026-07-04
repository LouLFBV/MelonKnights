using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "ScriptableObjects/EnemySO", order = 1)]
public class EnemySO : ScriptableObject
{
    public int pv;
    public float speed;
    public int xpDrop;
    public int coinDrop;
    public EnemyType enemyType;
}

public enum EnemyType
{
    Melee,
    Ranged,
    Builder,
    Explosif,
    Boss
}
