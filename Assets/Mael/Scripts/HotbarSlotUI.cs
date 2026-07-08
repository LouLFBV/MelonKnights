using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI inputText;
    [SerializeField] private GameObject highlightOverlay; // Ex: un cadre lumineux à activer/désactiver
    public bool isOutil;

    private TurretShopItem _item;

    public void Setup(TurretShopItem item , int inputNumber)
    {
        _item = item;
        inputText.text = inputNumber.ToString();

        if (iconImage != null) iconImage.sprite = item.icon;

        if(iconImage.sprite == null) iconImage.gameObject.SetActive(false);
        else iconImage.gameObject.SetActive(true);

        SetHighlighted(false);
    }

    public void UpdateInputText(int inputNumber)
    {
        if (inputText != null)
        {
            inputText.text = inputNumber.ToString();
        }
    }
    public void SetHighlighted(bool isHighlighted)
    {
        Debug.Log($"HotbarSlotUI : SetHighlighted called with isHighlighted = {isHighlighted}");
        if (highlightOverlay != null)
            highlightOverlay.SetActive(isHighlighted);
    }
}