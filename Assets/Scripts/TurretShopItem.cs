using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretItem", menuName = "Shop/Turret Item")]
public class TurretShopItem : ScriptableObject
{
    [Header("Infos affichées dans le shop")]
    public string turretName;
    [TextArea(3, 6)] public string description;
    public Sprite icon;

    [Header("Gameplay")]
    public GameObject turretPrefab;
    public int cost = 10;
}