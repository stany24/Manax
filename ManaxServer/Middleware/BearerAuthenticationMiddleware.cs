using System.Security.Claims;
using ManaxServer.Services.Token;
using System.Net;

namespace ManaxServer.Middleware;

public class BearerAuthenticationMiddleware(RequestDelegate next, ITokenService tokenService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await next(context);
            return;
        }

        string token = authHeader["Bearer ".Length..].Trim();
        
        if (tokenService.IsTokenRevoked(token))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Token has been revoked");
            return;
        }

        TokenInfo? tokenInfo = tokenService.GetTokenInfo(token);
        if (tokenInfo == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid or expired token");
            return;
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, tokenInfo.UserId.ToString()),
            new(ClaimTypes.Name, tokenInfo.Username),
        ];

        ClaimsIdentity identity = new(claims, "Bearer");
        ClaimsPrincipal principal = new(identity);
        context.User = principal;

        context.Items["TokenInfo"] = tokenInfo;
        context.Items["BearerToken"] = token;

        await next(context);
    }
}
