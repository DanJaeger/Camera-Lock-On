using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    CharacterController _characterController;
    Animator _animator;
    InputManager _inputManager;
    CameraController _cameraController;

    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    [Header("Movement:")]
    float _targetSpeed;
    float _newSpeed;
    Quaternion _newRotation;
    Quaternion _targetRotation;
    Vector3 _appliedMovement;
    Vector3 _currentMovementInput = Vector3.zero;
    Vector3 _currentMovement;
    Vector3 _cameraPlanarDirection;

     [Header("Sharpness:")]
    float _rotationSharpness = 10.0f;
    float _moveSharpness = 10.0f;

    bool _isMovementPressed;
    bool _strafing;
    bool _sprinting;
    float _strafeParameter;
    Vector3 _strafeParametersXZ;

    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public bool Strafing { get => _strafing; set => _strafing = value; }
    public InputManager InputManager { get => _inputManager; set => _inputManager = value; }
    public bool Sprinting { get => _sprinting; set => _sprinting = value; }
    public bool IsMovementPressed { get => _isMovementPressed; set => _isMovementPressed = value; }
    public float TargetSpeed { get => _targetSpeed; set => _targetSpeed = value; }
    public Quaternion TargetRotation { get => _targetRotation; set => _targetRotation = value; }
    public Vector3 CameraPlanarDirection { get => _cameraPlanarDirection; set => _cameraPlanarDirection = value; }
    public Quaternion NewRotation { get => _newRotation; set => _newRotation = value; }
    public float RotationSharpness { get => _rotationSharpness; set => _rotationSharpness = value; }
    public CameraController CameraController { get => _cameraController; set => _cameraController = value; }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _inputManager = GetComponent<InputManager>();
        _cameraController = GetComponent<CameraController>();
    }

    private void Start()
    {
        _animator.applyRootMotion = false;

        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();
    }
    private void Update()
    {
        _currentMovementInput = new Vector3(_inputManager.PlayerMovementInput.x, 0, _inputManager.PlayerMovementInput.y);
        _cameraPlanarDirection = _cameraController.CameraPlanarDirection;
        Quaternion _cameraPlanarRotation = Quaternion.LookRotation(_cameraPlanarDirection);
        _currentMovement = _cameraPlanarRotation * _currentMovementInput.normalized;

        _isMovementPressed = _currentMovementInput != Vector3.zero;
        _currentState.UpdateState();

        _newSpeed = Mathf.Lerp(_newSpeed, _targetSpeed, Time.deltaTime * _moveSharpness);
        _appliedMovement = _currentMovement * _newSpeed;
        _characterController.Move(_appliedMovement * Time.deltaTime);

        HandleAnimations();
    }

    //Use in others states except for strafing
    public void HandleInput()
    {
        _sprinting = _inputManager.Sprint && _isMovementPressed;
        _strafing = _inputManager.LockOn;
    }
    //Use in others states except for strafing
    public void HandleRotation()
    {
        if (_isMovementPressed)
        {
            _targetRotation = Quaternion.LookRotation(_currentMovement);
            _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
            transform.rotation = _newRotation;
        }
    }
    void HandleAnimations()
    {
        if (_strafing)
        {
            _strafeParameter = Mathf.Clamp01(_strafeParameter + Time.deltaTime * 4);
            _strafeParametersXZ = Vector3.Lerp(_strafeParametersXZ, _currentMovementInput * _newSpeed, _moveSharpness * Time.deltaTime);
        }
        else
        {
            _strafeParameter = Mathf.Clamp01(_strafeParameter - Time.deltaTime * 4);
            _strafeParametersXZ = Vector3.Lerp(_strafeParametersXZ, Vector3.forward * _newSpeed, _moveSharpness * Time.deltaTime);
        }
        _animator.SetFloat("Strafing", _strafeParameter);
        _animator.SetFloat("StrafeX", Mathf.Round(_strafeParametersXZ.x * 100.0f) / 100.0f);
        _animator.SetFloat("StrafeZ", Mathf.Round(_strafeParametersXZ.z * 100.0f) / 100.0f);
    }

}
