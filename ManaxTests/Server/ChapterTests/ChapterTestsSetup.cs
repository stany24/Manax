using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.ChapterTests;

public abstract class ChapterTestsSetup
{
    private ManaxMapper _mapper = null!;
    private MockNotificationService _mockNotificationService = null!;
    protected ManaxContext Context = null!;
    protected ChapterController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new MockNotificationService();

        Controller = new ChapterController(Context, _mapper, _mockNotificationService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}