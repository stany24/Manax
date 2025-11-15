using System.Security.Claims;
using ManaxLibrary.DTO.Feature;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.IssueTypeTests;

public abstract class IssueTypeTestsSetup
{
    private MockFeatureService _mockFeatureService = null!;
    protected ManaxContext Context { get; private set; } = null!;
    protected IssueTypeController Controller { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mockFeatureService = new MockFeatureService();
        _mockFeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);
        _mockFeatureService.SetFeatureEnabled(FeatureType.ReportedIssues, true);

        Controller = new IssueTypeController(Context, _mockFeatureService);

        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1"),
            new Claim(ClaimTypes.Role, "User")
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