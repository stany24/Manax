using System.Security.Claims;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.ReadTests;

public abstract class ReadTestsSetup
{
    private ManaxMapper _mapper = null!;
    protected ManaxContext Context = null!;
    protected ReadController Controller = null!;
    protected MockNotificationService MockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        MockNotificationService = new MockNotificationService();

        Controller = new ReadController(Context, _mapper, MockNotificationService);

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