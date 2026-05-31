using UnityEngine;

[RequireComponent(typeof(CameraRotator))]
public class CameraMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField, Range(5f, 50f)] private float _followSpeed;

    private CameraRotator _cameraRotator;

    private void Awake()
    {
        _cameraRotator = GetComponent<CameraRotator>();
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = _target.position + _cameraRotator.GetOffset();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, _followSpeed * Time.deltaTime);
        transform.LookAt(_target);
    }
}
