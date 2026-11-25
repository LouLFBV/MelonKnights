using UnityEngine;

public class Starter : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private WeaponSO sword;
    [SerializeField] private GameObject _swordButton;
    [SerializeField] private WeaponSO dagger;
    [SerializeField] private WeaponSO staff;

    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private GameObject panel;

    // NOUVEAU : On garde en mémoire l'arme que le joueur a choisie
    private WeaponSO _chosenWeapon;

    private void Start()
    {
        panel.SetActive(true);
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.SelectUI(_swordButton);
        }
    }

    public void SelectSword()
    {
        EquipWeapon(sword);
    }

    public void SelectDagger()
    {
        EquipWeapon(dagger);
    }

    public void SelectStaff()
    {
        EquipWeapon(staff);
    }

    private void EquipWeapon(WeaponSO weapon)
    {
        _chosenWeapon = weapon; // NOUVEAU : On se souvient de l'arme
        playerAttack.EquipWeapon(weapon);

        // NOUVEAU : On transmet cette arme à l'InventoryBar pour le Slot 1
        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.SetDefaultWeapon(weapon);

            // Optionnel : On sélectionne automatiquement le slot de l'arme (index 1) au départ
            InventoryBar.Instance.SelectSlot(0);
        }

        PlayerController.Instance.SetCanMove(true);
        panel.SetActive(false);
    }

    // NOUVEAU : La fonction à attribuer au bouton de ton icône en haut à gauche !
    public void OnTopLeftIconClicked()
    {
        Debug.Log("Starter : Icône en haut à gauche cliquée !");
        if (_chosenWeapon == null) return;

        // Au lieu de juste l'équiper brutalement, on demande à l'InventoryBar 
        // de sélectionner le slot numéro 1 (l'arme).
        // Comme ça, la surbrillance (Highlight) de la barre du bas se met à jour AUSSI !
        if (InventoryBar.Instance != null)
        {
            InventoryBar.Instance.SelectSlot(0);
        }
        else
        {
            // Sécurité au cas où l'InventoryBar n'est pas encore là
            playerAttack.EquipWeapon(_chosenWeapon);
        }
    }
}