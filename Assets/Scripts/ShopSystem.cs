using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    [Header("Objets en vente")]
    [SerializeField] private List<TurretShopItem> turretItems = new List<TurretShopItem>();

    [Header("Grille de gauche (les cases cliquables)")]
    [SerializeField] private Transform slotsContainer; // Parent avec un Grid Layout Group dessus
    [SerializeField] private GameObject slotPrefab;     // Prefab du SlotShop

    [Header("Panneau détail (page de droite)")]
    [SerializeField] private GameObject detailPanel;    // Peut rester vide/inactif tant qu'aucune tourelle n'est sélectionnée
    [SerializeField] private Image detailIcon;
    [SerializeField] private TextMeshProUGUI detailTitleText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    [SerializeField] private TextMeshProUGUI detailPriceText;
    [SerializeField] private Button buyButton;

    [Header("Affichage des pièces du joueur")]
    [SerializeField] private TextMeshProUGUI currentCoinText;

    private readonly List<SlotShop> _slots = new List<SlotShop>();
    private TurretShopItem _selectedItem;

    private void Start()
    {
        GenerateSlots();

        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    private void OnEnable()
    {
        RefreshCoinDisplay();
    }

    private void GenerateSlots()
    {
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        _slots.Clear();

        foreach (TurretShopItem item in turretItems)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            SlotShop slot = slotObj.GetComponent<SlotShop>();

            if (slot == null)
            {
                Debug.LogError("Le slotPrefab n'a pas de composant SlotShop dessus !");
                continue;
            }

            slot.Setup(item, this);
            _slots.Add(slot);
        }
    }

    private void Update()
    {
        RefreshCoinDisplay();

        // Met à jour l'état du bouton Acheter en continu (au cas où le joueur ramasse des pièces)
        if (_selectedItem != null && buyButton != null)
        {
            buyButton.interactable = GetPlayerCoins() >= _selectedItem.cost;
        }
    }

    // Appelée par SlotShop quand le joueur clique sur une case de la grille
    public void SelectItem(TurretShopItem item)
    {
        if (item == null) return;

        _selectedItem = item;

        if (detailPanel != null)
            detailPanel.SetActive(true);

        if (detailIcon != null) detailIcon.sprite = item.icon;
        if (detailTitleText != null) detailTitleText.text = item.turretName;
        if (detailDescriptionText != null) detailDescriptionText.text = item.description;
        if (detailPriceText != null) detailPriceText.text = item.cost.ToString();

        if (buyButton != null)
            buyButton.interactable = GetPlayerCoins() >= item.cost;
    }

    private void RefreshCoinDisplay()
    {
        if (currentCoinText != null)
            currentCoinText.text = "Monnaie actuelle : " + GetPlayerCoins();
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