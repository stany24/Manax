using System.Security.Claims;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.ReadTests;

public abstract class ReadTestsSetup:TestSetup
{
    protected ManaxContext Context { get; private set; } = null!;
    protected ReadController Controller { get; private set; } = null!;
    protected MockNotificationService MockNotificationService { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();
        MockNotificationService = new MockNotificationService();

        Controller = new ReadController(Context, MockNotificationService);

        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1")
        ], "test"));

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);
    }
}