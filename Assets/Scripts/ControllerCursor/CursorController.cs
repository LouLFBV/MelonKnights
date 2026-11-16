using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{

    public static CursorController Instance { get; private set; }
    [Header("References")]
    [SerializeField] private GameObject _uiCursorVisual;
    [SerializeField] private RectTransform _cursorRectTransform;
    [SerializeField] private InputActionProperty _rightStickAction;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Camera _mainCamera; 

    [Header("Settings")]
    [SerializeField] private float _cursorSpeed = 1200f;

    private Vector2 _virtualScreenPos;

    public Vector3 CurrentScreenPosition
    {
        get
        {
            if (InputDeviceManager.Instance != null &&
                InputDeviceManager.Instance.CurrentScheme == ControlScheme.Gamepad)
            {
                return _virtualScreenPos;
            }
            else
            {
                return Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : _virtualScreenPos;
            }
        }
    }

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (_cursorRectTransform == null && _uiCursorVisual != null)
            _cursorRectTransform = _uiCursorVisual.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        _rightStickAction.action?.Enable();
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.OnControlSchemeChanged += OnControlSchemeChanged;
            UpdateScheme(InputDeviceManager.Instance.CurrentScheme);
        }
    }

    private void OnDisable()
    {
        _rightStickAction.action?.Disable();
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.OnControlSchemeChanged -= OnControlSchemeChanged;
        }
    }

    private void OnControlSchemeChanged(ControlScheme scheme)
    {
        UpdateScheme(scheme);
    }

    private void UpdateScheme(ControlScheme scheme)
    {
        bool isGamepad = (scheme == ControlScheme.Gamepad);

        if (isGamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            if (_uiCursorVisual != null) _uiCursorVisual.SetActive(true);

            if (Mouse.current != null)
            {
                _virtualScreenPos = Mouse.current.position.ReadValue();
            }
            else
            {
                _virtualScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            }

            if (_cursorRectTransform != null)
                _cursorRectTransform.position = _virtualScreenPos;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (_uiCursorVisual != null) _uiCursorVisual.SetActive(false);
        }
    }


    private void Update()
    {
        if (InputDeviceManager.Instance == null) return;

        if (InputDeviceManager.Instance.CurrentScheme == ControlScheme.Gamepad)
        {
            Vector2 stickValue = _rightStickAction.action.ReadValue<Vector2>();

            if (stickValue.sqrMagnitude > 0.01f)
            {
                // 1. Déplacement théorique
                _virtualScreenPos += stickValue * _cursorSpeed * Time.unscaledDeltaTime;

                // 2. Clamping par rapport au joueur (en coordonnées écran)
                if (_playerTransform != null)
                {
                    Camera cam = _mainCamera != null ? _mainCamera : Camera.main;
                    if (cam != null)
                    {
                        Vector2 playerScreenPos = cam.WorldToScreenPoint(_playerTransform.position);
                        Vector2 offset = _virtualScreenPos - playerScreenPos;
                    }
                }

                // 3. Clamping aux bords de l'écran
                _virtualScreenPos.x = Mathf.Clamp(_virtualScreenPos.x, 0, Screen.width);
                _virtualScreenPos.y = Mathf.Clamp(_virtualScreenPos.y, 0, Screen.height);

                // 4. Application au RectTransform
                if (_cursorRectTransform != null)
                {
                    _cursorRectTransform.position = _virtualScreenPos;
                }
            }
        }
    }
}