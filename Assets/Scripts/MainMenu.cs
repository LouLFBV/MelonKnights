using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    private PlayerInput _playerInput;
    private bool _pauseMenuPressed = false;
    [SerializeField] private GameObject optionPanel;
    public void StartGame(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private void OnEnable()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput != null)
        {
            _playerInput.actions["Pause"].performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.actions["Pause"].performed -= OnPausePerformed;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        _pauseMenuPressed = true;
    }
    private void Update()
    {
        // Quitter le jeu si la touche Échap est pressée
        if (_pauseMenuPressed)
        {
            QuitGame();
            _pauseMenuPressed = false; // On reset l'input
        }
    }


    public void SetOptionPanelState()
    {
        optionPanel.SetActive(!optionPanel.activeSelf);
    }


    public void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}
