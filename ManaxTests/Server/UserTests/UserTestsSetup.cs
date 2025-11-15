using System.Security.Claims;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Validation;
using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.UserTests;

public abstract class UserTestsSetup
{
    private IPasswordValidationService _mockPasswordValidationService = null!;
    private MockPermissionService _mockPermissionService = null!;
    private MockTokenService _mockTokenService = null!;
    protected ManaxContext Context { get; private set; } = null!;
    protected UserController Controller { get; private set; } = null!;
    protected MockHashService MockHashService { get; private set; } = null!;
    protected MockNotificationService MockNotificationService { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        MockHashService = new MockHashService();
        _mockTokenService = new MockTokenService();
        MockNotificationService = new MockNotificationService();
        _mockPermissionService = new MockPermissionService();
        _mockPasswordValidationService = new MockPasswordValidationService();

        Controller = new UserController(Context, MockHashService, _mockTokenService,
            MockNotificationService, _mockPermissionService, _mockPasswordValidationService);

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