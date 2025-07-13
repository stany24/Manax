using System.Security.Claims;
using ManaxLibrary.DTOs.User;
using ManaxServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManaxServer.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute(UserRole minRole) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string? authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        string token = authHeader["Bearer ".Length..].Trim();
        ClaimsPrincipal? principal = JwtService.ValidateToken(token);
        if (principal == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        context.HttpContext.User = principal;
        string? roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (roleClaim == null || !Enum.TryParse(roleClaim, out UserRole userRole) || userRole < minRole)
            context.Result = new ForbidResult();
    }
}