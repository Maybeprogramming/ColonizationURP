using UnityEngine;

public class BaseFactory : MonoBehaviour
{
    [SerializeField] private Base _basePrefab;
    [SerializeField] private BotFactory _botFactoryTemplate;

    private int _baseNameID;

    private ResourcesData _resourcesData;
    private SpawnerResources _spawner;
    private BaseEventBinder _eventBinder;

    private void Awake()
    {
        _baseNameID = 0;
        _resourcesData = GameContext.ResourcesData;
        _spawner = GameContext.Spawner;
        _eventBinder = new BaseEventBinder(_resourcesData, _spawner);
    }

    public Base Spawn(Vector3 position)
    {
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
        ++_baseNameID;
        newBase.name = $"Base_{_baseNameID}";
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
}
