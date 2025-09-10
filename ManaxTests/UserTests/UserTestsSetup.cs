using System.Security.Claims;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxServer.Services.Validation;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

public abstract class UserTestsSetup
{
    private ManaxMapper _mapper = null!;
    private MockTokenService _mockTokenService = null!;
    private MockPermissionService _mockPermissionService = null!;
    private INotificationService _mockNotificationService = null!;
    private IPasswordValidationService _mockPasswordValidationService = null!;
    protected ManaxContext Context = null!;
    protected UserController Controller = null!;
    protected MockHashService MockHashService = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        MockHashService = new MockHashService();
        _mockTokenService = new MockTokenService();
        _mockNotificationService = new MockNotificationService();
        _mockPermissionService = new MockPermissionService();
        _mockPasswordValidationService = new MockPasswordValidationService();

        Controller = new UserController(Context, _mapper, MockHashService, _mockTokenService,
            _mockNotificationService, _mockPermissionService, _mockPasswordValidationService);

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