using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.ChapterTests;

public abstract class ChapterTestsSetup : TestSetup
{
    private MockNotificationService _mockNotificationService = null!;
    protected ManaxContext Context { get; private set; } = null!;
    protected ChapterController Controller { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();
        _mockNotificationService = new MockNotificationService();
        Controller = new ChapterController(Context, _mockNotificationService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}