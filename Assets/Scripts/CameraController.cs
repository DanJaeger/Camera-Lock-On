using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    InputManager _inputManager;

    [Header("Framing:")]
    [SerializeField] Camera _camera = null;
    [SerializeField] Transform _followTransform = null;
    [SerializeField] Vector2 _framingNormal = Vector2.zero;

    [Header("Distance:")]
    [SerializeField] float _defaultTargetDistance = 5.0f;
    [SerializeField] float _maxTargetDistance = 7.0f;
    [SerializeField] float _minTargetDistance = 3.0f;

    [Header("Rotation:")]
    [SerializeField] bool _invertX = false;
    [SerializeField] bool _invertY = false;
    [SerializeField] float _rotationSharpness = 5.0f;
    [SerializeField] float _positionSharpness = 5.0f;
    [SerializeField] float _defaultVerticalAngle = 20.0f;    
    [SerializeField] [Range(-90.0f, 90.0f)] float _minVerticalAngle = -90.0f;
    [SerializeField] [Range(-90.0f, 90.0f)] float _maxVerticalAngle = 90.0f;

    [Header("Obstructions:")]
    [SerializeField] float _checkRadius = 0.2f;
    [SerializeField] LayerMask _obstructionLayers = -1;
    List<Collider> _ignoreColliders = new List<Collider>();

    [Header("Lock On")]
    [SerializeField] float _lockOnLossTime = 1.0f;
    [SerializeField] float _lockOnDistance = 15.0f;
    [SerializeField] LayerMask _lockOnLayer = -1;
    [SerializeField] Vector3 _lockOnFraming = Vector3.zero;
    [SerializeField] [Range(1,179)] float _lockOnFOV = 40.0f;

    bool _lockedOn = false;
    ITargetable _target;

    Vector3 _planarDirection = Vector3.zero;
    Vector3 _targetPosition;
    Quaternion _targetRotation = Quaternion.identity;
    float _targetVerticalAngle;
    float _targetDistance; 
    float _lockOnLossTimeCurrent = 0.0f;
    float _fovNormal;
    float _framingLerp;

    Vector3 _newPosition;
    Quaternion _newRotation;

    public Vector3 CameraPlanarDirection { get => _planarDirection; }
    public ITargetable Target { get => _target; set => _target = value; }
    public bool LockedOn { get => _lockedOn; set => _lockedOn = value; }

    private void OnValidate()
    {
        _defaultTargetDistance = Mathf.Clamp(_defaultTargetDistance, _minTargetDistance, _maxTargetDistance);
        _defaultVerticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
    }

    private void Awake()
    {
        _inputManager = GetComponent<InputManager>();
    }

    private void Start()
    {
        _ignoreColliders.AddRange(GetComponentsInChildren<Collider>());

        _fovNormal = _camera.fieldOfView;
        _planarDirection = _followTransform.forward;

        _targetDistance = _defaultTargetDistance;
        _targetVerticalAngle = _defaultVerticalAngle;
        _targetRotation = Quaternion.LookRotation(_planarDirection) * Quaternion.Euler(_targetVerticalAngle, 0, 0);
        _targetPosition = _followTransform.position - (_targetRotation * Vector3.forward) * _targetDistance;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Cursor.lockState != CursorLockMode.Locked)
            return;

        _lockedOn = _inputManager.LockOn;
        float mouseX = _inputManager.MouseXInput;
        float mouseY = _inputManager.MouseYInput;

        if (_invertX)
            mouseX *= -1.0f;
        if (_invertY)
            mouseY *= -1.0f;

        Vector3 framing = Vector3.Lerp(_framingNormal, _lockOnFraming, _framingLerp); 
        Vector3 focusPosition = _followTransform.position + _followTransform.TransformDirection(framing);
        float fov = Mathf.Lerp(_fovNormal, _lockOnFOV, _framingLerp);
        _camera.fieldOfView = fov;

        if (_lockedOn && _target != null)
        {
            Vector3 camToTarget = _target.TargetTransform.position - _camera.transform.position;
            Vector3 planarCamToTarget = Vector3.ProjectOnPlane(camToTarget, Vector3.up);
            Quaternion lookRotation = Quaternion.LookRotation(planarCamToTarget, Vector3.up);

            _framingLerp = Mathf.Clamp01(_framingLerp + Time.deltaTime * 4);
            _planarDirection = planarCamToTarget != Vector3.zero ? planarCamToTarget.normalized : _planarDirection;
            _targetVerticalAngle = Mathf.Clamp(lookRotation.eulerAngles.x, _minVerticalAngle, _maxVerticalAngle);
            _targetDistance = Mathf.Clamp(_targetDistance, _minTargetDistance, _maxTargetDistance);

            bool valid = _target.Targetable &&
                InDistance(_target) &&
                InScreen(_target) &&
                NotBlocked(_target);

            if (valid) { _lockOnLossTimeCurrent = 0; }
            else { _lockOnLossTimeCurrent = Mathf.Clamp(_lockOnLossTimeCurrent * Time.deltaTime, 0, _lockOnLossTime);}

            if(_lockOnLossTimeCurrent == _lockOnLossTime)
            {
                _lockedOn = false;
            }
        }
        else
        {
            _framingLerp = Mathf.Clamp01(_framingLerp - Time.deltaTime * 4);
            _planarDirection = Quaternion.Euler(0, mouseX, 0) * _planarDirection;
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle + mouseY, _minVerticalAngle, _maxVerticalAngle);
            _targetDistance = Mathf.Clamp(_targetDistance, _minTargetDistance, _maxTargetDistance);
        }

        //Obstruction 
        float smallestDistance = _targetDistance;
        RaycastHit[] hits = Physics.SphereCastAll(
            focusPosition, _checkRadius, _targetRotation * -Vector3.forward, _targetDistance, _obstructionLayers);
        if(hits.Length != 0)
        {
            foreach(RaycastHit hit in hits)
            {
                if (!_ignoreColliders.Contains(hit.collider))
                {
                    if(hit.distance < smallestDistance)
                    {
                        smallestDistance = hit.distance;
                    }
                }
            }
        }

        _targetRotation = Quaternion.LookRotation(_planarDirection) * Quaternion.Euler(_targetVerticalAngle, 0, 0);
        _targetPosition = focusPosition - (_targetRotation * Vector3.forward) * smallestDistance;

        _newRotation = Quaternion.Slerp(_camera.transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
        _newPosition = Vector3.Lerp(_camera.transform.position, _targetPosition, Time.deltaTime * _positionSharpness);

        _camera.transform.rotation = _newRotation;
        _camera.transform.position = _newPosition;
    }
    public void ToggleLockOn(bool toggle)
    {
        if (toggle == _lockedOn)
            return;

        //_lockedOn = !_lockedOn;

        if (_lockedOn)
        {
            List<ITargetable> targetables = new List<ITargetable>();
            Collider[] colliders = Physics.OverlapSphere(transform.position, _lockOnDistance, _lockOnLayer);

            foreach (Collider collider in colliders)
            {
                
                ITargetable targetable = collider.GetComponent<ITargetable>();
                if(targetable != null)
                    if (targetable.Targetable)
                        if (InScreen(targetable))
                            if (NotBlocked(targetable))
                                targetables.Add(targetable);

            }

            //Find closest target to the center of the screen
            float hypotenuse;
            float smallestDistance = Mathf.Infinity;
            ITargetable closestTargetable = null;
            foreach (ITargetable targetable in targetables)
            {
                hypotenuse = CalculateHypotenuse(targetable.TargetTransform.position);
                if(smallestDistance > hypotenuse)
                {
                    closestTargetable = targetable;
                    smallestDistance = hypotenuse;
                }
            }
            _target = closestTargetable;
            _lockedOn = closestTargetable != null;
        }
    }
    bool InDistance(ITargetable targetable)
    {
        float distance = Vector3.Distance(transform.position, targetable.TargetTransform.position);
        return distance <= _lockOnDistance;
    }

    bool InScreen(ITargetable targetable)
    {
        Vector3 viewPortPosition = _camera.WorldToViewportPoint(targetable.TargetTransform.position);

        if (!(viewPortPosition.x > 0) || !(viewPortPosition.x < 1)) { return false; }
        if (!(viewPortPosition.y > 0) || !(viewPortPosition.y < 1)) { return false; }
        if (!(viewPortPosition.z > 0))                              { return false; }

        return true;
    }
    bool NotBlocked(ITargetable targetable)
    {
        Vector3 origin = _camera.transform.position;
        Vector3 direction = targetable.TargetTransform.position - origin;

        float radius = 0.15f;
        float distance = direction.magnitude;
        bool notBlocked = !Physics.SphereCast(origin, radius, direction, out RaycastHit hit, distance, _obstructionLayers);

        return notBlocked;
    }
    float CalculateHypotenuse(Vector3 position)
    {
        float screenCenterX = _camera.pixelWidth / 2;
        float screenCenterY = _camera.pixelHeight / 2;

        Vector3 screenPoint = _camera.WorldToScreenPoint(position);
        float xDelta = screenCenterX - screenPoint.x; 
        float yDelta = screenCenterY - screenPoint.y;
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(xDelta, 2) + Mathf.Pow(yDelta, 2));

        return hypotenuse;
    }

}
