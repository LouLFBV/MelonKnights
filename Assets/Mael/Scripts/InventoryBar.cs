using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryBar : MonoBehaviour
{
    public static InventoryBar Instance { get; private set; }

    [Header("Équipement de base")]
    [SerializeField] private WeaponSO defaultTool;   // Ton outil (ex: Pioche)
    [SerializeField] private WeaponSO defaultWeapon; // Ton arme (ex: Épée)
    [SerializeField] private PlayerAttack playerAttack; // Le script du joueur pour l'équiper

    [Header("Slots fixes (Outil, Arme...) déjà présents dans l'UI")]
    [SerializeField] private int fixedSlotCount = 2;

    [Header("UI des slots de tourelles (dynamiques)")]
    [SerializeField] private Transform turretSlotsContainer; // Parent avec un Horizontal Layout Group
    [SerializeField] private GameObject turretSlotUIPrefab;   // Prefab d'une case de tourelle

    private readonly List<TurretShopItem> _turretSlots = new List<TurretShopItem>();
    private readonly List<HotbarSlotUI> _turretSlotUIs = new List<HotbarSlotUI>();

    private int _selectedIndex = -1; // -1 = rien, 0 = Outil, 1 = Arme, 2+ = tourelles

    // index global (0-based, slots fixes inclus)
    public event Action<int> OnSlotSelected;
    // item de la tourelle sélectionnée, ou null si un slot fixe (outil/arme) a été sélectionné
    public event Action<TurretShopItem> OnTurretSelected;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        DetectNumberKeyPress();
    }

    private void DetectNumberKeyPress()
    {
        if (Keyboard.current == null) return;

        // Digit1 à Digit9 = touches physiques du haut du clavier,
        // peu importe les symboles affichés dessus en AZERTY (&é"'(-è_ç)
        for (int i = 0; i < 9; i++)
        {
            Key key = Key.Digit1 + i;
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                SelectSlot(i);
                break;
            }
        }
    }

    public void SelectSlot(int index)
    {
        Debug.Log($"InventoryBar : Sélection du slot {index}");
        int totalSlots = fixedSlotCount + _turretSlots.Count;
        if (index < 0 || index >= totalSlots) return;

        _selectedIndex = index;
        OnSlotSelected?.Invoke(index);

        if (index < fixedSlotCount)
        {
            OnTurretSelected?.Invoke(null); // Slot fixe

            if (playerAttack != null)
            {
                if (index == 0 && defaultWeapon != null)
                {
                    Debug.Log("InventoryBar : Équipe l'arme par défaut");
                    playerAttack.EquipWeapon(defaultWeapon);
                }
                else if (index == 1 && defaultTool != null)
                {
                    Debug.Log("InventoryBar : Équipe l'arme par défaut");
                    playerAttack.EquipWeapon(defaultTool);
                }
                Debug.Log($"InventoryBar : Slot fixe sélectionné, index {index}, arme équipée : {playerAttack}");
            }
        }
        else
        {
            Debug.Log($"InventoryBar : Slot de tourelle sélectionné, index {index}, item : {_turretSlots[index - fixedSlotCount]?.name}");
            TurretShopItem item = _turretSlots[index - fixedSlotCount];
            OnTurretSelected?.Invoke(item);
        }

        RefreshHighlights();
    }

    // À ajouter dans le script InventoryBar
    public void SetDefaultWeapon(WeaponSO weapon)
    {
        defaultWeapon = weapon;
    }

    // Appelée par ShopSystem après un achat réussi
    public void AddTurretSlot(TurretShopItem item)
    {
        if (item == null) return;

        _turretSlots.Add(item);

        if (turretSlotUIPrefab != null && turretSlotsContainer != null)
        {
            GameObject slotObj = Instantiate(turretSlotUIPrefab, turretSlotsContainer);
            HotbarSlotUI slotUI = slotObj.GetComponent<HotbarSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(item);
                _turretSlotUIs.Add(slotUI);
            }
        }
    }

    // A appeler quand le joueur a effectivement utilisé/placé la tourelle sur la map (à brancher plus tard)
    public void RemoveTurretSlot(TurretShopItem item)
    {
        int slotIndex = _turretSlots.IndexOf(item);
        if (slotIndex == -1) return;

        _turretSlots.RemoveAt(slotIndex);

        if (slotIndex < _turretSlotUIs.Count)
        {
            Destroy(_turretSlotUIs[slotIndex].gameObject);
            _turretSlotUIs.RemoveAt(slotIndex);
        }

        _selectedIndex = -1;
        RefreshHighlights();
    }

    private void RefreshHighlights()
    {
        for (int i = 0; i < _turretSlotUIs.Count; i++)
        {
            bool isSelected = (i + fixedSlotCount) == _selectedIndex;
            _turretSlotUIs[i].SetHighlighted(isSelected);
        }
    }
}