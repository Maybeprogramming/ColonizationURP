using System;
using System.Collections;
using UnityEngine;

public class ResourceScanner : MonoBehaviour
{
    [SerializeField] private Transform _scannerPoint;
    [SerializeField] private float _radius;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _timeBetweenScanning;

    [SerializeField] private ScanAnimator _animator;

    [SerializeField] private bool _isGizmosVisible;
    [SerializeField, Range(0, 1)] private float _transparencyGizmos;
    [SerializeField] private Color _gizmosColor;

    private WaitForSeconds _waitTimer;

    public event Action<Resource> ResourceFound;

    private void Start()
    {
        _waitTimer = new WaitForSeconds(_timeBetweenScanning);
        StartCoroutine(Scanning());
    }

    private void OnResourceFound(Resource resource)=>
        ResourceFound?.Invoke(resource);

    private void ScanLocation()
    { 
        var colliders = Physics.OverlapSphere(_scannerPoint.position, _radius, _layerMask);

        if (colliders.Length != 0)
        {
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out Resource resource))                
                    OnResourceFound(resource);                
            }
        }
    }

    private IEnumerator Scanning()
    {
        while (enabled)
        {
            _animator.Run();

            yield return _waitTimer;

            _animator.Stop();
            ScanLocation();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_isGizmosVisible)
        {
            Gizmos.color = new Color(_gizmosColor.r, _gizmosColor.g, _gizmosColor.b, _transparencyGizmos);

            Gizmos.DrawWireSphere(_scannerPoint.position, _radius);
        }
    }
}