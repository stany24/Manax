using System.Security.Claims;
using ManaxLibrary.DTO.Feature;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.RankTests;

public abstract class RankTestsSetup:TestSetup
{
    private MockFeatureService _mockFeatureService = null!;
    private MockNotificationService _mockNotificationService = null!;
    protected ManaxContext Context { get; private set; } = null!;
    protected RankController Controller { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mockNotificationService = new MockNotificationService();
        _mockFeatureService = new MockFeatureService();
        _mockFeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        Controller = new RankController(Context, _mockNotificationService);

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