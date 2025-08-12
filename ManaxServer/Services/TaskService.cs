using ManaxLibrary.Logging;
using ManaxServer.Localization;
using ManaxServer.Tasks;

namespace ManaxServer.Services;

public class TaskService : Service
{
    private const int MaxTasks = 12;
    private readonly SortedSet<ITask> _waitingTasks = new(TaskPriorityComparer.Instance);
    private readonly List<(ITask Task, Task RunningTask)> _runningTasks = [];
    private readonly SemaphoreSlim _taskSemaphore = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public TaskService()
    {
        Task.Run(() => TaskLoopAsync(_cancellationTokenSource.Token));
        Task.Run(() => PublishTasks(_cancellationTokenSource.Token));
    }

    public async Task AddTaskAsync(ITask task)
    {
        await _taskSemaphore.WaitAsync();
        try
        {
            bool alreadyWaiting = _waitingTasks.Contains(task);
            bool alreadyRunning = _runningTasks.Any(rt => rt.Task.Equals(task));
            if (!alreadyWaiting && !alreadyRunning)
            {
                _waitingTasks.Add(task);
            }
        }
        finally
        {
            _taskSemaphore.Release();
        }
    }
    
    private async Task RemoveTaskAsync(Task runningTask, CancellationToken cancellationToken)
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

    private async Task TaskLoopAsync(CancellationToken cancellationToken)
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
                    Task runningTask = Task.Run(() =>
                    {
                        try
                        {
                            task.Execute();
                        }
                        catch(Exception e)
                        {
                            Logger.LogError(Localizer.Format("TaskError",task.GetName()), e, Environment.StackTrace);
                        }
                    }, cancellationToken);
                    _runningTasks.Add((task, runningTask));
                    _ = runningTask.ContinueWith(async _ => await RemoveTaskAsync(runningTask, cancellationToken), cancellationToken);
                }
            }
            finally
            {
                _taskSemaphore.Release();
            }
            await Task.Delay(100, cancellationToken);
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
                            ServicesManager.Notification.NotifyRunningTasksAsync(new Dictionary<string, int>());
                            cleaned = true;
                        }
                        continue;
                    }
                    Dictionary<string, int> tasks = _waitingTasks.GroupBy(t => t.GetName())
                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    ServicesManager.Notification.NotifyRunningTasksAsync(tasks);
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

    public void Shutdown()
    {
        _cancellationTokenSource.Cancel();
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
