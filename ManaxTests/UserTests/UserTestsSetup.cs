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
    protected ManaxContext _context = null!;
    protected UserController _controller = null!;
    private ManaxMapper _mapper = null!;
    protected MockHashService _mockHashService = null!;
    private MockJwtService _mockJwtService = null!;
    protected Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockHashService = new MockHashService();
        _mockJwtService = new MockJwtService();
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new UserController(_context, _mapper, _mockHashService, _mockJwtService,
            _mockNotificationService.Object);

        ClaimsPrincipal adminUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Name, "TestAdmin"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
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
        SqliteTestDbContextFactory.CleanupTestDatabase(_context);
        _mockHashService.Reset();
    }
}