using UnityEngine;

public class BaseFactory : MonoBehaviour
{
    [SerializeField] private Base _basePrefab;
    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private SpawnerResources _spawner;
    [SerializeField] private BotFactory _botFactoryTemplate;

    public Base Spawn(Vector3 position, Base previousBase)
    {
        if (_basePrefab == null)
            _basePrefab = Object.FindFirstObjectByType<Base>();

        if (_botFactoryTemplate == null)
            _botFactoryTemplate = Object.FindFirstObjectByType<BotFactory>();

        if (_resourcesData == null)
            _resourcesData = Object.FindFirstObjectByType<ResourcesData>();

        if (_spawner == null)
            _spawner = Object.FindFirstObjectByType<SpawnerResources>();

        Vector3 spawnPosition = position;
        spawnPosition.y = 0f;

        Base newBase = Instantiate(_basePrefab, spawnPosition, Quaternion.identity);

        newBase.name = $"Base_{Random.Range(1000, 9999)}";

        Bot[] clonedBots = newBase.GetComponentsInChildren<Bot>();

        for (int i = 0; i < clonedBots.Length; i++)
            Destroy(clonedBots[i].gameObject);

        BotFactory botFactory = Instantiate(_botFactoryTemplate, newBase.transform);
        botFactory.Initialize(newBase);
        newBase.SetBotFactory(botFactory);

        ResourceScanner scanner = newBase.GetComponentInChildren<ResourceScanner>();
        ResourceWarhouse warhouse = newBase.GetComponent<ResourceWarhouse>();

        ResourceCounterView counterView = newBase.GetComponentInChildren<ResourceCounterView>();

        if (scanner != null)
            scanner.ResourceFound += _resourcesData.AddResourceHandler;

        if (warhouse != null)
        {
            newBase.ResourceAdded += warhouse.ResourceChangedHandler;
            newBase.ResourceAdded += _spawner.ResourceReleasedHandler;
            newBase.ResourceAdded += _resourcesData.ReservationRemoveHandler;

            if (counterView != null)
                warhouse.Changed += counterView.CountUpdateHandler;
        }

        scanner?.gameObject.SetActive(true);

        return newBase;
    }
}
