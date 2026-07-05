using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlightOverlay; // Ex: un cadre lumineux à activer/désactiver

    private TurretShopItem _item;

    public void Setup(TurretShopItem item)
    {
        _item = item;

        if (iconImage != null) iconImage.sprite = item.icon;

        SetHighlighted(false);
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlightOverlay != null)
            highlightOverlay.SetActive(isHighlighted);
    }
}