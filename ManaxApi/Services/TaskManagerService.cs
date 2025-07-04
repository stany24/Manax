using ManaxApi.BackgroundTask;

namespace ManaxApi.Services;

public static class TaskManagerService
{
    private const int MaxTasks = 12;
    private static readonly SortedSet<ITask> WaitingTasks = new(TaskPriorityComparer.Instance);
    private static readonly List<(ITask Task, Task RunningTask)> RunningTasks = [];
    private static readonly SemaphoreSlim TaskSemaphore = new(1, 1);
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    static TaskManagerService()
    {
        Task.Run(() => TaskLoopAsync(CancellationTokenSource.Token));
    }

    public static async Task AddTaskAsync(ITask task)
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
    
    private static async Task RemoveTaskAsync(Task runningTask, CancellationToken cancellationToken)
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

    private static async Task TaskLoopAsync(CancellationToken cancellationToken)
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
                        catch
                        {
                            Console.WriteLine("Error executing task: " + task.GetName());
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
    
    public static Dictionary<string, int> GetTasks()
    {
        return WaitingTasks.GroupBy(t => t.GetName())
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static void Shutdown()
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
        int cmp = y.GetPriority().CompareTo(x.GetPriority());
        if (cmp != 0) return cmp;
        return x.GetHashCode().CompareTo(y.GetHashCode());
    }
}
