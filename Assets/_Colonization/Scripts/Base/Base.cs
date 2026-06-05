using System;
using System.Collections.Generic;
using UnityEngine;
using CollectorBots.Scheduler;

[RequireComponent(typeof(BaseStateMachine), typeof(ResourceWarhouse))]
public class Base : MonoBehaviour, IBase
{
    private const float DefaultWorkInterval = 1f;

    [SerializeField] private BaseStateMachine _stateMachine;
    [SerializeField] private ResourceWarhouse _warhouse;
    [SerializeField] private List<Bot> _bots = new List<Bot>();
    [SerializeField] private float _delayTime = DefaultWorkInterval;
    [SerializeField] private BotFactory _botFactory;

    private ResourcesData _resourcesData;
    private BaseFactory _baseFactory;
    private BotRoster _roster;
    private TaskScheduler _taskScheduler;
    private ExpansionProvider _expansionProvider;
    private BaseWorkLoop _workLoop;

    public Vector3 Position => transform.position;
    public int ResourceCount => _warhouse.Count;
    public Vector3 FlagPosition => _expansionProvider.FlagPosition;
    public bool HasConstructNewBase => _expansionProvider.HasConstructNewBase;
    public BaseFactory BaseFactory => _baseFactory;
    public int BotCount => _roster.Count;
    public bool HasBotOnConstructTask => _roster.HasOnConstructTask();

    public event Action<Resource> ResourceAdded;
    public event Action NewBaseBuilt
    {
        add => SubscribeToNewBaseBuilt(value);
        remove => UnsubscribeFromNewBaseBuilt(value);
    }

    private void Awake()
    {
        _warhouse ??= GetComponent<ResourceWarhouse>();
        _stateMachine ??= GetComponent<BaseStateMachine>();
        _botFactory ??= GetComponentInChildren<BotFactory>();

        _resourcesData = GameContext.ResourcesData;
        _baseFactory = GameContext.BaseFactory;

        _roster = new BotRoster(_bots);
        _roster.ClearNulls();
        _expansionProvider = new ExpansionProvider();
    }

    private void OnEnable()
    {
        if (_botFactory != null)
            _botFactory.BotCreated += OnBotCreated;
    }

    private void OnDisable()
    {
        if (_botFactory != null)
            _botFactory.BotCreated -= OnBotCreated;
    }

    private void Start()
    {
        _taskScheduler = new TaskScheduler(_roster.Bots);
        _workLoop = new BaseWorkLoop(this, _resourcesData, _taskScheduler, () => Position, _delayTime);
        _workLoop.Start();
        _stateMachine.Init(this);
    }

    private void OnDestroy() =>
        _workLoop?.Stop();

    public void TakeResource(Resource resource) =>
        OnResourceAdded(resource);

    public bool TrySpawnBot()
    {
        if (_warhouse.TrySpendResource(BaseOption.BotSpawnCost) == false)
            return false;

        _botFactory.Spawn();
        return true;
    }

    public bool TrySpendResources(int count) =>
        _warhouse.TrySpendResource(count);

    public void AddBot(Bot bot) =>
        _roster.Add(bot);

    public void RemoveBot(Bot bot) =>
        _roster.Remove(bot);

    public bool HasBot(Bot bot) =>
        _roster.Contains(bot);

    public Bot GetFreeBot() =>
        _roster.GetFreeBot();

    public void CancelConstructTasks() =>
        _roster.CancelConstructTasks();

    public void SetBotFactory(BotFactory factory)
    {
        if (_botFactory != null)
            _botFactory.BotCreated -= OnBotCreated;

        _botFactory = factory;
        
        if (_botFactory != null)
            _botFactory.BotCreated += OnBotCreated;
    }

    public void ClearExpansionFlag() =>
        _expansionProvider.ClearFlag();

    public void AssignExpansionFlag(Vector3 position) =>
        _expansionProvider.AssignFlag(position);

    public void CancelExpansion() =>
        _expansionProvider.Cancel();

    private void OnResourceAdded(Resource resource) =>
        ResourceAdded?.Invoke(resource);

    private void OnBotCreated(Bot bot)
    {
        bot.transform.position = transform.position + BaseOption.BotSpawnOffset;
        bot.Init(this, BaseFactory);
        AddBot(bot);
    }

    private void SubscribeToNewBaseBuilt(Action handler)
    {
        if (_expansionProvider != null)        
            _expansionProvider.NewBaseBuilt += handler;        
    }

    private void UnsubscribeFromNewBaseBuilt(Action handler)
    {
        if (_expansionProvider != null)        
            _expansionProvider.NewBaseBuilt -= handler;        
    }
}
