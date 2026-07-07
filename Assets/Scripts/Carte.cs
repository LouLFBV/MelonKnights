using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Carte : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI carteName;
    [SerializeField] private TextMeshProUGUI carteDescription;
    [SerializeField] private Image carteImage;
    [SerializeField] private Button carteButton;

    // NOUVEAU : Remplacement du constructeur par une méthode d'initialisation propre
    public void Setup(CarteSO carteSO, Action onClickAction)
    {
        if (carteSO == null)
        {
            Debug.LogError($"Il manque le ScriptableObject CarteSO sur {gameObject.name}");
            return;
        }

        // On remplit les textes et visuels de l'UI
        carteName.text = carteSO.carteName;
        carteDescription.text = carteSO.carteDescription;
        carteImage.sprite = carteSO.carteSprite;

        if (carteButton == null) return;

        carteButton.onClick.RemoveAllListeners();
        // Petite astuce : .Invoke() est plus safe si jamais onClickAction est null
        carteButton.onClick.AddListener(() => onClickAction?.Invoke());
    }
}