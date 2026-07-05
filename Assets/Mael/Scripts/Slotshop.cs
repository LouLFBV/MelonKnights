using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SlotShop : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private Image iconImage; // Glisse ici l'Image qui affiche la tourelle dans la case

    private Button _button;
    private TurretShopItem _item;
    private ShopSystem _shopSystem;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Setup(TurretShopItem item, ShopSystem shopSystem)
    {
        _item = item;
        _shopSystem = shopSystem;

        if (iconImage != null) iconImage.sprite = item.icon;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        // Le clic sur une case ne fait que sélectionner l'item pour l'afficher en détail à droite
        _shopSystem.SelectItem(_item);
    }
}