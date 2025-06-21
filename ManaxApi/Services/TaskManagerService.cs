using ManaxApi.Task;

namespace ManaxApi.Services;

public static class TaskManagerService
{
    private const int MaxTasks = 5;
    private static readonly List<ITask> WaitingTasks = [];
    private static readonly List<System.Threading.Tasks.Task> RunningTasks = [];

    static TaskManagerService()
    {
        System.Threading.Tasks.Task.Run(TaskLoop);
    }

    public static void AddTask(ITask task)
    {
        WaitingTasks.Add(task);
    }

    private static void TaskLoop()
    {
        while (true)
        {
            if (RunningTasks.Count < MaxTasks && WaitingTasks.Count > 0)
            {
                ITask task = WaitingTasks[0];
                WaitingTasks.RemoveAt(0);
                System.Threading.Tasks.Task runningTask = System.Threading.Tasks.Task.Run(() => task.Execute());
                RunningTasks.Add(runningTask);
                runningTask.ContinueWith(_ => { RunningTasks.Remove(runningTask); });
            }
            Thread.Sleep(100);
        }
    }

    public static Dictionary<string, int> GetTasks()
    {
        return WaitingTasks.GroupBy(e => e.GetName())
            .ToDictionary(g => g.Key, g => g.Count());
    }
}