using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarteSystem : MonoBehaviour
{
    [SerializeField] private XpSystem playerXPSystem;
    [SerializeField] private HealthSystem playerHealthSystem;
    [SerializeField] private PlayerAttack playerAtatck;
    [SerializeField] private CarteSO[] carteSOs;

    [Header("UI")]
    [SerializeField] private GameObject cartePrefab;
    [SerializeField] private GameObject cartesPanel;
    [SerializeField] private Transform cartesParent;
    [SerializeField] private CanvasGroup canvasGroupParent;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        cartesPanel.SetActive(false);
        canvasGroupParent.alpha = 0f;
    }

    private void OnEnable()
    {
        if (playerXPSystem != null)
            playerXPSystem.OnLevelUp += OnLevelUp;
    }

    private void OnDisable()
    {
        if (playerXPSystem != null)
            playerXPSystem.OnLevelUp -= OnLevelUp;
    }

    private void OnLevelUp()
    {
        if(cartesPanel.activeSelf)
            return;
        GenerateCarteButtons();
        FadeIn();
    }

    private void GenerateCarteButtons()
    {
        foreach (Transform child in cartesParent)
        {
            Destroy(child.gameObject);
        }

        // 1. On prépare la liste des cartes disponibles
        List<CarteSO> availableCards = new List<CarteSO>();

        // 2. FILTRAGE : On ajoute uniquement les cartes qui NE sont PAS au plafond
        foreach (CarteSO carte in carteSOs)
        {
            if (!IsCardMaxed(carte))
            {
                availableCards.Add(carte);
            }
        }

        // 3. On s'assure qu'il reste des cartes à proposer (au cas où le joueur a TOUT maxé !)
        if (availableCards.Count == 0)
        {
            Debug.Log("Incroyable, tu as absolument maxé toutes les cartes du jeu !");
            // Optionnel : Tu peux donner de l'or/monnaie bonus à la place ici si tu veux
            FadeOut();
            return;
        }

        // 4. Tirage aléatoire parmi les cartes valides restantes
        int cardCount = Mathf.Min(3, availableCards.Count);

        for (int i = 0; i < cardCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);

            CarteSO selectedCard = availableCards[randomIndex];
            availableCards.RemoveAt(randomIndex);

            GameObject card = Instantiate(cartePrefab, cartesParent);
            Carte carteScript = card.GetComponent<Carte>();

            if (carteScript != null)
            {
                carteScript.Setup(selectedCard, () => ButtonAction(selectedCard));
            }
        }
    }

    // Nouvelle fonction Helper pour centraliser les vérifications de plafonds
    private bool IsCardMaxed(CarteSO carteSO)
    {
        switch (carteSO.carteType)
        {
            case CarteType.AttackSpeed:
                return playerAtatck.IsAttackSpeedMaxed();

            case CarteType.Damage:
                return playerAtatck.IsDamageMaxed();

            case CarteType.Health:
                return playerHealthSystem.IsHealthCardMaxed();

            case CarteType.XP:
                return playerXPSystem.IsXpCardMaxed();

            case CarteType.MovementSpeed:
                return PlayerController.Instance.IsMovementSpeedMaxed();

            case CarteType.Coin:
                return PlayerController.Instance.IsCoinMaxed();

            default:
                return false;
        }
    }

    private void FadeIn()
    {
        cartesPanel.SetActive(true);


        canvasGroupParent.interactable = true;
        canvasGroupParent.blocksRaycasts = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvas(0f, 1f));
    }

    private void FadeOut()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return FadeCanvas(1f, 0f);

        canvasGroupParent.interactable = false;
        canvasGroupParent.blocksRaycasts = false;

        cartesPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private IEnumerator FadeCanvas(float start, float end)
    {
        float timer = 0f;

        canvasGroupParent.alpha = start;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            canvasGroupParent.alpha = Mathf.Lerp(start, end, timer / fadeDuration);

            yield return null;
        }

        canvasGroupParent.alpha = end;
    }

    private void ButtonAction(CarteSO carteSO)
    {
        switch (carteSO.carteType)
        {
            case CarteType.AttackSpeed:
                playerAtatck.IncreaseAttackSpeed(carteSO.boostValue);
                break;

            case CarteType.Damage:
                playerAtatck.IncreaseDamage(carteSO.boostValue);
                break;

            case CarteType.Health:
                playerHealthSystem.IncreaseHealth(carteSO.boostValue);
                break;

            case CarteType.MovementSpeed:
                PlayerController.Instance.IncreaseMovementSpeed(carteSO.boostValue);
                break;

            case CarteType.Coin:
                PlayerController.Instance.IncreaseCoin(carteSO.boostValue);
                break;

            case CarteType.XP:
                playerXPSystem.IncreaseXP(carteSO.boostValue);
                break;

            default:
                Debug.LogWarning($"Carte type {carteSO.carteType} non géré.");
                break;
        }

        FadeOut();
    }
}