using UnityEngine;

public class BaseEventBinder
{
    private readonly ResourcesData _resourcesData;
    private readonly SpawnerResources _spawner;

    public BaseEventBinder(ResourcesData resourcesData, SpawnerResources spawner)
    {
        _resourcesData = resourcesData;
        _spawner = spawner;
    }

    public void Bind(Base targetBase)
    {
        if (targetBase == null)
            return;

        ResourceWarhouse warhouse = targetBase.GetComponent<ResourceWarhouse>();

        if (warhouse == null)
            return;

        ResourceScanner scanner = targetBase.GetComponentInChildren<ResourceScanner>();
        ResourceCounterView counterView = targetBase.GetComponentInChildren<ResourceCounterView>();

        if (scanner != null)
            scanner.ResourceFound += _resourcesData.AddResourceHandler;

        targetBase.ResourceAdded += warhouse.ResourceChangedHandler;
        targetBase.ResourceAdded += _spawner.ResourceReleasedHandler;
        targetBase.ResourceAdded += _resourcesData.ReservationRemoveHandler;

        if (counterView != null)
            warhouse.Changed += counterView.CountUpdateHandler;
    }

    public void Unbind(Base targetBase)
    {
        if (targetBase == null)
            return;

        ResourceWarhouse warhouse = targetBase.GetComponent<ResourceWarhouse>();

        if (warhouse == null)
            return;

        ResourceScanner scanner = targetBase.GetComponentInChildren<ResourceScanner>();
        ResourceCounterView counterView = targetBase.GetComponentInChildren<ResourceCounterView>();

        if (scanner != null)
            scanner.ResourceFound -= _resourcesData.AddResourceHandler;

        targetBase.ResourceAdded -= warhouse.ResourceChangedHandler;
        targetBase.ResourceAdded -= _spawner.ResourceReleasedHandler;
        targetBase.ResourceAdded -= _resourcesData.ReservationRemoveHandler;

        if (counterView != null)
            warhouse.Changed -= counterView.CountUpdateHandler;
    }
}
