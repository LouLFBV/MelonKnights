using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance;

    [Tooltip("Délai en secondes avant de réautoriser le passage à la souris après un mouvement manette.")]
    [SerializeField] private float _gamepadToKbMouseDelay = 1f;

    public ControlScheme CurrentScheme { get; private set; } = ControlScheme.KeyboardMouse;

    public static event Action<ControlScheme> OnControlSchemeChanged;

    private float _lastGamepadTime = -10f;
    private GameObject _lastSelectedObject;
    private GameObject _mousePressedObject;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        // 1. On mémorise en permanence le dernier élément valide sélectionné (utile si la manette reprend la main)
        if (current != null && current.activeInHierarchy)
        {
            _lastSelectedObject = current;
        }

        // 2. Empêche Unity de laisser un bouton sélectionné après un clic souris en mode KeyboardMouse
        if (CurrentScheme == ControlScheme.KeyboardMouse && Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _mousePressedObject = current;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (current != null && current == _mousePressedObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
                _mousePressedObject = null;
            }
        }
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        InputAction action = (InputAction)obj;
        InputControl control = action.activeControl;

        if (control == null) return;

        InputDevice device = control.device;

        if (device is Gamepad)
        {
            _lastGamepadTime = Time.unscaledTime;
            SetControlScheme(ControlScheme.Gamepad);
        }
        else if (device is Keyboard)
        {
            SetControlScheme(ControlScheme.KeyboardMouse);
        }
        else if (device is Mouse)
        {
            bool isClick = control is ButtonControl;
            bool delayPassed = (Time.unscaledTime - _lastGamepadTime) > _gamepadToKbMouseDelay;

            if (delayPassed || isClick)
            {
                SetControlScheme(ControlScheme.KeyboardMouse);
            }
        }
    }

    private void SetControlScheme(ControlScheme newScheme)
    {
        if (CurrentScheme == newScheme) return;

        CurrentScheme = newScheme;

        if (EventSystem.current != null)
        {
            if (CurrentScheme == ControlScheme.KeyboardMouse)
            {
                // Retire le focus visuel dès qu'on repasse sur souris/clavier
                EventSystem.current.SetSelectedGameObject(null);
            }
            else if (CurrentScheme == ControlScheme.Gamepad)
            {
                // Redonne le focus dès qu'on reprend la manette
                RestoreGamepadSelection();
            }
        }

        OnControlSchemeChanged?.Invoke(CurrentScheme);
    }

    public void SelectUI(GameObject target)
    {
        _lastSelectedObject = target;

        if (EventSystem.current == null) return;

        if (CurrentScheme == ControlScheme.Gamepad && target != null && target.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    private void RestoreGamepadSelection()
    {
        if (EventSystem.current == null) return;

        // Si un script (ex: ouverture de panneau) a déjà sélectionné un bouton valide, on ne touche à rien
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current != null && current.activeInHierarchy) return;

        // 1. Priorité : le dernier élément sélectionné s'il est toujours actif et interactif
        if (_lastSelectedObject != null && _lastSelectedObject.activeInHierarchy)
        {
            Selectable lastSelectable = _lastSelectedObject.GetComponent<Selectable>();
            if (lastSelectable != null && lastSelectable.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(_lastSelectedObject);
                return;
            }
        }

        // 2. Secours : si l'ancien menu a été fermé/détruit, on prend le premier bouton actif à l'écran
        foreach (Selectable selectable in Selectable.allSelectablesArray)
        {
            if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
                return;
            }
        }
    }
}

public enum ControlScheme
{
    KeyboardMouse,
    Gamepad
}