using UnityEngine;
using UnityEngine.InputSystem;

public class UIPlayer : MonoBehaviour
{
    public static UIPlayer Instance;

    [SerializeField] private GameObject[] healthBarImage;
    [SerializeField] private GameObject[] starImage;
    [SerializeField] private HealthSystem playerHealthSystem;
    [SerializeField] private GameObject panelMenu;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource starAudioSource;

    public event System.Action OnStarAmountChanged;
    private int _starAmount;
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

    public int GetStarAmount()
    {
        return _starAmount;
    }

    private void UpdateHealthBar()
    {
        for (int i = 0; i < healthBarImage.Length; i++)
        {
            healthBarImage[i].SetActive(false);
        }

        if (playerHealthSystem.currentHealth >= 0 && playerHealthSystem.currentHealth < healthBarImage.Length)
        {
            healthBarImage[playerHealthSystem.currentHealth].SetActive(true);
        }
    }

    public void UpdateStarAmount(int starAmount)
    {
        starAudioSource.PlayOneShot(starAudioSource.clip);
        _starAmount += starAmount;
        OnStarAmountChanged?.Invoke();
        for (int i = 0; i < starImage.Length; i++)
        {
            starImage[i].SetActive(false);
        }

        if (_starAmount >= 0 && _starAmount < starImage.Length)
        {
            starImage[_starAmount].SetActive(true);
        }
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