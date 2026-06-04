using System.Collections;
using UnityEngine;
using CollectorBots.Scheduler;

public class BaseWorkLoop
{
    private readonly MonoBehaviour _coroutineHost;
    private readonly ResourcesData _resourcesData;
    private readonly TaskScheduler _taskScheduler;
    private readonly System.Func<Vector3> _positionProvider;
    private readonly float _interval;

    private WaitForSeconds _wait;
    private Coroutine _running;

    public BaseWorkLoop(MonoBehaviour coroutineHost, ResourcesData resourcesData, TaskScheduler taskScheduler, System.Func<Vector3> positionProvider, float interval)
    {
        _coroutineHost = coroutineHost;
        _resourcesData = resourcesData;
        _taskScheduler = taskScheduler;
        _positionProvider = positionProvider;
        _interval = interval;
    }

    public void Start()
    {
        _wait = new WaitForSeconds(_interval);
        _running = _coroutineHost.StartCoroutine(Working());
    }

    public void Stop()
    {
        if (_running != null)
            _coroutineHost.StopCoroutine(_running);
    }

    private IEnumerator Working()
    {
        while (_coroutineHost.isActiveAndEnabled)
        {
            yield return _wait;
            DoWork();
        }
    }

    private void DoWork()
    {
        while (_resourcesData.TryGetResource(out Resource resource))
        {
            _taskScheduler.AddTask(new Task(resource, _positionProvider()));
        }

        int assignedTasksCount = _taskScheduler.AssignTasks();

        if (_taskScheduler.PendingTasksCount > 0 && assignedTasksCount == 0)
        {
            Debug.Log("Нет свободных рабочих");
        }
    }
}
