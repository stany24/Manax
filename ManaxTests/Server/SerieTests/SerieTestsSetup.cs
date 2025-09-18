using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Server.Mocks;
using Moq;

namespace ManaxTests.Server.SerieTests;

public abstract class SerieTestsSetup
{
    private ManaxMapper _mapper = null!;
    private Mock<IFixService> _mockFixService = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IBackgroundTaskService> _mockTaskService = null!;
    protected ManaxContext Context = null!;
    protected SerieController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();
        _mockFixService = new Mock<IFixService>();
        _mockTaskService = new Mock<IBackgroundTaskService>();

        Controller = new SerieController(Context, _mapper, _mockNotificationService.Object, _mockFixService.Object,
            _mockTaskService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}