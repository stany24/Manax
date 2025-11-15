using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.SerieTests;

public abstract class SerieTestsSetup
{
    private MockFixService _mockFixService = null!;
    private MockNotificationService _mockNotificationService = null!;
    private MockBackgroundTaskService _mockTaskService = null!;
    protected ManaxContext Context { get; private set; } = null!;
    protected SerieController Controller { get; private set; } = null!;

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