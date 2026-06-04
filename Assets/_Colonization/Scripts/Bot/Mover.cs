using System.Collections;
using UnityEngine;

public class Mover : MonoBehaviour, IMover
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _offsetDistance;

    private float _distance;
    private Vector3 _target;
    private Coroutine _moving;

    private void Start() =>    
        _target = transform.position;    

    public void MoveTo(Vector3 target)
    {
        if (_moving != null)
            StopCoroutine(_moving);

        _target = target;

        _moving = StartCoroutine(Moving());
    }

    public void Stop()
    {
        if (_moving != null)
        {
            StopCoroutine(_moving);
            _moving = null;
        }
    }

    public bool IsMovingComplete()
    {
        _distance = (transform.position - _target).sqrMagnitude;

        return _distance < _offsetDistance * _offsetDistance;
    }

    private IEnumerator Moving()
    {
        Vector3 direction = (_target - transform.position).normalized;
        transform.LookAt(new Vector3(_target.x, transform.position.y, _target.z));

        while (IsMovingComplete() == false)
        {
            transform.position += direction * _moveSpeed * Time.deltaTime;

            yield return null;
        }
    }
}