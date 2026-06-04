using UnityEngine;

public class BaseFactory : MonoBehaviour
{
    private const int BaseNameMin = 1000;
    private const int BaseNameMax = 9999;

    [SerializeField] private Base _basePrefab;
    [SerializeField] private BotFactory _botFactoryTemplate;

    private ResourcesData _resourcesData;
    private SpawnerResources _spawner;
    private BaseEventBinder _eventBinder;

    private void Awake()
    {
        _resourcesData = GameContext.ResourcesData;
        _spawner = GameContext.Spawner;

        if (_resourcesData == null)
            Debug.LogError($"{nameof(BaseFactory)} on '{name}': {nameof(_resourcesData)} is not assigned. Add {nameof(GameContext)} to scene.", this);

        if (_spawner == null)
            Debug.LogError($"{nameof(BaseFactory)} on '{name}': {nameof(_spawner)} is not assigned. Add {nameof(GameContext)} to scene.", this);

        if (_botFactoryTemplate == null)
            Debug.LogError($"{nameof(BaseFactory)} on '{name}': {nameof(_botFactoryTemplate)} is not assigned.", this);

        _eventBinder = new BaseEventBinder(_resourcesData, _spawner);
    }

    public Base Spawn(Vector3 position)
    {
        if (!HasDependency(_basePrefab, nameof(_basePrefab), string.Empty)) return null;
        if (!HasDependency(_botFactoryTemplate, nameof(_botFactoryTemplate), string.Empty)) return null;
        if (!HasDependency(_resourcesData, nameof(_resourcesData), $"Check {nameof(GameContext)}.")) return null;
        if (!HasDependency(_spawner, nameof(_spawner), $"Check {nameof(GameContext)}.")) return null;

        Vector3 spawnPosition = position;
        spawnPosition.y = 0f;

        Base newBase = InstantiateBase(spawnPosition);
        ConfigureBaseChildren(newBase);
        _eventBinder.Bind(newBase);

        return newBase;
    }

    private Base InstantiateBase(Vector3 spawnPosition)
    {
        Base newBase = Instantiate(_basePrefab, spawnPosition, Quaternion.identity);
        newBase.name = $"Base_{Random.Range(BaseNameMin, BaseNameMax)}";
        return newBase;
    }

    private void ConfigureBaseChildren(Base newBase)
    {
        Bot[] clonedBots = newBase.GetComponentsInChildren<Bot>();

        for (int i = 0; i < clonedBots.Length; i++)
            Destroy(clonedBots[i].gameObject);

        BotFactory existingBotFactory = newBase.GetComponentInChildren<BotFactory>();

        if (existingBotFactory != null && existingBotFactory.gameObject != _botFactoryTemplate.gameObject)
            Destroy(existingBotFactory.gameObject);

        BotFactory botFactory = Instantiate(_botFactoryTemplate, newBase.transform);
        newBase.SetBotFactory(botFactory);

        ResourceScanner scanner = newBase.GetComponentInChildren<ResourceScanner>();
        scanner?.gameObject.SetActive(true);
    }

    private bool HasDependency(Object dependency, string dependencyName, string contextHint)
    {
        if (dependency == null)
        {
            Debug.LogError($"{nameof(BaseFactory)} on '{name}': {dependencyName} is not assigned. {contextHint}", this);
            return false;
        }

        return true;
    }
}
