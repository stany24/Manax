using System.Security.Claims;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.IssueTests;

public abstract class IssueTestsSetup
{
    private ManaxMapper _mapper = null!;
    private MockNotificationService _mockNotificationService = null!;
    protected ManaxContext Context = null!;
    protected IssueController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new MockNotificationService();

        Controller = new IssueController(Context, _mapper, _mockNotificationService);

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