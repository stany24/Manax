using ManaxServer.Tasks;

namespace ManaxServer.Services.BackgroundTask;

public interface IBackgroundTaskService
{
    public void AddTask(IBackGroundTask backGroundTask);
}