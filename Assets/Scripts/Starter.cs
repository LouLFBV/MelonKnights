using UnityEngine;

public class Starter : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private WeaponSO sword;
    [SerializeField] private WeaponSO dagger;
    [SerializeField] private WeaponSO staff;

    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private GameObject panel;

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
        playerAttack.EquipWeapon(weapon);

        panel.SetActive(false);
    }
}