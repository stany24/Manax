using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Server.Mocks;
using Moq;

namespace ManaxTests.Server.LibraryTests;

public abstract class LibraryTestsSetup
{
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    protected ManaxContext Context = null!;
    protected LibraryController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        Controller = new LibraryController(Context, _mapper, _mockNotificationService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}