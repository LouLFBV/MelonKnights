using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UIPlayer : MonoBehaviour
{
    public static UIPlayer Instance;

    [SerializeField] private GameObject panelMenu;

    [Header("Health Settings")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private HealthSystem playerHealthSystem;

    [Header("Coins Settings")]
    [SerializeField] private TextMeshProUGUI coinText;
    private int _coinAmount;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource starAudioSource;

    public event System.Action OnStarAmountChanged;
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
    }

    private void OnEnable()
    {
        _playerInput.actions["Pause"].performed += OnPausePerformed;

        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged += UpdateHealthBar;
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
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (!PlayerController.Instance.CanMove || PlayerController.Instance.JustReleasedThisFrame)
        {
            return;
        }

        SetPauseMenuState(!panelMenu.activeSelf);
    }

    private void UpdateCoinAmount(int coinAdded)
    {
        _coinAmount += coinAdded;
        coinText.text = _coinAmount.ToString();
    }

    public int GetCoin()
    {
        return _coinAmount;
    }

    private void UpdateHealthBar()
    {
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
        Cursor.visible = showCursor;

        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}