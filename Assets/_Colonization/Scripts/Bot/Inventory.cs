using UnityEngine;

public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField] private Resource _resource;
    [SerializeField] private Transform _container;

    public bool IsFull => _resource != null;

    public void Add(Resource resource)
    {
        _resource = resource;

        Attach(_resource);
        SetTransform();
    }

    public Resource Drop()
    {
        Resource resource = _resource;
        Detach(_resource);
        _resource = null;

        return resource;
    }

    private void Attach(Resource resource) =>    
        resource.gameObject.transform.parent = _container.transform;    

    private static void Detach(Resource resource) =>    
        resource.gameObject.transform.parent = null;

    private void SetTransform()
    {
        _resource.transform.SetPositionAndRotation(_container.position, _container.rotation);
        _resource.transform.localScale = _container.localScale;
    }
}