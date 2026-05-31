using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private PlayerInputSystem _input;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Bounds _moveBounds;

    private void Update()
    {
        Vector3 input = _input.MoveDirection;

        Vector3 forward = _cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = _cameraTransform.right;
        right.y = 0;
        right.Normalize();

        Vector3 movement = forward * input.z + right * input.x;
        Vector3 newPosition = transform.position + movement * _moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, _moveBounds.min.x, _moveBounds.max.x);
        newPosition.z = Mathf.Clamp(newPosition.z, _moveBounds.min.z, _moveBounds.max.z);

        transform.position = newPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_moveBounds.center, _moveBounds.size);
    }
}
