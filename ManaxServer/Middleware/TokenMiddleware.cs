using System.Net;
using System.Text.Json;
using ManaxLibrary.Logging;
using ManaxServer.Models;
using ManaxServer.Services.Token;

namespace ManaxServer.Middleware;

public class TokenMiddleware(
    RequestDelegate next,
    ITokenService tokenService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (tokenService.IsTokenRevoked(context.Request.Headers.Authorization.ToString()))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("The token has been revoked.");
            return;
        }
        await next(context);
    }
}