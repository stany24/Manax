using ManaxLibrary.Logging;
using ManaxServer.Localization;
using ManaxServer.Services.Notification;
using ManaxServer.Tasks;

namespace ManaxServer.Services.BackgroundTask;

public class BackgroundTaskService(INotificationService notificationService) : Service, IBackgroundTaskService
{
    private const int MaxTasks = 6;

    private readonly List<(IBackGroundTask Task, Task RunningTask)> _runningTasks = [];
    private readonly SemaphoreSlim _taskSemaphore = new(1, 1);
    private readonly SortedSet<IBackGroundTask> _waitingTasks = new(TaskPriorityComparer.Instance);

    public async Task AddTaskAsync(IBackGroundTask backGroundTask)
    {
        await _taskSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            bool alreadyWaiting = _waitingTasks.Contains(backGroundTask);
            bool alreadyRunning = _runningTasks.Any(rt => rt.Task.Equals(backGroundTask));
            if (alreadyWaiting || alreadyRunning) return;

            _waitingTasks.Add(backGroundTask);
            Logger.LogInfo("Task added to waiting list " + backGroundTask.GetName());
        }
        finally
        {
            _taskSemaphore.Release();
        }

        await TryStartTasksAsync().ConfigureAwait(false);
        await PublishTasksAsync().ConfigureAwait(false);
    }

    ~BackgroundTaskService()
    {
        _taskSemaphore.Dispose();
    }

    private async Task TryStartTasksAsync()
    {
        while (true)
        {
            await _taskSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_runningTasks.Count >= MaxTasks) break;

                IBackGroundTask? backGroundTask = _waitingTasks.Min;
                if (backGroundTask == null) break;

                bool hasHigherPriorityRunning =
                    _runningTasks.Any(t => t.Task.GetPriority() < backGroundTask.GetPriority());
                if (hasHigherPriorityRunning) break;

                _waitingTasks.Remove(backGroundTask);
                Logger.LogInfo("Task started " + backGroundTask.GetName());

                Task runningTask = new(() =>
                {
                    try
                    {
                        backGroundTask.Execute();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(Localizer.TaskError(backGroundTask.GetName()), e, Environment.StackTrace);
                    }
                });

                _runningTasks.Add((backGroundTask, runningTask));

                _ = runningTask.ContinueWith(async t =>
                {
                    Logger.LogInfo("Task ended " + backGroundTask.GetName());
                    await RemoveRunningTaskAndTryStartAsync(t).ConfigureAwait(false);
                }, TaskContinuationOptions.RunContinuationsAsynchronously);

                runningTask.Start();
            }
            finally
            {
                _taskSemaphore.Release();
            }
        }
    }

    private async Task RemoveRunningTaskAndTryStartAsync(Task runningTask)
    {
        await _taskSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _runningTasks.RemoveAll(rt => rt.RunningTask == runningTask);
        }
        finally
        {
            _taskSemaphore.Release();
        }

        await TryStartTasksAsync().ConfigureAwait(false);
        await PublishTasksAsync().ConfigureAwait(false);
    }

    private async Task PublishTasksAsync()
    {
        await _taskSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            Dictionary<string, int> running = _runningTasks.GroupBy(rt => rt.Task.GetName())
                .ToDictionary(g => g.Key, g => g.Count());
            Dictionary<string, int> waiting = _waitingTasks.GroupBy(t => t.GetName())
                .ToDictionary(g => g.Key, g => g.Count());
            Dictionary<string, int> taskCounts = new(waiting);

            foreach (KeyValuePair<string, int> kvp in running)
                if (taskCounts.ContainsKey(kvp.Key))
                    taskCounts[kvp.Key] += kvp.Value;
                else
                    taskCounts[kvp.Key] = kvp.Value;

            notificationService.NotifyRunningTasksAsync(taskCounts);
        }
        finally
        {
            _taskSemaphore.Release();
        }
    }
}

public class TaskPriorityComparer : IComparer<IBackGroundTask>
{
    public static readonly TaskPriorityComparer Instance = new();

    public int Compare(IBackGroundTask? x, IBackGroundTask? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        int cmp = x.GetPriority().CompareTo(y.GetPriority());
        return cmp != 0 ? cmp : x.GetHashCode().CompareTo(y.GetHashCode());
    }
}