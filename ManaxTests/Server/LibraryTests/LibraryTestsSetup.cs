using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.LibraryTests;

public abstract class LibraryTestsSetup
{
    private MockNotificationService _mockNotificationService = null!;
    protected ManaxContext Context { get; private set; } = null!;
    protected LibraryController Controller { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mockNotificationService = new MockNotificationService();

        Controller = new LibraryController(Context, _mockNotificationService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}