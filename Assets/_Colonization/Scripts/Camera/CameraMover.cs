using UnityEngine;

[RequireComponent(typeof(CameraRotator))]
public class CameraMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _followSpeed;

    private CameraRotator _cameraRotation;
    private Vector3 _velocity;

    private void Awake()
    {
        _cameraRotation = GetComponent<CameraRotator>();
    }

    private void OnValidate()
    {
        if (_target == null)
        {
            Debug.Log($"{_target}, Назначьте цель для камеры через инспектор");
        }
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = _target.position + _cameraRotation.GetOffset();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _followSpeed);
        transform.LookAt(_target);
    }
}
