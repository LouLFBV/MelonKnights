using UnityEngine;

[CreateAssetMenu(fileName = "New Carte", menuName = "Carte")]
public class CarteSO : ScriptableObject
{
    public CarteType carteType;
    public string carteName;
    public string carteDescription;
    public Sprite carteSprite;
    public float boostValue;
}

public enum CarteType
{
    AttackSpeed,
    Damage,
    Health,
    MovementSpeed,
    Coin,
    XP,
    Heal
}