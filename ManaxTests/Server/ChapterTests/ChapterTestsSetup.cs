using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Server.Mocks;
using Moq;

namespace ManaxTests.Server.ChapterTests;

public abstract class ChapterTestsSetup
{
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    protected ManaxContext Context = null!;
    protected ChapterController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        Controller = new ChapterController(Context, _mapper, _mockNotificationService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}