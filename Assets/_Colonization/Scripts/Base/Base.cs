using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollectorBots.Scheduler;

[RequireComponent(typeof(BaseStateMachine), typeof(ResourceWarhouse))]
public class Base : MonoBehaviour, IBase
{
    private const int BotSpawnCost = 3;
    private const int ExpandCost = 5;

    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private BaseStateMachine _stateMachine;
    [SerializeField] private ResourceWarhouse _warhouse;
    [SerializeField] private List<Bot> _bots = new List<Bot>();
    [SerializeField] private float _delayTime;
    [SerializeField] private BotFactory _botFactory;

    private Coroutine _working;
    private WaitForSeconds _wait;
    private TaskScheduler _taskScheduler;

    public event Action<Resource> ResourceAdded;
    public event Action NewBaseBuilt;

    public Vector3 Position => transform.position;
    public int ResourceCount => _warhouse.Count;
    public Vector3 FlagPosition { get; set; }

    [field: SerializeField] public bool HasConstractNewBase { get; set; }

    private void Awake()
    {
        _resourcesData ??= UnityEngine.Object.FindFirstObjectByType<ResourcesData>();
        _warhouse ??= GetComponent<ResourceWarhouse>();
        _stateMachine ??= GetComponent<BaseStateMachine>();
        _botFactory ??= GetComponentInChildren<BotFactory>() ?? UnityEngine.Object.FindFirstObjectByType<BotFactory>();
        _bots.RemoveAll(bot => bot == null);
    }

    private void Start()
    {
        _wait = new WaitForSeconds(_delayTime);
        _taskScheduler = new TaskScheduler(_bots);
        _working = StartCoroutine(Working());
        _stateMachine.Init(this);
    }

    private void OnDestroy() =>
        StopCoroutine(_working);

    public void TakeResource(Resource resource) =>
        OnResourceAdded(resource);

    public bool TrySpawnBot()
    {
        if (_warhouse.TrySpendResource(BotSpawnCost) == false)
            return false;

        _botFactory.Spawn();
        return true;
    }

    public bool TrySpendResources(int count) =>
        _warhouse.TrySpendResource(count);

    public void AddBot(Bot bot)
    {
        _bots.Add(bot);
        _taskScheduler?.AddBot(bot);
    }

    public void RemoveBot(Bot bot)
    {
        _bots.Remove(bot);
        _taskScheduler.RemoveBot(bot);
    }

    public void SetBotFactory(BotFactory factory) =>
        _botFactory = factory;

    public bool HasBot(Bot bot) =>
        _bots.Contains(bot);

    public int BotCount => _bots.Count;
    public bool HasBotOnConstructTask => _bots.Exists(bot => bot.HasConstructTask);

    public void ClearExpansionFlag()
    {
        HasConstractNewBase = false;
        NewBaseBuilt?.Invoke();
    }

    public Bot GetFreeBot() =>
        _bots.Find(bot => bot.IsBusy == false && bot.HasConstructTask == false);

    public void CancelConstructTasks()
    {
        for (int i = 0; i < _bots.Count; i++)
            _bots[i].HasConstructTask = false;
    }

    private void DoWork()
    {
        while (_resourcesData.TryGetResource(out Resource resource))
        {
            _taskScheduler.AddTask(new Task(resource, Position));
        }

        int assignedTasksCount = _taskScheduler.AssignTasks();

        if (_taskScheduler.PendingTasksCount > 0 && assignedTasksCount == 0)
        {
            Debug.Log("Нет свободных рабочих");
        }
    }

    private void OnResourceAdded(Resource resource) =>
        ResourceAdded?.Invoke(resource);

    private IEnumerator Working()
    {
        while (enabled)
        {
            yield return _wait;
            DoWork();
        }
    }
}
