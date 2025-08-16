using ManaxServer.Tasks;

namespace ManaxServer.Services.Task;

public interface ITaskService
{
    System.Threading.Tasks.Task AddTaskAsync(ITask task);
    void Shutdown();
}