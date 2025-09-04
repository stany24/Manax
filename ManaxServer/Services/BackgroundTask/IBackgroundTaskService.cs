using ManaxServer.Tasks;

namespace ManaxServer.Services.BackgroundTask;

public interface IBackgroundTaskService
{
    public Task AddTaskAsync(IBackGroundTask backGroundTask);
}