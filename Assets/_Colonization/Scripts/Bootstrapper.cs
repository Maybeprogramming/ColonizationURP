using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private Base _base;
    [SerializeField] private ResourceWarhouse _counter;
    [SerializeField] private ResourceCounterView _counterView;
    [SerializeField] private SpawnerResources _spawner;
    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private ResourceScanner _resourceScanner;

    private void OnEnable()
    {
        _base.ResourceAdded += _counter.ResourceChangedHandler;
        _base.ResourceAdded += _spawner.ResourceReleasedHandler;
        _counter.Changed += _counterView.CountUpdateHandler;
        _resourceScanner.ResourceFound += _resourcesData.AddResourceHandler;
        _base.ResourceAdded += _resourcesData.ReservationRemoveHandler;
    }

    private void OnDisable()
    {
        _base.ResourceAdded -= _counter.ResourceChangedHandler;
        _base.ResourceAdded -= _spawner.ResourceReleasedHandler;
        _counter.Changed -= _counterView.CountUpdateHandler;
        _resourceScanner.ResourceFound -= _resourcesData.AddResourceHandler;
        _base.ResourceAdded -= _resourcesData.ReservationRemoveHandler;
    }
}