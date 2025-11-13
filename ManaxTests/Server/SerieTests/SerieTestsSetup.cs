using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.SerieTests;

public abstract class SerieTestsSetup
{
    private MockFixService _mockFixService = null!;
    private MockNotificationService _mockNotificationService = null!;
    private MockBackgroundTaskService _mockTaskService = null!;
    protected ManaxContext Context = null!;
    protected SerieController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mockNotificationService = new MockNotificationService();
        _mockFixService = new MockFixService();
        _mockTaskService = new MockBackgroundTaskService();

        Controller = new SerieController(Context, _mockNotificationService, _mockFixService, _mockTaskService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}