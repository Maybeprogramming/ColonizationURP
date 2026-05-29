using System.Collections;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Resource _resource;
    [SerializeField] private Transform _container;

    public bool IsFull => _resource != null;

    private Coroutine _animating;

    public void Add(Resource resource)
    {
        _resource = resource;

        _animating = StartCoroutine(PlayAnimation(_container.transform.position));
        Attach(_resource);
    }

    public Resource Drop(Vector3 target)
    {
        Resource resource = _resource;
        _animating = StartCoroutine(PlayAnimation(target));
        Detach(resource);
        _resource = null;

        return resource;
    }

    private void Attach(Resource resource) =>    
        resource.gameObject.transform.parent = _container.transform;    

    private static void Detach(Resource resource) =>    
        resource.gameObject.transform.parent = null;

    private IEnumerator PlayAnimation(Vector3 target)
    {
        _resource.transform.position = _container.position;
        _resource.transform.rotation = _container.rotation;
        _resource.transform.localScale = _container.localScale;

        yield return null;
    }
}