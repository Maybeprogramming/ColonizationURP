using UnityEngine;

[RequireComponent(typeof(CameraRotator))]
public class CameraMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _followSpeed;

    private CameraRotator _cameraRotator;
    private Vector3 _velocity;

    private void Awake()
    {
        _cameraRotator = GetComponent<CameraRotator>();
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = _target.position + _cameraRotator.GetOffset();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _followSpeed);
        transform.LookAt(_target);
    }
}
