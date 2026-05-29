using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private Base _base;
    [SerializeField] private ResourceCounter _counter;
    [SerializeField] private ResourceCounterView _counterView;
    [SerializeField] private SpawnerResources _spawner;
    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private ResourceScanner _resourceScanner;

    private void OnEnable()
    {
        _base.ResourceAdded += _counter.ResourceAddedHandler;
        _base.ResourceAdded += _spawner.ResourceReleasedHandler;
        _counter.Added += _counterView.CountUpdateHandler;
        _resourceScanner.ResourceFound += _resourcesData.AddResourceHandler;
        _base.ResourceAdded += _resourcesData.ReservationRemoveHandler;
    }

    private void OnDisable()
    {
        _base.ResourceAdded -= _counter.ResourceAddedHandler;
        _base.ResourceAdded -= _spawner.ResourceReleasedHandler;
        _counter.Added -= _counterView.CountUpdateHandler;
        _resourceScanner.ResourceFound -= _resourcesData.AddResourceHandler;
        _base.ResourceAdded -= _resourcesData.ReservationRemoveHandler;
    }
}