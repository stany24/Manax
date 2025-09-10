using System.Net;
using System.Security.Claims;
using ManaxServer.Middleware;
using ManaxServer.Services.Token;

namespace ManaxTests.MiddlewareTests;

[TestClass]
public class BearerAuthenticationMiddlewareTests : BearerAuthenticationMiddlewareSetup
{
    [TestMethod]
    public async Task InvokeAsync_NoAuthorizationHeader_CallsNext()
    {
        BearerAuthenticationMiddleware middleware = new(Next, TokenService);

        await middleware.InvokeAsync(HttpContext);

        Assert.IsTrue(NextCalled);
        Assert.IsNull(HttpContext.User.Identity?.Name);
    }

    [TestMethod]
    public async Task InvokeAsync_EmptyAuthorizationHeader_CallsNext()
    {
        HttpContext.Request.Headers.Authorization = "";
        BearerAuthenticationMiddleware middleware = new(Next, TokenService);

        await middleware.InvokeAsync(HttpContext);

        Assert.IsTrue(NextCalled);
        Assert.IsNull(HttpContext.User.Identity?.Name);
    }

    [TestMethod]
    public async Task InvokeAsync_NonBearerToken_CallsNext()
    {
        HttpContext.Request.Headers.Authorization = "Basic dGVzdDp0ZXN0";
        BearerAuthenticationMiddleware middleware = new(Next, TokenService);

        await middleware.InvokeAsync(HttpContext);

        Assert.IsTrue(NextCalled);
        Assert.IsNull(HttpContext.User.Identity?.Name);
    }

    [TestMethod]
    public async Task InvokeAsync_RevokedToken_ReturnsUnauthorized()
    {
        const string token = "revoked-token";
        TokenService.RevokeToken(token);
        HttpContext.Request.Headers.Authorization = $"Bearer {token}";
        HttpContext.Response.Body = new MemoryStream();
        BearerAuthenticationMiddleware middleware = new(Next, TokenService);

        await middleware.InvokeAsync(HttpContext);

        Assert.IsFalse(NextCalled);
        Assert.AreEqual((int)HttpStatusCode.Unauthorized, HttpContext.Response.StatusCode);
        
        HttpContext.Response.Body.Position = 0;
        using StreamReader reader = new(HttpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        Assert.AreEqual("Token has been revoked", responseBody);
    }

    [TestMethod]
    public async Task InvokeAsync_InvalidToken_ReturnsUnauthorized()
    {
        string token = "invalid-token";
        HttpContext.Request.Headers.Authorization = $"Bearer {token}";
        HttpContext.Response.Body = new MemoryStream();
        BearerAuthenticationMiddleware middleware = new(Next, TokenService);

        await middleware.InvokeAsync(HttpContext);

        Assert.IsFalse(NextCalled);
        Assert.AreEqual((int)HttpStatusCode.Unauthorized, HttpContext.Response.StatusCode);
        
        HttpContext.Response.Body.Position = 0;
        using StreamReader reader = new(HttpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        Assert.AreEqual("Invalid or expired token", responseBody);
    }

    [TestMethod]
    public async Task InvokeAsync_ValidToken_SetsUserAndCallsNext()
    {
        TokenInfo tokenInfo = new()
        {
            UserId = 123,
            Username = "testuser",
            Expiry = DateTime.UtcNow.AddHours(1)
        };
        
        const string token = "valid-token";
        TokenService.AddToken(token, tokenInfo);
        HttpContext.Request.Headers.Authorization = $"Bearer {token}";
        BearerAuthenticationMiddleware middleware = new(Next, TokenService);

        await middleware.InvokeAsync(HttpContext);

        Assert.IsTrue(NextCalled);
        Assert.IsNotNull(HttpContext.User.Identity);
        Assert.IsTrue(HttpContext.User.Identity.IsAuthenticated);
        Assert.AreEqual("testuser", HttpContext.User.Identity.Name);
        Assert.AreEqual("123", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.AreEqual(tokenInfo, HttpContext.Items["TokenInfo"]);
        Assert.AreEqual(token, HttpContext.Items["BearerToken"]);
    }
    
}
