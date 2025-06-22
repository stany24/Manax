using ManaxApi.Task;

namespace ManaxApi.Services;

public static class TaskManagerService
{
    private const int MaxTasks = 12;
    private static readonly List<ITask> WaitingTasks = [];
    private static readonly Dictionary<string,int> TasksInfo = [];
    private static readonly List<System.Threading.Tasks.Task> RunningTasks = [];
    private static readonly SemaphoreSlim TaskSemaphore = new(1, 1);
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    static TaskManagerService()
    {
        System.Threading.Tasks.Task.Run(() => TaskLoopAsync(CancellationTokenSource.Token));
    }

    public static async System.Threading.Tasks.Task AddTaskAsync(ITask task)
    {
        await TaskSemaphore.WaitAsync();
        try
        {
            WaitingTasks.Add(task);
            if (!TasksInfo.ContainsKey(task.GetName())) 
            { 
                TasksInfo[task.GetName()] = 1; 
            }
            else 
            { 
                TasksInfo[task.GetName()]++; 
            }
        }
        finally
        {
            TaskSemaphore.Release();
        }
    }

    private static async System.Threading.Tasks.Task TaskLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await TaskSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (RunningTasks.Count < MaxTasks && WaitingTasks.Count > 0)
                {
                    ITask task = WaitingTasks[0];
                    WaitingTasks.RemoveAt(0);
                    
                    System.Threading.Tasks.Task runningTask = System.Threading.Tasks.Task.Run(async () => 
                    {
                        try
                        {
                            await System.Threading.Tasks.Task.Run(() => task.Execute(), cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // Log exception if needed
                            Console.WriteLine($"Task error: {ex.Message}");
                        }
                    }, cancellationToken);
                    
                    RunningTasks.Add(runningTask);
                    _ = runningTask.ContinueWith(async _ =>
                    {
                        await TaskSemaphore.WaitAsync(cancellationToken);
                        try
                        {
                            RunningTasks.Remove(runningTask);
                            if (!TasksInfo.ContainsKey(task.GetName())) return;
                            TasksInfo[task.GetName()]--;
                            if (TasksInfo[task.GetName()] <= 0)
                            {
                                TasksInfo.Remove(task.GetName());
                            }
                        }
                        finally
                        {
                            TaskSemaphore.Release();
                        }
                    }, cancellationToken);
                }
            }
            finally
            {
                TaskSemaphore.Release();
            }
            await System.Threading.Tasks.Task.Delay(100, cancellationToken);
        }
    }

    public static Dictionary<string, int> GetTasks()
    {
        Dictionary<string, int> tasksCopy = new();
        
        TaskSemaphore.Wait();
        try
        {
            foreach (KeyValuePair<string, int> kvp in TasksInfo)
            {
                tasksCopy[kvp.Key] = kvp.Value;
            }
        }
        finally
        {
            TaskSemaphore.Release();
        }
        
        return tasksCopy;
    }
    
    public static void Shutdown()
    {
        CancellationTokenSource.Cancel();
    }
}