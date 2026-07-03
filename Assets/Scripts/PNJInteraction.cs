using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PNJInteraction : MonoBehaviour
{
    [Header("Configuration UI")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_InputField secretInputField;
    [SerializeField] private Button validateButton;
    [SerializeField] private Button closeButton;

    [Header("Configuration PNJ")]
    [SerializeField] private string correctCode;
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private bool isInNiveau1 = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    private Transform playerTransform;
    private bool isPlayerNearby = false; // Représente maintenant STRICTEMENT la présence physique dans le rayon
    private bool _isCodeValidated = false;

    void Start()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        if (validateButton != null)
            validateButton.onClick.AddListener(TryValidateCode);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnPlayerExitZone);

        playerTransform = PlayerController.Instance.transform;
    }

    void Update()
    {
        if (playerTransform == null || _isCodeValidated) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isCurrentlyInsideRadius = distance <= detectionRadius;

        // Le joueur entre PHYSIQUEMENT dans la zone pour la première fois
        if (isCurrentlyInsideRadius && !isPlayerNearby)
        {
            isPlayerNearby = true; // On verrouille l'état "le joueur est là"
            OnPlayerEnterZone();
        }
        // Le joueur sort PHYSIQUEMENT de la zone
        else if (!isCurrentlyInsideRadius && isPlayerNearby)
        {
            isPlayerNearby = false; // On libère l'état pour une prochaine visite
            OnPlayerExitZone();
        }
    }

    private void OnPlayerEnterZone()
    {
        Debug.Log("<color=yellow>Le joueur est entré dans la zone d'interaction.</color>");

        if (isInNiveau1)
            PlayerController.Instance.SetCanMove(false);

        // On écoute la validation ET l'annulation
        PlayerController.Instance.OnValidatePressed += TryValidateCode;
        PlayerController.Instance.OnCancelPressed += OnPlayerExitZone;

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);

            if (isInNiveau1)
            {
                secretInputField.Select();
                secretInputField.ActivateInputField();
                UIPlayer.Instance.ManageCursor(true);
            }
        }
    }

    // Appelée par le bouton Fermer, la touche Annuler, ou quand le joueur s'éloigne physiquement
    private void OnPlayerExitZone()
    {
        Debug.Log("<color=yellow>Fermeture de l'interaction (Bouton, Annulation ou Éloignement).</color>");

        // ATTENTION : On ne passe plus "isPlayerNearby = false" ici ! 
        // Tant que le joueur ne recule pas physiquement, Update saura qu'il est encore là et ne réouvrira pas le menu.

        // Nettoyage sécurisé des abonnements
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnValidatePressed -= TryValidateCode;
            PlayerController.Instance.OnCancelPressed -= OnPlayerExitZone;

            // On redonne les mouvements au joueur
            PlayerController.Instance.SetCanMove(true);
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
            ClearFields();
            UIPlayer.Instance.ManageCursor(false);
        }
    }

    public void TryValidateCode()
    {
        if (secretInputField == null || _isCodeValidated) return;

        string playerInput = secretInputField.text;

        if (playerInput.Trim().ToLower() == correctCode.ToLower())
        {
            Debug.Log("<color=green>Succès ! Le code est correct.</color>");
            UIPlayer.Instance.UpdateStarAmount(1);
            _isCodeValidated = true;
            OnPlayerExitZone();
        }
        else
        {
            Debug.Log("<color=red>Code incorrect ! Réessaye.</color>");
            secretInputField.text = "";
            secretInputField.Select();
            secretInputField.ActivateInputField();
            audioSource.PlayOneShot(audioSource.clip); // Joue le son d'erreur
        }
    }

    private void ClearFields()
    {
        if (secretInputField != null)
            secretInputField.text = "";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}