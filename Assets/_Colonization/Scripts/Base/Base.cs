using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollectorBots.Scheduler;

[RequireComponent (typeof(BaseStateMachine), typeof(ResourceWarhouse))]
public class Base : MonoBehaviour, IBase
{
    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private BaseStateMachine _stateMachine;
    [SerializeField] private ResourceWarhouse _warhouse;
    [SerializeField] private List<Bot> _bots;
    [SerializeField] private float _delayTime;

    private Coroutine _working;
    private WaitForSeconds _wait;
    private TaskScheduler _taskScheduler;

    public event Action<Resource> ResourceAdded;

    public Vector3 Position => transform.position;
    public int ResourceCount => _warhouse.Count;
    [field: SerializeField] public bool HasConstractNewBase { get; set; } //заглушка для реализациии 2 части проекта

    private void Awake() 
    {
        _warhouse ??= GetComponent<ResourceWarhouse>();
        _stateMachine ??= GetComponent<BaseStateMachine>();
    }

    private void Start()
    {
        _wait = new WaitForSeconds(_delayTime);
        _taskScheduler = new TaskScheduler(_bots ?? new List<Bot>());
        _working = StartCoroutine(Working());
        _stateMachine.Init(this);
    }

    private void OnDestroy() =>
        StopCoroutine(_working);

    public void TakeResource(Resource resource) =>
        OnResourceAdded(resource);

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

    public void AddBot(Bot bot)
    {
        _bots.Add(bot);
        _taskScheduler.AddBot(bot);
    }
}