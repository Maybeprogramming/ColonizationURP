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
        BotFactory botFactory = Object.FindFirstObjectByType<BotFactory>();

        if (botFactory != null)
            botFactory.Initialize(_base);

        _base.ResourceAdded += _counter.ResourceChangedHandler;
        _base.ResourceAdded += _spawner.ResourceReleasedHandler;
        _base.ResourceAdded += _resourcesData.ReservationRemoveHandler;
        _counter.Changed += _counterView.CountUpdateHandler;
        _resourceScanner.ResourceFound += _resourcesData.AddResourceHandler;

        if (GetComponent<FlagPlacer>() == null)
            gameObject.AddComponent<FlagPlacer>();
    }

    private void OnDisable()
    {
        _base.ResourceAdded -= _counter.ResourceChangedHandler;
        _base.ResourceAdded -= _spawner.ResourceReleasedHandler;
        _base.ResourceAdded -= _resourcesData.ReservationRemoveHandler;
        _counter.Changed -= _counterView.CountUpdateHandler;
        _resourceScanner.ResourceFound -= _resourcesData.AddResourceHandler;
    }
}
