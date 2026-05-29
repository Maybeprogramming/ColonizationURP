using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourcesData : MonoBehaviour
{
    [SerializeField] private List<Resource> _resourcesAvailable;
    [SerializeField] private List<Resource> _resourcesReserved;

    private void Start()
    {
        _resourcesAvailable = new List<Resource>();
        _resourcesReserved = new List<Resource>();
    }

    public bool TryGetResource(out Resource resource)
    {
        if (_resourcesAvailable.Count > 0)
        {
            resource = _resourcesAvailable.First();

            Lock(resource);

            return true;
        }

        resource = null;
        return false;
    }

    public void AddResourceHandler(Resource resource) =>    
        AddResource(resource);

    public void ReservationRemoveHandler(Resource resource) =>
        ReservationRemove(resource);

    private void ReservationRemove(Resource resource)
    {
        if (_resourcesReserved.Contains(resource))
        {
            Unlock(resource);
        }
        else
        {
            Debug.Log($"{resource} - нет в списке {nameof(_resourcesReserved)}");
        }
    }

    private void AddResource(Resource resource)
    {
        if (_resourcesAvailable.Contains(resource) == false &&
            _resourcesReserved.Contains(resource) == false)
        {
            _resourcesAvailable.Add(resource);
        }
    }

    private void Lock(Resource resource)
    {
        _resourcesAvailable.Remove(resource);
        _resourcesReserved.Add(resource);
    }

    private void Unlock(Resource resource) =>    
        _resourcesReserved.Remove(resource);    
}