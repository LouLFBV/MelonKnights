using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    [Header("Objets en vente (même ordre que les slots ci-dessous)")]
    [SerializeField] private List<TurretShopItem> turretItems = new List<TurretShopItem>();

    [Header("Slots placés manuellement dans la grille (même ordre que les items ci-dessus)")]
    [SerializeField] private List<SlotShop> slots = new List<SlotShop>();

    [Header("Panneau détail (page de droite)")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TextMeshProUGUI detailTitleText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    [SerializeField] private TextMeshProUGUI detailPriceText;
    [SerializeField] private Button buyButton;

    [Header("Affichage des pièces du joueur")]
    [SerializeField] private TextMeshProUGUI currentCoinText;

    private TurretShopItem _selectedItem;

    private void Start()
    {
        AssignItemsToSlots();

        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    private void OnEnable()
    {
        RefreshCoinDisplay();
    }

    private void AssignItemsToSlots()
    {
        int count = Mathf.Min(turretItems.Count, slots.Count);

        for (int i = 0; i < count; i++)
        {
            slots[i].Setup(turretItems[i], this);
        }

        if (turretItems.Count != slots.Count)
        {
            Debug.LogWarning($"ShopSystem : {turretItems.Count} items pour {slots.Count} slots. " +
                              "Ajoute/enlève des slots ou des items pour que ça matche.");
        }
    }

    private void Update()
    {
        RefreshCoinDisplay();

        if (_selectedItem != null && buyButton != null)
        {
            buyButton.interactable = GetPlayerCoins() >= _selectedItem.cost;
        }
    }

    // Appelée par SlotShop quand le joueur clique sur une case
    public void SelectItem(TurretShopItem item)
    {
        if (item == null) return;

        _selectedItem = item;

        if (detailPanel != null)
            detailPanel.SetActive(true);

        if (detailIcon != null) detailIcon.sprite = item.icon;
        if (detailTitleText != null) detailTitleText.text = item.turretName;
        if (detailDescriptionText != null) detailDescriptionText.text = item.description;
        if (detailPriceText != null) detailPriceText.text = "Acheter : " + item.cost;

        if (buyButton != null)
            buyButton.interactable = GetPlayerCoins() >= item.cost;
    }

    private void RefreshCoinDisplay()
    {
        if (currentCoinText != null)
            currentCoinText.text = GetPlayerCoins().ToString();
    }

    private int GetPlayerCoins()
    {
        if (UIPlayer.Instance == null) return 0;
        return UIPlayer.Instance.GetCoin();
    }

    private void OnBuyButtonClicked()
    {
        if (_selectedItem == null) return;
        TryPurchase(_selectedItem);
    }

    public bool TryPurchase(TurretShopItem item)
    {
        if (UIPlayer.Instance == null || item == null) return false;

        int playerCoins = GetPlayerCoins();
        if (playerCoins < item.cost)
        {
            Debug.Log("Pas assez de pièces pour acheter " + item.turretName);
            return false;
        }

        bool success = UIPlayer.Instance.SpendCoin(item.cost);
        if (!success) return false;

        Debug.Log($"Achat réussi : {item.turretName} (-{item.cost} pièces)");

        // TODO : logique de placement de la tourelle (ex: activer un mode "fantôme"
        // qui suit la souris jusqu'à ce que le joueur clique sur une case valide)

        return true;
    }
}