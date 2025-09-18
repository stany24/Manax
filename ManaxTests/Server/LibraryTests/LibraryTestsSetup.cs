using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.LibraryTests;

public abstract class LibraryTestsSetup
{
    private ManaxMapper _mapper = null!;
    private MockNotificationService _mockNotificationService = null!;
    protected ManaxContext Context = null!;
    protected LibraryController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new MockNotificationService();

        Controller = new LibraryController(Context, _mapper, _mockNotificationService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}