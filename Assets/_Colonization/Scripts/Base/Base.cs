using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollectorBots.Scheduler;

public class Base : MonoBehaviour
{
    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private ResourceCounter _counter;
    [SerializeField] private List<Bot> _bots;
    [SerializeField] private float _delayTime;

    private Coroutine _working;
    private WaitForSeconds _wait;
    private TaskScheduler _taskScheduler;

    public event Action<Resource> ResourceAdded;

    public Vector3 Position => transform.position;

    private void Awake() =>    
        _counter ??= GetComponent<ResourceCounter>();    

    private void Start()
    {
        _wait = new WaitForSeconds(_delayTime);
        _taskScheduler = new TaskScheduler(_bots ?? new List<Bot>());
        _working = StartCoroutine(Working());
    }

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
}