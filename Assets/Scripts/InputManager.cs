using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PlayerInput _playerInput;

    Vector2 _playerMovementInput;
    float _mouseX;
    float _mouseY;
    bool _sprint;
    bool _lockOn;

    public float MouseXInput { get => _mouseX; set => _mouseX = value; }
    public float MouseYInput { get => _mouseY; set => _mouseY = value; }
    public Vector2 PlayerMovementInput { get => _playerMovementInput; set => _playerMovementInput = value; }
    public bool Sprint { get => _sprint; set => _sprint = value; }
    public bool LockOn { get => _lockOn; set => _lockOn = value; }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        SetupInput();

    }
    void SetupInput()
    {
        _playerInput.PlayerControls.MoveCamera.started += OnCameraMovementInput;
        _playerInput.PlayerControls.MoveCamera.canceled += OnCameraMovementInput;
        _playerInput.PlayerControls.MoveCamera.performed += OnCameraMovementInput;

        _playerInput.PlayerControls.MovePlayer.started += OnPlayerMovementInput;
        _playerInput.PlayerControls.MovePlayer.canceled += OnPlayerMovementInput;
        _playerInput.PlayerControls.MovePlayer.performed += OnPlayerMovementInput;

        _playerInput.PlayerControls.Sprint.started += OnSprintInput;
        _playerInput.PlayerControls.Sprint.canceled += OnSprintInput;

        _playerInput.PlayerControls.LockOn.started += OnLockOn;
        _playerInput.PlayerControls.LockOn.canceled += OnLockOn;
    }
    void OnCameraMovementInput(InputAction.CallbackContext context)
    {
        Vector2 currentCameraMovementInput = context.ReadValue<Vector2>();
        _mouseX = currentCameraMovementInput.x;
        _mouseY = currentCameraMovementInput.y;
    }
    void OnPlayerMovementInput(InputAction.CallbackContext context)
    {
        _playerMovementInput = context.ReadValue<Vector2>();
    }
    void OnSprintInput(InputAction.CallbackContext context)
    {
        _sprint = context.ReadValueAsButton();
    }
    void OnLockOn(InputAction.CallbackContext context)
    {
        _lockOn = context.ReadValueAsButton();
    }
    private void OnEnable()
    {
        _playerInput.PlayerControls.Enable();
    }
    private void OnDisable()
    {
        _playerInput.PlayerControls.Disable();
    }
}
