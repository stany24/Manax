using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;

namespace ManaxTests.MiddlewareTests;

public abstract class BearerAuthenticationMiddlewareSetup
{
    protected DefaultHttpContext HttpContext = null!;
    protected RequestDelegate Next = null!;
    protected bool NextCalled;
    protected MockTokenService TokenService = null!;

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