using ManaxLibrary.Logging;
using ManaxServer.Localization;
using ManaxServer.Tasks;

namespace ManaxServer.Services;

public class TaskService : Service
{
    private const int MaxTasks = 12;
    private readonly SortedSet<ITask> WaitingTasks = new(TaskPriorityComparer.Instance);
    private readonly List<(ITask Task, Task RunningTask)> RunningTasks = [];
    private readonly SemaphoreSlim TaskSemaphore = new(1, 1);
    private readonly CancellationTokenSource CancellationTokenSource = new();

    public TaskService()
    {
        Task.Run(() => TaskLoopAsync(CancellationTokenSource.Token));
        Task.Run(() => PublishTasks(CancellationTokenSource.Token));
    }

    public async Task AddTaskAsync(ITask task)
    {
        await TaskSemaphore.WaitAsync();
        try
        {
            bool alreadyWaiting = WaitingTasks.Contains(task);
            bool alreadyRunning = RunningTasks.Any(rt => rt.Task.Equals(task));
            if (!alreadyWaiting && !alreadyRunning)
            {
                WaitingTasks.Add(task);
            }
        }
        finally
        {
            TaskSemaphore.Release();
        }
    }
    
    private async Task RemoveTaskAsync(Task runningTask, CancellationToken cancellationToken)
    {
        await TaskSemaphore.WaitAsync(cancellationToken);
        try
        {
            RunningTasks.RemoveAll(rt => rt.RunningTask == runningTask);
        }
        finally
        {
            TaskSemaphore.Release();
        }
    }

    private async Task TaskLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await TaskSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (RunningTasks.Count < MaxTasks && WaitingTasks.Count > 0)
                {
                    ITask? task = WaitingTasks.Min;
                    if (task is null) continue;
                    WaitingTasks.Remove(task);
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
                    RunningTasks.Add((task, runningTask));
                    _ = runningTask.ContinueWith(async _ => await RemoveTaskAsync(runningTask, cancellationToken), cancellationToken);
                }
            }
            finally
            {
                TaskSemaphore.Release();
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
                await TaskSemaphore.WaitAsync(cancellationToken);
                try
                {
                    if (WaitingTasks.Count == 0)
                    {
                        if (!cleaned)
                        {
                            ServicesManager.Notification.NotifyRunningTasksAsync(new Dictionary<string, int>());
                            cleaned = true;
                        }
                        continue;
                    }
                    Dictionary<string, int> tasks = WaitingTasks.GroupBy(t => t.GetName())
                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    ServicesManager.Notification.NotifyRunningTasksAsync(tasks);
                    cleaned = false;
                }
                finally
                {
                    TaskSemaphore.Release();
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
        CancellationTokenSource.Cancel();
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
