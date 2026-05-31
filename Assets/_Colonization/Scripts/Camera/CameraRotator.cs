using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private PlayerInputSystem _input;
    [SerializeField, Range(0.1f, 10f)] private float _horizontalSensitivity;
    [SerializeField, Range(0.1f, 10f)] private float _verticalSensitivity;
    [SerializeField, Range(0.1f, 10f)] private float _scrollSensitivity;
    [SerializeField, Range(0.1f, 10f)] private float _horizontalSmoothness;
    [SerializeField, Range(0.1f, 10f)] private float _verticalSmoothness;
    [SerializeField, Range(0.1f, 10f)] private float _scrollSmoothness;
    [SerializeField] private float _minVerticalAngle;
    [SerializeField] private float _maxVerticalAngle;
    [SerializeField] private float _minDistance;
    [SerializeField] private float _maxDistance;

    private float _currentHorizontal;
    private float _currentVertical;
    private float _currentDistance;
    private float _targetHorizontal;
    private float _targetVertical;
    private float _targetDistance;

    private Vector2 _lookDelta => _input.LookDelta;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        HandleRotation();
        HandleZoom();
        HandleSmothing();
    }

    private void HandleZoom()
    {
        _targetDistance -= _input.ScrollDelta.y * _scrollSensitivity;
        _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);        
    }

    private void HandleSmothing()
    {
        _currentHorizontal = Mathf.Lerp(_currentHorizontal, _targetHorizontal, _horizontalSmoothness * Time.deltaTime);
        _currentVertical = Mathf.Lerp(_currentVertical, _targetVertical, _verticalSmoothness * Time.deltaTime);
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, _scrollSmoothness * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (_input.IsRightMousePressed)
        {
            _targetHorizontal += _lookDelta.x * _horizontalSensitivity;
            _targetVertical -= _lookDelta.y * _verticalSensitivity;
            _targetVertical = Mathf.Clamp(_targetVertical, _minVerticalAngle, _maxVerticalAngle);
        }
        else
        {
            _targetHorizontal = _currentHorizontal;
            _targetVertical = _currentVertical;
        }
    }

    private void Init()
    {
        Vector3 offset = transform.position - _target.position;
        _currentDistance = offset.magnitude;
        _targetDistance = _currentDistance;

        Vector3 direction = offset.normalized;

        _currentHorizontal = Mathf.Atan2(-direction.x, -direction.z) * Mathf.Rad2Deg;
        _currentVertical = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;

        _targetHorizontal = _currentHorizontal;
        _targetVertical = _currentVertical;
    }

    public Vector3 GetOffset()
    {
        Quaternion rotation = Quaternion.Euler(_currentVertical, _currentHorizontal, Vector3.zero.z);

        return rotation * Vector3.back * _currentDistance;
    }
}
