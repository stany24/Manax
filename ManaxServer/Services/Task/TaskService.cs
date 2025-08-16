using ManaxLibrary.Logging;
using ManaxServer.Localization;
using ManaxServer.Services.Notification;
using ManaxServer.Tasks;

namespace ManaxServer.Services.Task;

public class TaskService : Service, ITaskService
{
    private const int MaxTasks = 12;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly INotificationService _notificationService;
    private readonly List<(ITask Task, System.Threading.Tasks.Task RunningTask)> _runningTasks = [];
    private readonly SemaphoreSlim _taskSemaphore = new(1, 1);
    private readonly SortedSet<ITask> _waitingTasks = new(TaskPriorityComparer.Instance);

    public TaskService(INotificationService notificationService)
    {
        _notificationService = notificationService;
        System.Threading.Tasks.Task.Run(() => TaskLoopAsync(_cancellationTokenSource.Token));
        System.Threading.Tasks.Task.Run(() => PublishTasks(_cancellationTokenSource.Token));
    }

    public async System.Threading.Tasks.Task AddTaskAsync(ITask task)
    {
        await _taskSemaphore.WaitAsync();
        try
        {
            bool alreadyWaiting = _waitingTasks.Contains(task);
            bool alreadyRunning = _runningTasks.Any(rt => rt.Task.Equals(task));
            if (!alreadyWaiting && !alreadyRunning) _waitingTasks.Add(task);
        }
        finally
        {
            _taskSemaphore.Release();
        }
    }

    public void Shutdown()
    {
        _cancellationTokenSource.Cancel();
    }

    private async System.Threading.Tasks.Task RemoveTaskAsync(System.Threading.Tasks.Task runningTask,
        CancellationToken cancellationToken)
    {
        await _taskSemaphore.WaitAsync(cancellationToken);
        try
        {
            _runningTasks.RemoveAll(rt => rt.RunningTask == runningTask);
        }
        finally
        {
            _taskSemaphore.Release();
        }
    }

    private async System.Threading.Tasks.Task TaskLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _taskSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (_runningTasks.Count < MaxTasks && _waitingTasks.Count > 0)
                {
                    ITask? task = _waitingTasks.Min;
                    if (task is null) continue;
                    _waitingTasks.Remove(task);
                    System.Threading.Tasks.Task runningTask = System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            task.Execute();
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(Localizer.Format("TaskError", task.GetName()), e, Environment.StackTrace);
                        }
                    }, cancellationToken);
                    _runningTasks.Add((task, runningTask));
                    _ = runningTask.ContinueWith(async _ => await RemoveTaskAsync(runningTask, cancellationToken),
                        cancellationToken);
                }
            }
            finally
            {
                _taskSemaphore.Release();
            }

            await System.Threading.Tasks.Task.Delay(100, cancellationToken);
        }
    }

    private async void PublishTasks(CancellationToken cancellationToken)
    {
        try
        {
            bool cleaned = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(1000);
                await _taskSemaphore.WaitAsync(cancellationToken);
                try
                {
                    if (_waitingTasks.Count == 0)
                    {
                        if (!cleaned)
                        {
                            _notificationService.NotifyRunningTasksAsync(new Dictionary<string, int>());
                            cleaned = true;
                        }

                        continue;
                    }

                    Dictionary<string, int> tasks = _waitingTasks.GroupBy(t => t.GetName())
                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    _notificationService.NotifyRunningTasksAsync(tasks);
                    cleaned = false;
                }
                finally
                {
                    _taskSemaphore.Release();
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Error in TaskManagerService PublishTasks", e, Environment.StackTrace);
        }
    }
}

public class TaskPriorityComparer : IComparer<ITask>
{
    public static readonly TaskPriorityComparer Instance = new();

    public int Compare(ITask? x, ITask? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        int cmp = x.GetPriority().CompareTo(y.GetPriority());
        if (cmp != 0) return cmp;
        return x.GetHashCode().CompareTo(y.GetHashCode());
    }
}