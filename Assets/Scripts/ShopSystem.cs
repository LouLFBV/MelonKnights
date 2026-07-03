using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;

    public void ButtonOpenShopPanel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}
