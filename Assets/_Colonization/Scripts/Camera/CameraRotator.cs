using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float _horizontalSensitivity;
    [SerializeField] private float _verticalSensitivity;
    [SerializeField] private float _scrollSensitivity;
    [SerializeField, Range(0, 1)] private float _horizontalSmoothness;
    [SerializeField, Range(0, 1)] private float _verticalSmoothness;
    [SerializeField, Range(0, 1)] private float _scrollSmoothness;
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
    private const string MouseX = "Mouse X";
    private const string MouseY = "Mouse Y";
    private const string MouseScrollWheel = "Mouse ScrollWheel";
    private float _multiplierDistance;

    private void Start()
    {
        _multiplierDistance = 0.5f;
        _targetDistance = (_minDistance + _maxDistance) * _multiplierDistance;
        _currentDistance = _targetDistance;
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            _targetHorizontal += Input.GetAxis(MouseX) * _horizontalSensitivity;
            _targetVertical -= Input.GetAxis(MouseY) * _verticalSensitivity;
            _targetVertical = Mathf.Clamp(_targetVertical, _minVerticalAngle, _maxVerticalAngle);
        }

        _targetDistance -= Input.GetAxis(MouseScrollWheel) * _scrollSensitivity;
        _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);

        _currentHorizontal = Mathf.Lerp(_currentHorizontal, _targetHorizontal, _horizontalSmoothness);
        _currentVertical = Mathf.Lerp(_currentVertical, _targetVertical, _verticalSmoothness);
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, _scrollSmoothness);
    }

    public Vector3 GetOffset()
    {
        Quaternion rotation = Quaternion.Euler(_currentVertical, _currentHorizontal, Vector3.zero.z);

        return rotation * Vector3.back * _currentDistance;
    }
}
