using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectorBots.Scheduler
{
    public class TaskScheduler
    {
        private readonly List<Task> _tasks;
        private readonly List<Bot> _bots;

        public TaskScheduler(IEnumerable<Bot> bots)
        {
            _tasks = new List<Task>();
            _bots = new List<Bot>(bots);
        }

        public int PendingTasksCount => _tasks.Count;

        public void AddTask(Task task) =>        
            _tasks.Add(task);        

        public int AssignTasks()
        {
            RemoveInvalidTasks();

            int assignedTasksCount = 0;

            foreach (Bot bot in _bots)
            {
                if (bot == null || bot.IsBusy || _tasks.Count == 0)
                {
                    continue;
                }

                Task task = GetNextTask();

                if (task == null)
                {
                    break;
                }

                bot.SetTargetResource(task.Resource);
                assignedTasksCount++;
            }

            return assignedTasksCount;
        }

        public void AddBot(Bot bot)
        {
            _bots.Add(bot);
        }

        public void RemoveBot(Bot bot)
        {
            _bots.Remove(bot);
        }

        private Task GetNextTask()
        {
            RemoveInvalidTasks();

            if (_tasks.Count == 0)
            {
                return null;
            }

            Task task = _tasks
                .OrderBy(currentTask => currentTask.Distance)
                .First();

            _tasks.Remove(task);

            return task;
        }

        private void RemoveInvalidTasks()
        {
            _tasks.RemoveAll(task =>
                task == null ||
                task.Resource == null ||
                task.Resource.gameObject.activeInHierarchy == false);
        }
    }
}
