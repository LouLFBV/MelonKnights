using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlightOverlay; // Ex: un cadre lumineux à activer/désactiver
    public bool isOutil;

    private TurretShopItem _item;

    public void Setup(TurretShopItem item)
    {
        _item = item;

        if (iconImage != null) iconImage.sprite = item.icon;

        if(iconImage.sprite == null) iconImage.gameObject.SetActive(false);
        else iconImage.gameObject.SetActive(true);

        SetHighlighted(false);
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlightOverlay != null)
            highlightOverlay.SetActive(isHighlighted);
    }
}