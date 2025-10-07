using ManaxServer.Services.BackgroundTask;
using ManaxServer.Tasks;

namespace ManaxTests.Server.Mocks;

public class MockBackgroundTaskService : IBackgroundTaskService
{
    public Task AddTaskAsync(IBackGroundTask backGroundTask)
    {
        return Task.CompletedTask;
    }
}