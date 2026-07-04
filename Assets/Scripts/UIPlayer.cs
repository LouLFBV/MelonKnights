using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class UIPlayer : MonoBehaviour
{
    public static UIPlayer Instance;

    [SerializeField] private GameObject panelMenu;

    [Header("Health Settings")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private TextMeshProUGUI currentHealthText;
    [SerializeField] private HealthSystem playerHealthSystem;


    [Header("XP Settings")]
    [SerializeField] private Image xpBarImage;
    [SerializeField] private TextMeshProUGUI xpLevelText;
    [SerializeField] private XpSystem xpSystem;


    [Header("Coins Settings")]
    [SerializeField] private TextMeshProUGUI coinText;
    private int _coinAmount = 0;


    [Header("Weapon Icon Settings")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private GameObject swordVisual;
    [SerializeField] private GameObject daggerVisual;
    [SerializeField] private GameObject staffVisual;


    [Header("Audio Settings")]
    [SerializeField] private AudioSource coinAudioSource;

    private PlayerInput _playerInput;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        ManageCursor(false);
        UpdateCoinText();
        UpdateHealthBar();
        UpdateCoinText();
        UpdateXpBar();
    }

    private void OnEnable()
    {
        _playerInput.actions["Pause"].performed += OnPausePerformed;

        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged += UpdateHealthBar;
        }

        if(xpSystem != null)
        {
            xpSystem.OnXpChanged += UpdateXpBar;
            xpSystem.OnLevelUp += UpdateXpLevelText;
        }

        if (playerAttack != null)
        {
            playerAttack.OnWeaponEquipped += UpdateIconWeapon;
        }
    }

    private void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.actions["Pause"].performed -= OnPausePerformed;
        }

        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= UpdateHealthBar;
        }

        if (xpSystem != null)
        {
            xpSystem.OnXpChanged -= UpdateXpBar;
            xpSystem.OnLevelUp -= UpdateXpLevelText;
        }

        if (playerAttack != null)
        {
            playerAttack.OnWeaponEquipped -= UpdateIconWeapon;
        }
    }

    private void UpdateIconWeapon(WeaponSO weaponSO)
    {
        if (weaponSO != null)
        {
            swordVisual.SetActive(false);
            daggerVisual.SetActive(false);
            staffVisual.SetActive(false);

            switch (weaponSO.weaponType)
            {
                case WeaponType.Sword:
                     swordVisual.SetActive(true);
                    break;
                case WeaponType.Dagger:
                    daggerVisual.SetActive(true);
                    break;
                case WeaponType.Staff:
                    staffVisual.SetActive(true);
                    break;
            }
        }
    }

    private void UpdateXpLevelText()
    {
        xpLevelText.text = xpSystem.levelPlayer.ToString();
    }

    private void UpdateXpBar()
    {
        xpBarImage.fillAmount = (float)xpSystem.currentXp / xpSystem.maxXp;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (!PlayerController.Instance.CanMove || PlayerController.Instance.JustReleasedThisFrame)
        {
            return;
        }

        SetPauseMenuState(!panelMenu.activeSelf);
    }

    public void AddCoin(int coinAdded)
    {
        _coinAmount += coinAdded;
        UpdateCoinText();
        coinAudioSource.PlayOneShot(coinAudioSource.clip);
    }

    private void UpdateCoinText()
    {
        coinText.text = _coinAmount.ToString();
    }

    public int GetCoin()
    {
        return _coinAmount;
    }

    private void UpdateHealthBar()
    {
        currentHealthText.text = $"{playerHealthSystem.currentHealth} / {playerHealthSystem.maxHealth}";
        healthBarImage.fillAmount = (float)playerHealthSystem.currentHealth / playerHealthSystem.maxHealth;
    }


    public void SetPauseMenuState(bool isActive)
    {
        panelMenu.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;

        ManageCursor(isActive);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ManageCursor(bool showCursor)
    {
        //Cursor.visible = showCursor;

        //if (showCursor)
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //}
        //else
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //}
    }
}