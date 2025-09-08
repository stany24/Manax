using System.Security.Claims;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests.UserTests;

public abstract class UserTestsSetup
{
    protected ManaxContext Context = null!;
    protected UserController Controller = null!;
    private ManaxMapper _mapper = null!;
    protected MockHashService MockHashService = null!;
    private MockTokenService _mockTokenService = null!;
    protected Mock<INotificationService> MockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        MockHashService = new MockHashService();
        _mockTokenService = new MockTokenService();
        MockNotificationService = new Mock<INotificationService>();

        Controller = new UserController(Context, _mapper, MockHashService, _mockTokenService,
            MockNotificationService.Object);

        ClaimsPrincipal adminUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Name, "TestAdmin"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = adminUser
            }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
        MockHashService.Reset();
    }
}