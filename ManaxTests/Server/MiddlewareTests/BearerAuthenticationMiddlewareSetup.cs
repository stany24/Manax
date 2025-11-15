using ManaxTests.Server.Mocks;
using Microsoft.AspNetCore.Http;

namespace ManaxTests.Server.MiddlewareTests;

public abstract class BearerAuthenticationMiddlewareSetup
{
    protected DefaultHttpContext HttpContext { get; private set; } = null!;
    protected RequestDelegate Next { get; private set; } = null!;
    protected bool NextCalled { get; private set; }
    protected MockTokenService TokenService { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        TokenService = new MockTokenService();
        HttpContext = new DefaultHttpContext();
        NextCalled = false;
        Next = _ =>
        {
            NextCalled = true;
            return Task.CompletedTask;
        };
    }
}